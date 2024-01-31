## Features

ToothPick is a simple service that provides an easy-to-use web application as a wrapper for YT-DLP functionalities.

See https://github.com/yt-dlp/yt-dlp for information on supported features. 

## Requirements

If not installing via docker, ffmpeg and yt-dlp are required. 

I recommend grabbing the latest release directly from GitHub so that ToothPick can keep it updated to the nightly version:

```bash
wget https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp &&\
  mv yt-dlp /usr/local/bin &&\
  chmod -R 755 /usr/local/bin/yt-dlp &&\
  /usr/local/bin/yt-dlp --update-to nightly
```

## Installation

You can either install it via docker, or compile it yourself with .NET 8 sdk.

Here's an example of a simple docker-compose.yml you can use:
```yml
version: "3"

volumes:
  data:
  cookies:

services:
  toothpick:
    image: mattmckenzy/toothpick:latest
    container_name: toothpick
    hostname: toothpick
    environment:
      - TZ=America/Toronto
      - ASPNETCORE_URLS=http://+:8121
      - CULTURE=en-CA
    volumes:
      - data:/ToothPick/data
      - cookies:/ToothPick/Cookies
      - /mnt/media:/ToothPick/Media
      - /etc/localtime:/etc/localtime:ro
      - /etc/timezone:/etc/timezone:ro
      - /dev/shm:/dev/shm
    restart: always
    
networks:
  default:
    name: toothpick-net
    external: true
```

I serve the web application behind an nginx reverse proxy with this configuration:
```nginx
server {
    listen 443 ssl;
    listen [::]:443 ssl;

    allow 192.168.0.0/24;
    deny all;

    server_name toothpick.*;

    include /etc/nginx/ssl.conf;

    client_max_body_size 0;

    location / {
        include /etc/nginx/proxy.conf;
        include /etc/nginx/resolver.conf;
        set $upstream_app toothpick;
        set $upstream_port 8121;
        set $upstream_proto http;
        proxy_pass $upstream_proto://$upstream_app:$upstream_port;
    }
}
```


## Settings

This settings page has some important configuration options and are all well described. I suggest giving them a quick read before you start using it!

## Known Issues

Nothing at the moment.

## Future Work

Nothing Planned.

## Release Notes

### 1.0.0

Initial release of ToothPick.