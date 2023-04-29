# JackTheVideoRipper

#### Download videos and audio from YouTube and hundreds of more streaming providers. Designed and developed for Windows 10.

![](https://github.com/fromanan/JackTheVideoRipper/raw/master/docs/demo.gif)

<hr>

## Features

* Download video and/or audio with wide selection of supported formats (`mp4`, `mkv`, `mov`, `avi`, `m4v` to name a few)


* Extract URLs from any document type or YouTube playlist and add URLs to queue for batch download


* 100+ supported streaming providers such as YouTube, DailyMotion, Vimeo, and many of the common ones


* Compressing media files, this works especially well with low quality videos that have been encoded with high
  bit-rates, or those that have been upscaled.


* Repairing and recoding broken video files


* Verifying integrity of media files

<hr>

## How Does It Work?

`JackTheVideoRipper` is a GUI that manages and automates powerful tools under-the-hood to provide a streamlined,
turn-key, point-and-click experience in order to download video, audio, or playlists in a friendly and easy way.
The true power lies within included command-line tools `youtube-dl` and `ffmpeg` which do the heavy lifting of
extracting and/or transcoding the media.

<hr>

## Requirements

* Windows 10/11 (x64)

<hr>

## Setup & Installation

### Downloading

Download the latest version in the [release](https://github.com/fromanan/JackTheVideoRipper/releases) section.

For the bleeding edge features, at the cost of potential stability issues and more bugs, choose the `Experimental`
release

Otherwise, choose the `Latest Stable` as it is the most well tested and covers all of the features

### Installing

Run the provided installer from the downloaded package, this will get all the files you will need to run.

When installing the Microsoft Visual Studio C++ Redistributables / .NET Runtime, it may say there is an error
because a newer version is already present / installed on your machine. You may safely cancel/exit this prompt.
It will not affect the main installer.

After the installation wizard finishes, you should be all good to go! Startup the JackTheVideoRipper app from your
Start Menu to get going.

### Running

The application should open up within a few seconds and will check for updates in the background.

If the application doesn't open within a few seconds, please check that your Windows installation is up-to-date as the
program relies on the latest .NET packages to run. You should also ensure that your Antivirus software hasn't blocked
it, as Windows is quite strict on the programs that it allows to run, particularly with Open-Source developers.

Once the main window is open, you can use the buttons/toolbar to download media or drag-and-drop urls and files to
compress or download media. You can also copy-paste while on the interface to the same effect.

### Bundled Dependencies

All required dependencies are installed, updated, and automatically kept up-to-date for you.

* [vcredist-x86](https://www.microsoft.com/en-us/download/confirmation.aspx?id=5555)
* [yt-dlp](https://github.com/yt-dlp/yt-dlp)
* [ffmpeg](https://www.ffmpeg.org/download.html#build-windows)
* [AtomicParsley](http://atomicparsley.sourceforge.net)

<hr>

## Building from Source

It is recommended that you first
[download and run the Windows Installer](https://github.com/fromanan/JackTheVideoRipper/releases), so that all of the
needed dependencies are installed correctly, before you download the source code, build, and run the application. 

The application looks for the dependencies in the same place regardless of if it is built from source or from the
installer. If the dependencies are not installed when the application runs, the application will crash or not behave
correctly.

<hr>

## Contributing

I am currently the only developer on this project and work on it in my free time. I use this project almost every day
when I am messing around with media as I like to tinker in my spare time.

I am not opposed to help though! If you find an issue please submit a bug via GitHub issues or if you would like to
become a contributor please feel free to email me at [fromanan@pm.me](mailto:fromanan@pm.me)

<hr>

## FAQ

##### 1. <i>What streaming providers are supported?</i>

A list of support services is available within the app; navigate to the menu and go to Help > About. `yt-dlp` will
dictate which services are supported.

##### 2. <i>The downloaded file doesn't play on my computer, what's wrong?</i>

Try using a different media player. The best free media player out there is
[VLC Player](https://www.videolan.org/vlc/index.html). Install and use that to ensure you can playback all desired
media files.

##### 3. <i>I want to convert the media to another format, how do I do that?</i>

The best free easy-to-use GUI tool is [Handbrake](https://handbrake.fr/).

##### 4. <i>Feature X isn't working correctly, why not?</i>

This app is currently in pre-release and not everything will work correctly. Please standby for further updates.

##### 5. <i>Why is `yt-dlp` not bundled but rather downloaded when Windows Installer runs?</i>

This has to do with `yt-dlp`'s features and functionality. `yt-dlp` is constantly evolving and updating to make sure
it is working with the latest changes made by streaming providers. Because of the ever changing nature bundling the 
current version of `yt-dlp` doesn't make sense. It will likely be out of date already by the time a user runs the
installer. Most other software, like bundled apps `vcredist-x86` or `ffmpeg`, are extremely stable and do not need
to be altered frequently to work correctly.

##### 6. <i>Why are you using `yt-dlp` over `youtube-dl`?</i>

`youtube-dl` is no longer being actively maintained, in fact, most popular video services (particularly YouTube) have
begun to block it. The last release, as of this writing, was in December of 2021. `yt-dlp` is an excellent fork of
`youtube-dl` with lots of updates and bug fixes.

<hr>

## Acknowledgements

#### 1. DanTheMan213 : For starting the initial project which I worked from

#### 2. The developers / maintainers of FFMPEG, Aria2C, & yt-dlp