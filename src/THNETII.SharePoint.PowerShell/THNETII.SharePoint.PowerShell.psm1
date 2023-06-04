Import-Module (Join-Path $PSScriptRoot "THNETII.Msal.PowerShell.psm1")
Add-Type -LiteralPath (Join-Path $PSScriptRoot "Microsoft.SharePoint.Client.dll")
Add-Type -LiteralPath (Join-Path $PSScriptRoot "Microsoft.IdentityModel.Protocols.dll")
Add-Type -LiteralPath (Join-Path $PSScriptRoot "THNETII.SharePoint.BearerAuthorization.dll")
Add-Type -LiteralPath (Join-Path $PSScriptRoot "THNETII.SharePoint.AzureAcs.Protocol.dll")
Add-Type -LiteralPath (Join-Path $PSScriptRoot "System.IdentityModel.Tokens.Jwt.dll")

[System.Collections.Generic.IDictionary[string,Microsoft.IdentityModel.Protocols.IConfigurationManager[THNETII.SharePoint.BearerAuthorization.SharePointSiteAuthorizationMetadata]]] `
$Script:SPAuthDiscoveryManagers = New-Object "System.Collections.Generic.Dictionary[string,Microsoft.IdentityModel.Protocols.IConfigurationManager[THNETII.SharePoint.BearerAuthorization.SharePointSiteAuthorizationMetadata]]" `
    -ArgumentList @([System.StringComparer]::OrdinalIgnoreCase)

function Get-SPAuthDiscoveryManager {
    [CmdletBinding()]
    [OutputType([Microsoft.IdentityModel.Protocols.IConfigurationManager[THNETII.SharePoint.BearerAuthorization.SharePointSiteAuthorizationMetadata]])]
    param (
        [Parameter(Mandatory=$true, Position=0)]
        [ValidateNotNull()]
        [uri]$SiteUri
    )

    [Microsoft.IdentityModel.Protocols.IConfigurationManager[THNETII.SharePoint.BearerAuthorization.SharePointSiteAuthorizationMetadata]]`
    $Manager = $null
    [switch]$Found = $Script:SPAuthDiscoveryManagers.TryGetValue($SiteUri, [ref] $Manager)
    if ($Found) {
        return $Manager
    }

    $Manager = New-Object "THNETII.SharePoint.BearerAuthorization.SharePointSiteAuthorizationManager" ([string]$SiteUri)
    $Script:SPAuthDiscoveryManagers[$SiteUri] = $Manager
    return $Manager
}

function Get-SPAuthDiscovery {
    [CmdletBinding()]
    [OutputType([THNETII.SharePoint.BearerAuthorization.SharePointSiteAuthorizationMetadata])]
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

function New-SPAuthMsalPublicClientApplicationBuilder {
    [CmdletBinding(DefaultParameterSetName="BySiteUri")]
    [OutputType([Microsoft.Identity.Client.PublicClientApplicationBuilder])]
    param (
        [Parameter(ParameterSetName="BySiteUri", Mandatory=$true)]
        [uri]$SiteUri,
        [Parameter(ParameterSetName="BySiteUri", Mandatory=$false, ValueFromPipeline=$true)]
        [Parameter(ParameterSetName="ByAuthMetadata", Mandatory=$true, ValueFromPipeline=$true)]
        [THNETII.SharePoint.BearerAuthorization.SharePointSiteAuthorizationMetadata]$AuthDiscovery,
        [AllowNull()]
        [string]$ClientId,
        [AllowNull()]
        [Microsoft.Identity.Client.PublicClientApplicationOptions]$Options
    )

    if (-not $AuthDiscovery) {
        $AuthDiscovery = Get-SPAuthDiscovery -SiteUri $SiteUri
    }

    if (-not $Options) {
        $Options = New-Object Microsoft.Identity.Client.PublicClientApplicationOptions
    }

    $Options.Instance = $AuthDiscovery.GetAuthorizationInstance()
    $Options.TenantId = $AuthDiscovery.Realm

    $Options | New-MsalPublicClientApplicationBuilder -ClientId $ClientId
}

function New-SPAuthMsalConfidentialClientApplicationBuilder {
    [CmdletBinding(DefaultParameterSetName="BySiteUri")]
    [OutputType([Microsoft.Identity.Client.ConfidentialClientApplicationBuilder])]
    param (
        [Parameter(ParameterSetName="BySiteUri", Mandatory=$true)]
        [uri]$SiteUri,
        [Parameter(ParameterSetName="BySiteUri", Mandatory=$false, ValueFromPipeline=$true)]
        [Parameter(ParameterSetName="ByAuthMetadata", Mandatory=$true, ValueFromPipeline=$true)]
        [THNETII.SharePoint.BearerAuthorization.SharePointSiteAuthorizationMetadata]$AuthDiscovery,
        [AllowNull()]
        [string]$ClientId,
        [AllowNull()]
        [Microsoft.Identity.Client.ConfidentialClientApplicationOptions]$Options
    )

    if (-not $AuthDiscovery) {
        $AuthDiscovery = Get-SPAuthDiscovery -SiteUri $SiteUri
    }

    if (-not $Options) {
        $Options = New-Object Microsoft.Identity.Client.ConfidentialClientApplicationOptions
    }

    $Options.Instance = $AuthDiscovery.GetAuthorizationInstance()
    $Options.TenantId = $AuthDiscovery.Realm

    $Options | New-MsalConfidentialClientApplicationBuilder -ClientId $ClientId
}
