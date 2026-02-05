$ErrorActionPreference = 'Stop'

$packageName = 'thresh'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url64 = 'https://github.com/dealer426/eknova/releases/download/v1.0.0/thresh-windows-x64.zip'
$checksum64 = '<TO_BE_UPDATED_AFTER_RELEASE>'
$checksumType64 = 'sha256'

$packageArgs = @{
  packageName    = $packageName
  unzipLocation  = $toolsDir
  url64bit       = $url64
  checksum64     = $checksum64
  checksumType64 = $checksumType64
}

Install-ChocolateyZipPackage @packageArgs
