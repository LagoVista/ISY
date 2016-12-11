$scriptPath = Split-Path $MyInvocation.MyCommand.Path
Set-Location $scriptPath

& ${env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild /property:Configuration=Release /property:Platform=x86
& ${env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild /property:Configuration=Release /property:Platform=x64
& ${env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild /property:Configuration=Release /property:Platform=ARM
& ${env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild /property:Configuration=Release 