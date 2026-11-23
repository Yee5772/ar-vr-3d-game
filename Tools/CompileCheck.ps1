$ErrorActionPreference = 'Stop'
$unityData = 'C:\Program Files\Unity\Hub\Editor\6000.6.8f1\Editor\Data'
$projectRoot = Split-Path $PSScriptRoot -Parent
$lines = @('/target:library', '/nologo', '/langversion:latest', '/nostdlib+', ('/out:"' + $PSScriptRoot + '\CompileCheck.dll"'))
$lines += Get-ChildItem "$unityData\NetStandard\ref\2.1.0" -Filter *.dll | ForEach-Object { '/reference:"' + $_.FullName + '"' }
$lines += Get-ChildItem "$unityData\NetStandard\compat\2.1.0\shims\netstandard" -Filter *.dll | ForEach-Object { '/reference:"' + $_.FullName + '"' }
$lines += Get-ChildItem "$unityData\Managed\UnityEngine" -Filter *.dll | Where-Object { $_.Name -ne 'UnityEditor.dll' } | ForEach-Object { '/reference:"' + $_.FullName + '"' }
$lines += Get-ChildItem "$unityData\Managed" -Filter 'UnityEditor*.dll' | Where-Object { $_.Name -ne 'UnityEditor.dll' } | ForEach-Object { '/reference:"' + $_.FullName + '"' }
$lines += Get-ChildItem "$projectRoot\Assets" -Recurse -Filter *.cs | ForEach-Object { '"' + $_.FullName + '"' }
$responsePath = Join-Path $PSScriptRoot 'compile.rsp'
[System.IO.File]::WriteAllLines($responsePath, $lines)
& "$unityData\NetCoreRuntime\dotnet.exe" "$unityData\DotNetSdk\sdk\8.0.318\Roslyn\bincore\csc.dll" "@$responsePath"
if ($LASTEXITCODE -ne 0) { throw 'C# compilation failed.' }
'PASS: C# compiled against the installed Unity 6.6 engine and editor assemblies.'
