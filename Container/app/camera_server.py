#"!", "video/x-raw,width=1280,height=720,format=NV12,framerate=30/1",
import subprocess
import signal
import sys
import requests
from flask import Flask, request, jsonify
import os

app = Flask(__name__)

BACK  = r"\\_SB_.PCI0.I2C3.CAMR"
FRONT = r"\\_SB_.PCI0.I2C2.CAMF"
usb_camera = = os.environ['USB_IP']
is_night = False

current_cam = None
gst_process = None

def stop_camera():
    global gst_process, current_cam
    try:
        requests.get(f"http://{usb_camera}:5001/switch?cam=none", timeout=1)
    except Exception:
        print("Failed to stop center camera")
    #is_night = False

    if gst_process is not None:
        if gst_process.poll() is None:
            print("Stopping GStreamer and releasing camera...")

            gst_process.terminate()  # Sends SIGTERM
            try:
                gst_process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                gst_process.kill()
                gst_process.wait(timeout=2)

    gst_process = None
    current_cam = None
    print("Camera stopped.")

def start_camera(cam_name: str, night: str):
    global gst_process, current_cam, is_night , BACK, FRONT

    stop_camera()
    pipeline = None
    is_night = night == "true"

    if cam_name == BACK:
        pipeline = [
            "gst-launch-1.0", "-e",
            "libcamerasrc", f"camera-name={BACK}",
            "!", "videoconvert",
            "!", "videobalance", f"brightness={0.22 if not is_night else 0.8}", f"contrast={1.4 if not is_night else 2.0}", f"saturation={1.9 if not is_night else 2.0}",
            "!", "gamma", f"gamma={0.9 if not is_night else 0.2}",
            "!", "videoflip", "video-direction=180",
            "!", "videoconvert", "!", "video/x-raw",
            "!", "queue", "max-size-buffers=3", "leaky=downstream",
            "!", "x264enc",
                "tune=zerolatency",
                "speed-preset=veryfast",
                "bitrate=3500",
                "key-int-max=30",
                "bframes=0",
                "byte-stream=true",
                "threads=4",
            "!", "h264parse", "config-interval=1",
            "!", "rtspclientsink",
                "location=rtsp://127.0.0.1:8554/cam",
                "protocols=tcp",
                "latency=0",
                "do-rtsp-keep-alive=true"
        ]
        current_cam = cam_name
    elif cam_name == FRONT:
        pipeline = [
            "gst-launch-1.0", "-e",
            "libcamerasrc", f"camera-name={FRONT}",
            "!", "videoconvert",
            "!", "videobalance", f"brightness={0.02 if not is_night else 0.8}", f"contrast={1.2 if not is_night else 2.0}", f"saturation={1.5 if not is_night else 2.0}",
            "!", "gamma", f"gamma={0.9 if not is_night else 0.2}",
            "!", "videoconvert", "!", "video/x-raw",
            "!", "queue", "max-size-buffers=3", "leaky=downstream",
            "!", "x264enc",
                "tune=zerolatency",
                "speed-preset=veryfast",
                "bitrate=3500",
                "key-int-max=30",
                "bframes=0",
                "byte-stream=true",
                "threads=4",
            "!", "h264parse", "config-interval=1",
            "!", "rtspclientsink",
                "location=rtsp://127.0.0.1:8554/cam",
                "protocols=tcp",
                "latency=0",
                "do-rtsp-keep-alive=true"
        ]
        current_cam = cam_name
    else: return

    print(f"Starting camera: {cam_name}")
    gst_process = subprocess.Popen(pipeline)

def is_cam_published():
    try:
        r = requests.get("http://127.0.0.1:9997/v3/paths/get/cam", timeout=1)
        if r.status_code != 200:
            return False
        return bool(r.json().get("ready"))
    except Exception:
        return False

def switch_center(night: str):
    global current_cam, is_night
    stop_camera()
    is_night = night == "true"
    try:
        r = requests.get(f"http://{usb_camera}:5001/switch?cam=center&night={night}", timeout=1)
        if r.status_code != 200:
            current_cam = "none"
            return "error"
        current_cam = r.json().get("Camera")
        return r.json().get("Status") 
    except Exception:
        print("Failed to start center camera")
        return "error"

@app.route("/switch", methods=["GET", "POST"])
def switch_camera():
    cam = request.args.get("cam", "").lower()
    night = request.args.get("night", "false").lower()

    if cam == "back":
        start_camera(BACK, night)
        status =  "ok"
    elif cam == "front":
        start_camera(FRONT, night)
        status =  "ok"
    elif cam == "center":
        status = switch_center(night)
    elif cam == "none":
        stop_camera()
        status =  "ok"
    else:
        return jsonify({"Status": "error", "Camera": "use ?cam=back, ?cam=front or ?cam=none"}), 400

    return jsonify({"Status": status, "Camera": cam})

@app.route("/status", methods=["GET"])
def status():
    global current_cam, is_night

    if current_cam == BACK:
        cam = "back"
    elif current_cam == FRONT:
        cam = "front"
    elif current_cam == "center":
        cam = "center"
    else:
        cam = "none"

    if cam == "center":
        try:
            r = requests.get(f"http://{usb_camera}:5001/status", timeout=1)
            if r.status_code != 200:
                process_running = False
            process_running = bool(r.json().get("Running"))
        except Exception:
            process_running = False
    else:
        process_running = gst_process is not None and gst_process.poll() is None

    published = is_cam_published()
    return jsonify({
	"Night": is_night,
        "Current": cam,
        "Running": process_running,
        "Published": published,
        "Ready": process_running and published
    })

def cleanup(sig, frame):
    if gst_process and gst_process.poll() is None:
        gst_process.terminate()
    sys.exit(0)

signal.signal(signal.SIGINT, cleanup)
signal.signal(signal.SIGTERM, cleanup)

if __name__ == "__main__":
    print("Camera server ready")
    app.run(host="0.0.0.0", port=5000)
