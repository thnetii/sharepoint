$ErrorActionPreference = [System.Management.Automation.ActionPreference]::Stop

$config = Get-Content -Path (Join-Path $PSScriptRoot "configuration.json") | ConvertFrom-Json
[uri]$SiteUri = $config.SiteUrl

$TfmFolder = "netcoreapp3.1"
if ($PSVersionTable.PSEdition -ine "Core") {
    $TfmFolder = "net45"
}

Import-Module -Verbose -ErrorAction "Stop" ([System.IO.Path]::Combine($PSScriptRoot, "..", "publish", "THNETII.SharePoint.PowerShell", "Debug", $TfmFolder, "THNETII.SharePoint.PowerShell.psm1"))
[THNETII.SharePoint.IdentityModel.SharePointAuthorizationDiscoveryMetadata]`
$Discovery = Get-SPAuthDiscovery -SiteUri $SiteUri

$Discovery | Format-List *

[Microsoft.Identity.Client.PublicClientApplicationBuilder]$MsalBuilder = `
    $Discovery | New-SPAuthMsalPublicClientApplicationBuilder `
    -ClientId $config.MsalClientId
[Microsoft.Identity.Client.IPublicClientApplication]$MsalApp = `
    $MsalBuilder.Build()

$MsalApp.AppConfig | Format-List *
