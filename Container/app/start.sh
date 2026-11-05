#!/bin/bash
set -e

export USB_IP=192.168.77.2

echo "Starting MediaMTX..."
mediamtx /app/mediamtx.yml &
sleep 1

echo "Starting camera server..."
exec python3 /app/camera_server.py
