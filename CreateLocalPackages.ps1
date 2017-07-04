$scriptPath = Split-Path $MyInvocation.MyCommand.Path

Set-Location $scriptPath

. ./BuildAll.ps1

. ./UpdateNuspecVersion.ps1 -preRelease alpha -major 1 -minor 2

$oldChildren = gci $scriptPath  *.nupkg
foreach( $oldChild in $oldChildren){
	"Removed " + $oldChild
	Remove-Item $oldChild -Recurse
}

$children = gci $scriptPath -recurse *.nuspec

$scriptPath

foreach( $child in $children){
	$projectPath = Split-Path $child.FullName
	nuget pack $child.FullName
	"------------------------------------------------------"
	"   "
}

$children = gci $scriptpath *.nupkg

# Add this key via control panel environment variables [NUGETAPIKEY] to be obtained from the LagoVista Nuget Account

foreach( $child in $children){
	Write-Output "Publishing Output $child.fullName" 
	nuget push $child.fullName -Source https://www.nuget.org/api/v2/package -ApiKey $env:NUGETAPIKEY
}

#$oldChildren = gci $scriptPath  *.nupkg
#foreach( $oldChild in $oldChildren){
#	"Removed " + $oldChild
#	Remove-Item $oldChild -Recurse
#}
