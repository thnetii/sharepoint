Import-Module (Join-Path $PSScriptRoot "THNETII.Msal.PowerShell.psm1")
Add-Type -LiteralPath (Join-Path $PSScriptRoot "Microsoft.SharePoint.Client.dll")
Add-Type -LiteralPath (Join-Path $PSScriptRoot "Microsoft.IdentityModel.Protocols.dll")
Add-Type -LiteralPath (Join-Path $PSScriptRoot "THNETII.SharePoint.IdentityModel.dll")

[System.Collections.Generic.IDictionary[string,Microsoft.IdentityModel.Protocols.IConfigurationManager[THNETII.SharePoint.IdentityModel.SharePointAuthorizationDiscoveryMetadata]]] `
$Script:SPAuthDiscoveryManagers = New-Object "System.Collections.Generic.Dictionary[string,Microsoft.IdentityModel.Protocols.IConfigurationManager[THNETII.SharePoint.IdentityModel.SharePointAuthorizationDiscoveryMetadata]]" `
    -ArgumentList @([System.StringComparer]::OrdinalIgnoreCase)

function Get-SPAuthDiscoveryManager {
    [CmdletBinding()]
    [OutputType([Microsoft.IdentityModel.Protocols.IConfigurationManager[THNETII.SharePoint.IdentityModel.SharePointAuthorizationDiscoveryMetadata]])]
    param (
        [Parameter(Mandatory=$true, Position=0)]
        [ValidateNotNull()]
        [uri]$SiteUri
    )

    $ProbeUri = New-Object System.UriBuilder $SiteUri
    if (-not $ProbeUri.Path.EndsWith("/")) {
        $ProbeUri.Path += "/"
    }
    $ProbeUri.Path += "_vti_bin/client.svc"
    [string]$ProbeUrl = $ProbeUri.Uri
    [Microsoft.IdentityModel.Protocols.IConfigurationManager[THNETII.SharePoint.IdentityModel.SharePointAuthorizationDiscoveryMetadata]] `
    $Manager = $null

    [switch]$Found = $Script:SPAuthDiscoveryManagers.TryGetValue($ProbeUrl, [ref] $Manager)
    if ($Found) {
        return $Manager
    }

    [THNETII.SharePoint.IdentityModel.HttpWwwAuthenticateHeaderParameterRetriever] `
    $HttpRetriever = New-Object THNETII.SharePoint.IdentityModel.HttpWwwAuthenticateHeaderParameterRetriever
    [THNETII.SharePoint.IdentityModel.SharePointAuthorizationDiscoveryRetriever] `
    $MetaRetriever = New-Object THNETII.SharePoint.IdentityModel.SharePointAuthorizationDiscoveryRetriever
    $Manager = New-Object "Microsoft.IdentityModel.Protocols.ConfigurationManager[THNETII.SharePoint.IdentityModel.SharePointAuthorizationDiscoveryMetadata]" `
        -ArgumentList @([string]$ProbeUrl,
            [Microsoft.IdentityModel.Protocols.IConfigurationRetriever[THNETII.SharePoint.IdentityModel.SharePointAuthorizationDiscoveryMetadata]]$MetaRetriever,
            [Microsoft.IdentityModel.Protocols.IDocumentRetriever]$HttpRetriever)
    $Script:SPAuthDiscoveryManagers[$ProbeUrl] = $Manager
    return $Manager
}

function Get-SPAuthDiscovery {
    [CmdletBinding()]
    [OutputType([THNETII.SharePoint.IdentityModel.SharePointAuthorizationDiscoveryMetadata])]
    param (
        [Parameter(Mandatory=$true, Position=0)]
        [ValidateNotNull()]
        [uri]$SiteUri,
        [switch]$Force
    )

    $Manager = Get-SPAuthDiscoveryManager -SiteUri $SiteUri
    if ($Force) {
        $Manager.RequestRefresh()
    }
    $Manager.GetConfigurationAsync([System.Threading.CancellationToken]::None
        ).GetAwaiter().GetResult()
}
