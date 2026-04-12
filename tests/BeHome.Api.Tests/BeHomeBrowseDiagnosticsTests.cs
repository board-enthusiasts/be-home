using NUnit.Framework;

public sealed class BeHomeBrowseDiagnosticsTests
{
    [Test]
    public void Summarize_IncludesBrowseRouteAndFilterCounts()
    {
        var diagnostics = new BeHomeBrowseDiagnostics
        {
            surface = "browse",
            route = "/browse?embed=board",
            searchResultCount = 10,
            totalCatalogCount = 42,
            currentPage = 2,
            searchQueryLength = 7,
            selectedStudiosCount = 1,
            selectedGenresCount = 3,
        };

        var summary = BeHomeBrowseDiagnosticsFormatter.Summarize(diagnostics);

        Assert.That(summary, Does.Contain("surface=browse"));
        Assert.That(summary, Does.Contain("route=/browse?embed=board"));
        Assert.That(summary, Does.Contain("results=10/42"));
        Assert.That(summary, Does.Contain("page=2"));
        Assert.That(summary, Does.Contain("filters=query:7,studios:1,genres:3"));
    }

    [Test]
    public void Summarize_IncludesTitleMediaMixAndAssetFlags()
    {
        var diagnostics = new BeHomeBrowseDiagnostics
        {
            surface = "quick-view",
            route = "/browse?embed=board",
            titleId = "title-1",
            titleDisplayName = "Lantern Drift",
            studioSlug = "blue-harbor-games",
            studioDisplayName = "Blue Harbor Games",
            contentKind = "game",
            selectedPreviewKind = "hero",
            selectedPreviewHost = "cdn.example.com",
            heroImageHost = "cdn.example.com",
            cardImageHost = "cdn.example.com",
            acquisitionHost = "publisher.example.com",
            showcaseMediaCount = 3,
            showcaseImageCount = 2,
            showcaseVideoCount = 1,
            hasHeroImage = true,
            hasCardImage = true,
            hasLogoImage = true,
            hasAcquisitionUrl = true,
        };

        var summary = BeHomeBrowseDiagnosticsFormatter.Summarize(diagnostics);

        Assert.That(summary, Does.Contain("title=Lantern Drift [title-1]"));
        Assert.That(summary, Does.Contain("studio=Blue Harbor Games [blue-harbor-games]"));
        Assert.That(summary, Does.Contain("content=game"));
        Assert.That(summary, Does.Contain("preview=hero@cdn.example.com"));
        Assert.That(summary, Does.Contain("showcase=3 (images=2, videos=1)"));
        Assert.That(summary, Does.Contain("assets=hero,card,logo,acquisition"));
    }
}
