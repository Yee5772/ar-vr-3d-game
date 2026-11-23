$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$unityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.6.8f1\Editor\Unity.exe'
$arguments = '-batchmode -projectPath "' + $projectRoot + '" -executeMethod SkyMeadow.Editor.RefinedModelInstaller.BuildInstalled -quit -logFile "' + $projectRoot + '\Build.log"'
$buildProcess = Start-Process -FilePath $unityEditor -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
if ($buildProcess.ExitCode -ne 0) { throw ('Unity build failed. See Build.log. Exit code: ' + $buildProcess.ExitCode) }
'Build completed: ' + $projectRoot + '\Builds\Windows\SkyMeadow.exe'
