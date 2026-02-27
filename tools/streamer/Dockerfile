FROM alpine:latest

RUN apk add --no-cache \
	ffmpeg \
	python3

RUN mkdir -p /app && chown 2000:2000 /app

USER 2000

COPY --chown=2000:2000 start.sh /app/start.sh
COPY --chown=2000:2000 index.html /app/index.html
RUN chmod +x /app/start.sh

EXPOSE 8080
CMD ["/app/start.sh"]
