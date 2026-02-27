docker run ^
-v C:\sources\private\StreamSharp\tools\streamer\media:/media ^
-e INPUT_FILE="/media/Tonnano - S01E01 - Wesh Am I Bulgarian WEBDL-1080p.mkv" ^
-e VIDEO_CODEC=copy ^
-e AUDIO_CODEC=copy ^
-e PRESET=veryfast ^
-e BITRATE=2500k ^
-e RESOLUTION=1280x720 ^
-p 8080:8080 ^
media-hls
