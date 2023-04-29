# Creating The Installer

1. Download & Install [NSIS](https://nsis.sourceforge.io/Main_Page)


2. Download the dependencies and place them into [deps/](../install/deps/)
    * [AtomicParsley.exe](http://atomicparsley.sourceforge.net/)
    * [aria2c](https://github.com/aria2/aria2/releases/latest)
    * [ffmpeg.exe / ffprobe.exe](https://www.ffmpeg.org/download.html#build-windows)
    * [vcredist_x86.exe](https://www.microsoft.com/en-us/download/details.aspx?id=26999)
    * [windowsdesktop-runtime-6.0.8-win-x64.exe](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
    * [yt-dlp.exe](https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe)


3. Build the main JackTheVideoRipper project for Release


4. Run the Publish Configuration to compile and package the project into a single exe file


5. Build the JackCrashHandler project for Release


6. Copy the resulting [JackCrashHandler.dll](../src/JackCrashHandler/bin/Release/net6.0-windows/JackCrashHandler.dll) & [JackCrashHandler.exe](../src/JackCrashHandler/bin/Release/net6.0-windows/JackCrashHandler.exe) into the [deps/](../install/deps/) folder


7. Increment the version in the following files
    * [installer.nsi](../install/installer.nsi)
    * [version file](../version)
    * \<AssemblyInfo\> / \<FileInfo\> tags within [JackTheVideoRipper.csproj](../src/JackTheVideoRipper/JackTheVideoRipper.csproj)


8. Use NSIS to compile `installer.nsi` (right click, Compile with NSIS)


9. After the process completes it should give you a success message / sound


10. The Installer should be generated within [install/](../install/)
