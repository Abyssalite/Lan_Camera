#!/bin/bash
set -e

export USB_CAM=0
export MAIN_IP=192.168.77.1

echo "Starting camera server..."
nohup python3 ~/usb-camera/camera.py &
