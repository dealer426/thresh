$ErrorActionPreference = 'Stop'

$packageName = 'thresh'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

# Remove from PATH if needed (chocolatey handles this automatically for exe files in tools dir)
# Clean up is automatic for zip-based packages
