using NUnit.Framework;

namespace BoardEnthusiasts.BeHome.Tests;

[TestFixture]
public sealed class BeHomeBrowseUrlResolverTests
{
    [TestCase("https://staging.boardenthusiasts.com/browse")]
    [TestCase("https://boardenthusiasts.com/browse")]
    public void ResolveBrowseSiteUrl_WithRootRelativeRoute_ReturnsHostedAbsoluteUrl(string browsePageUrl)
    {
        var resolved = BeHomeBrowseUrlResolver.ResolveBrowseSiteUrl(
            browsePageUrl,
            "/browse/board-enthusiasts-be/be-home");

        Assert.That(resolved, Is.EqualTo($"{GetSiteOrigin(browsePageUrl)}/browse/board-enthusiasts-be/be-home"));
    }

    [TestCase("https://staging.boardenthusiasts.com/browse", "https://staging.boardenthusiasts.com/offerings")]
    [TestCase("https://boardenthusiasts.com/browse", "https://boardenthusiasts.com/offerings")]
    public void ResolveBrowseSiteUrl_WithAbsoluteHttpUrl_ReturnsOriginalUrl(string browsePageUrl, string absoluteUrl)
    {
        var resolved = BeHomeBrowseUrlResolver.ResolveBrowseSiteUrl(
            browsePageUrl,
            absoluteUrl);

        Assert.That(resolved, Is.EqualTo(absoluteUrl));
    }

    [TestCase("https://staging.boardenthusiasts.com/browse")]
    [TestCase("https://boardenthusiasts.com/browse")]
    public void ResolveActiveBrowseUrl_WithHttpResolvedUrl_ReturnsResolvedUrl(string browsePageUrl)
    {
        var expectedUrl = $"{GetSiteOrigin(browsePageUrl)}/browse/board-enthusiasts-be/be-home";
        var resolved = BeHomeBrowseUrlResolver.ResolveActiveBrowseUrl(
            browsePageUrl,
            expectedUrl,
            "/browse/board-enthusiasts-be/be-home");

        Assert.That(resolved, Is.EqualTo(expectedUrl));
    }

    [TestCase("https://staging.boardenthusiasts.com/browse")]
    [TestCase("https://boardenthusiasts.com/browse")]
    public void ResolveActiveBrowseUrl_WithFileResolvedUrl_FallsBackToHostedRoute(string browsePageUrl)
    {
        var resolved = BeHomeBrowseUrlResolver.ResolveActiveBrowseUrl(
            browsePageUrl,
            "file:///browse/board-enthusiasts-be/be-home",
            "/browse/board-enthusiasts-be/be-home");

        Assert.That(resolved, Is.EqualTo($"{GetSiteOrigin(browsePageUrl)}/browse/board-enthusiasts-be/be-home"));
    }

    [TestCase("https://staging.boardenthusiasts.com/browse")]
    [TestCase("https://boardenthusiasts.com/browse")]
    public void ResolveActiveBrowseUrl_WithoutKnownRoute_FallsBackToBrowseHome(string browsePageUrl)
    {
        var resolved = BeHomeBrowseUrlResolver.ResolveActiveBrowseUrl(
            browsePageUrl,
            "file:///browse",
            string.Empty);

        Assert.That(resolved, Is.EqualTo(browsePageUrl));
    }

    private static string GetSiteOrigin(string browsePageUrl)
    {
        var browseUri = new Uri(browsePageUrl, UriKind.Absolute);
        return browseUri.GetLeftPart(UriPartial.Authority);
    }
}
