$ErrorActionPreference = 'Stop'

$packageName = 'thresh'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url64 = 'https://github.com/dealer426/thresh/releases/download/v1.5.0/thresh-windows-x64.zip'
$checksum64 = 'deed76e07698f5e152b0e250eb5626af5b00381f13136e8877d95b4b5e5f35f1'
$checksumType64 = 'sha256'

$packageArgs = @{
  packageName    = $packageName
  unzipLocation  = $toolsDir
  url64bit       = $url64
  checksum64     = $checksum64
  checksumType64 = $checksumType64
}

Install-ChocolateyZipPackage @packageArgs
