#!/bin/sh

INPUT_FILE=${INPUT_FILE:-/media/media.mp4}
VIDEO_CODEC=${VIDEO_CODEC:-libx264}
AUDIO_CODEC=${AUDIO_CODEC:-aac}
BITRATE=${BITRATE:-2500k}
RESOLUTION=${RESOLUTION:-1280x720}
PRESET=${PRESET:-fast}

OUTPUT_DIR=/app/hls
mkdir -p $OUTPUT_DIR
cp /app/index.html $OUTPUT_DIR/index.html
if [ ! -f "$INPUT_FILE" ]; then
	echo "Input file not found: $INPUT_FILE"
	exit 1
fi

ffmpeg -i "$INPUT_FILE" \
	-c:v $VIDEO_CODEC \
	-c:a $AUDIO_CODEC \
	-b:v $BITRATE \
	-s $RESOLUTION \
	-hls_time 10 \
	-hls_list_size 0 \
	-hls_segment_filename "$OUTPUT_DIR/segment_%03d.ts" \
	-f hls \
	"$OUTPUT_DIR/stream.m3u8" &

python3 -m http.server 8080 --directory $OUTPUT_DIR
