import subprocess
import signal
import sys
import requests
from flask import Flask, request, jsonify
import os

app = Flask(__name__)

CENTER = f"/dev/video{os.environ['USB_CAM']}"
main_camera = os.environ['MAIN_IP']

current_cam = None
gst_process = None

def stop_camera():
    global gst_process, current_cam

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

def start_camera(cam_name: str, is_night: bool):
    global gst_process, current_cam, CENTER

    stop_camera()
    pipeline = None

    if cam_name == CENTER:
        pipeline = [
            "gst-launch-1.0", "-e",
            "v4l2src", f"device={CENTER}",
            "!", "image/jpeg", "!", "jpegdec",
            "!", "videoconvert",
            "!", "videobalance", f"brightness={0.02 if not is_night else 0.6}", f"contrast={1.1 if not is_night else 1.8}", f"saturation={1.1 if not is_night else 1.8}",
            "!", "gamma", f"gamma={0.9 if not is_night else 0.2}",
            "!", "videoconvert",
            "!", "queue", "max-size-buffers=3", "leaky=downstream",
            "!", "x264enc",
                "tune=zerolatency",
                "speed-preset=veryfast",
                "bitrate=2000",
                "key-int-max=30",
                "bframes=0",
                "byte-stream=true",
                "threads=4",
            "!", "h264parse", "config-interval=1",
            "!", "rtspclientsink",
                f"location=rtsp://{main_camera}:8554/cam",
                "protocols=tcp",
                "latency=0",
                "do-rtsp-keep-alive=true"
        ]
        current_cam = cam_name
    else: return

    print(f"Starting camera: {cam_name}")
    gst_process = subprocess.Popen(pipeline)

@app.route("/switch", methods=["GET", "POST"])
def switch_camera():
    cam = request.args.get("cam", "").lower()
    night = request.args.get("night", "false").lower()

    if cam == "center":
        start_camera(CENTER, night == "true")
        status =  "ok"
    elif cam == "none":
        stop_camera()
        status =  "ok"
    else:
        return jsonify({"Status": "error", "Camera": "use ?cam=center or ?cam=none"}), 400

    return jsonify({"Status": status, "Camera": cam})

@app.route("/status", methods=["GET"])
def status():
    if current_cam == CENTER:
        cam = "center"
    else:
        cam = "none"

    process_running = gst_process is not None and gst_process.poll() is None

    return jsonify({
        "Current": cam,
        "Running": process_running
    })

def cleanup(sig, frame):
    if gst_process and gst_process.poll() is None:
        gst_process.terminate()
    sys.exit(0)

signal.signal(signal.SIGINT, cleanup)
signal.signal(signal.SIGTERM, cleanup)

if __name__ == "__main__":
    print("Camera server ready")
    app.run(host="0.0.0.0", port=5001)
