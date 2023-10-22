$versionFilePath = "version";
$installerFilePath = "install\installer.nsi";
$projectFilePath = "src\Application\Application.csproj";

$currentVersion = Get-Content ${versionFilePath};

$currentVersion = $currentVersion.TrimStart("v");

$newVersion = Read-Host "Enter new version number (current = ${currentVersion})";

(Get-Content ${versionFilePath}).replace(${currentVersion}, ${newVersion}) | Set-Content ${versionFilePath};

if (!$?) {
    Write-Host "Failed to update file '${versionFilePath}'";
    EXIT 1;
}

(Get-Content ${installerFilePath}).replace(${currentVersion}, ${newVersion}) | Set-Content ${installerFilePath};

if (!$?) {
    Write-Host "Failed to update file '${installerFilePath}'";
    EXIT 1;
}

(Get-Content ${projectFilePath}).replace(${currentVersion}, ${newVersion}) | Set-Content ${projectFilePath};

if (!$?) {
    Write-Host "Failed to update file '${projectFilePath}'";
    EXIT 1;
}

Write-Host "Success!";

Write-Host -NoNewLine "Press any key to continue...";
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown");
EXIT 0;