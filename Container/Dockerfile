FROM ubuntu:24.04

RUN apt-get update && apt-get install -y --no-install-recommends \
    python3 \
    python3-gi \
    python3-flask \
    python3-requests \
    gir1.2-gstreamer-1.0 \
    gstreamer1.0-libcamera \
    gstreamer1.0-tools \
    libcamera-tools \
    gstreamer1.0-plugins-base \
    gstreamer1.0-plugins-good \
    gstreamer1.0-plugins-bad \
    gstreamer1.0-plugins-ugly \
    gstreamer1.0-rtsp \
    x264 libx264-164 \
    v4l-utils \
    curl \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*
    
# Install MediaMTX
RUN curl -L -o /tmp/mediamtx.tar.gz \
    https://github.com/bluenviron/mediamtx/releases/download/v1.9.3/mediamtx_v1.9.3_linux_amd64.tar.gz \
    && tar -xzf /tmp/mediamtx.tar.gz -C /usr/local/bin mediamtx \
    && rm /tmp/mediamtx.tar.gz

WORKDIR /app

COPY app .

CMD ["/app/start.sh"]
