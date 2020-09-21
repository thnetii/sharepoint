namespace THNETII.SharePoint.IdentityModel
{
    internal static class LogMessages
    {
        internal const string THSP1001 = nameof(THSP1001) + ": The address specified '{0}' is not valid as per HTTPS scheme. Please specify an https address for security reasons. If you want to test with http address, set the " + nameof(HttpWwwAuthenticateHeaderParameterRetriever.RequireHttps) + " property on IDocumentRetriever to false.";
        internal const string THSP1002 = nameof(THSP1002) + ": Obtaining HTTP Bearer Token Authorization parameters from: '{0}'.";
        internal const string THSP1003 = nameof(THSP1003) + ": Unable to retrieve HTTP Bearer Token Authorization parameters from: '{0}'.";
    }
}
