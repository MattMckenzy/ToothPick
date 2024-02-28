[<img alt="GitHub Release" src="https://img.shields.io/github/v/release/mattmckenzy/toothpick?logo=github&color=forestgreen">](https://github.com/MattMckenzy/ToothPick)
[<img alt="Docker Release" src="https://img.shields.io/docker/v/mattmckenzy/toothpick/latest?logo=docker">](https://hub.docker.com/r/mattmckenzy/toothpick)

# ToothPick

## Features

ToothPick is a simple service that provides an easy-to-use web application as a wrapper for YT-DLP functionalities.

It'll let you define Libraries, Series and Locations to automatically fetch media playlist pages and download new, untracked media.

Media will be stored in the defined output path under a Library/Series/Media.mp4 structure.

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


## Pages

Here's a quick overview of the web application's pages and it's functionalities.

### Home Page

On this page, you can find the following information:
* The current processing status, either the time until next processing or the current processed locations count.
* A list of current downloads or download errors, if any.
* A list of all log messages.

This page can also let you cancel processing, cancel the delay until next processing, stop a download, stop a download and save its tracking and delete log messages.

![Home Page](https://github.com/MattMckenzy/ToothPick/blob/main/Resources/Images/HomePage.png?raw=true)


### Libraries Page

This page is where you can define libraries. Libraries are the top-level entity in ToothPick and can contain many Series. They offer a convenient way to group different types of Series.

![Home Page](https://github.com/MattMckenzy/ToothPick/blob/main/Resources/Images/LibrariesPage.png?raw=true)


### Series Page

This page is where you can define Series. Series can group together many locations, but they will still store all relevant media under the parent series. You can also define some image URLs to populate the series folder with images that media servers such as Jellyfin will automatically use to decorate the series.

![Series Page](https://github.com/MattMckenzy/ToothPick/blob/main/Resources/Images/SeriesPage.png?raw=true)


### Locations Page

This page is where you can define Locations. Locations have fields that will directly affect how yt-dlp will fetch media within playlists to track and download. Here's a quick explanation of the relevant fields:

* Url: the url of the playlist or individual media page.
* Fetch Count: how many playlist items will be fetched to compare with currently tracked media and download if new ones are found.
* Reverse Fetch: reverses the playlist fetch direction, will fetch playlist items starting from the bottom of the page/playlist.
* Match Filters: filters added to the yt-dlp "--match-filters" option. The possible fields are shown here: https://github.com/yt-dlp/yt-dlp?tab=readme-ov-file#output-template amd the format is shown here: https://github.com/yt-dlp/yt-dlp?tab=readme-ov-file#filtering-formats. I.e. "language=en-US"
* Download Format: optional download format to specify when downloading media at this location. See here: https://github.com/yt-dlp/yt-dlp?tab=readme-ov-file#filtering-formats.
* Cookies: the name of the cookies file used for this location, cookies files can be uploaded to the ToothPick service in the management page.

![Locations Page](https://github.com/MattMckenzy/ToothPick/blob/main/Resources/Images/LocationsPage.png?raw=true)


### Media Page

The media page lists all the media that is currently tracked by ToothPick. Media who's URL is found here will not be downloaded if its information is fetched again in a playlist. If you wish to download a media again, simply delete its entry on this page, and once a location fetches it again it will download once more.

![Media Page](https://github.com/MattMckenzy/ToothPick/blob/main/Resources/Images/MediaPage.png?raw=true)


### Settings Page

The settings page has some important configuration options and are all well described. I suggest giving them a quick read before you start using ToothPick!

Some settings can help you define when new series' media should be downloaded or simply tracked, how many media items to fetch at a time by default, and even integrate logs with Gotify to get notifications anywhere.

![Settings Page](https://github.com/MattMckenzy/ToothPick/blob/main/Resources/Images/SettingsPage.png?raw=true)


### Management Page

This page can be used to export and import relevant entities as well as upload new cookies text files. Useful functions might find its way here as well.

![Management Page](https://github.com/MattMckenzy/ToothPick/blob/main/Resources/Images/ManagementPage.png?raw=true)


## Known Issues

Nothing at the moment.


## Future Work

Nothing Planned.


## Release Notes

### 1.1.3

- Fixed dirty check for banner location in series component.
