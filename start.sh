#!/bin/bash
set -e

APP_DIR="$(cd "$(dirname "$0")" && pwd)"
VLC_DIR="$APP_DIR/libvlc"

export LD_LIBRARY_PATH="$VLC_DIR"
export VLC_PLUGIN_PATH="$VLC_DIR/plugins"

exec dotnet run --project "$APP_DIR/Lan_Camera.Desktop" -f net10.0 "$@"