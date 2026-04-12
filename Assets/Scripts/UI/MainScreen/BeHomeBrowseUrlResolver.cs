using System;

/// <summary>
/// Resolves hosted BE browse routes into absolute site URLs for the embedded browser.
/// </summary>
public static class BeHomeBrowseUrlResolver
{
    /// <summary>
    /// Resolves a hosted browse route or URL against the configured browse home.
    /// </summary>
    /// <param name="browsePageUrl">The configured absolute browse home URL.</param>
    /// <param name="routeOrUrl">The hosted route or URL received from the site.</param>
    /// <returns>The absolute hosted browse URL to load.</returns>
    public static string ResolveBrowseSiteUrl(string browsePageUrl, string routeOrUrl)
    {
        if (string.IsNullOrWhiteSpace(routeOrUrl))
        {
            return browsePageUrl;
        }

        if (TryGetHttpAbsoluteUrl(routeOrUrl, out var absoluteUrl))
        {
            return absoluteUrl;
        }

        if (!Uri.TryCreate(browsePageUrl, UriKind.Absolute, out var browseUri))
        {
            return routeOrUrl;
        }

        return new Uri(browseUri, routeOrUrl).ToString();
    }

    /// <summary>
    /// Resolves the best recovery URL for the main browse surface.
    /// </summary>
    /// <param name="browsePageUrl">The configured absolute browse home URL.</param>
    /// <param name="lastResolvedBrowseUrl">The last resolved absolute browse URL.</param>
    /// <param name="lastHostedBrowseRoute">The last hosted route reported by the site.</param>
    /// <returns>The absolute hosted browse URL to recover.</returns>
    public static string ResolveActiveBrowseUrl(string browsePageUrl, string lastResolvedBrowseUrl, string lastHostedBrowseRoute)
    {
        if (TryGetHttpAbsoluteUrl(lastResolvedBrowseUrl, out var absoluteResolvedUrl))
        {
            return absoluteResolvedUrl;
        }

        if (!string.IsNullOrWhiteSpace(lastHostedBrowseRoute))
        {
            return ResolveBrowseSiteUrl(browsePageUrl, lastHostedBrowseRoute);
        }

        return browsePageUrl;
    }

    private static bool TryGetHttpAbsoluteUrl(string url, out string absoluteUrl)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && uri.IsAbsoluteUri
            && (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            absoluteUrl = uri.ToString();
            return true;
        }

        absoluteUrl = string.Empty;
        return false;
    }
}
