using BoardEnthusiasts.BeHome.Api.Models;
using BoardEnthusiasts.BeHome.UI.NativeBrowse;

using NUnit.Framework;

namespace BoardEnthusiasts.BeHome.Tests;

[TestFixture]
public sealed class BeHomeNativeBrowseViewModelBuilderTests
{
    [Test]
    public void CreateLoading_UsesEnvironmentAwareStatusCopy()
    {
        var model = new BeHomeNativeBrowseModel("staging", "https://api.staging.boardenthusiasts.com");

        var viewModel = BeHomeNativeBrowseViewModelBuilder.CreateLoading(model);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.IsLoading, Is.True);
            Assert.That(viewModel.ShowEmptyState, Is.True);
            Assert.That(viewModel.StatusText, Is.EqualTo("Loading public catalog..."));
            Assert.That(viewModel.MessageText, Does.Contain("staging"));
        });
    }

    [Test]
    public void CreateLoaded_PreservesSelectedTitleWhenPresent()
    {
        var model = new BeHomeNativeBrowseModel("production", "https://api.boardenthusiasts.com");
        var page = new BeHomeCatalogPage(
        [
            new BeHomeCatalogTitleSummary(
                "title-1",
                "studio-1",
                "pine-lantern-labs",
                "signal-signal",
                "game",
                "Signal Signal",
                "A co-op timing challenge.",
                "Cooperative Puzzle",
                "1-4 players",
                "ESRB E10+",
                string.Empty,
                string.Empty),
            new BeHomeCatalogTitleSummary(
                "title-2",
                "studio-2",
                "harborlight-mechanics",
                "moon-turn",
                "app",
                "Moon Turn",
                "A storytelling companion app.",
                "Companion",
                "Solo",
                "Everyone",
                string.Empty,
                string.Empty),
        ],
        1,
        48,
        2,
        1);

        var viewModel = BeHomeNativeBrowseViewModelBuilder.CreateLoaded(model, page, "title-2");

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.ShowEmptyState, Is.False);
            Assert.That(viewModel.SelectedTitleHeading, Is.EqualTo("Moon Turn"));
            Assert.That(viewModel.Cards[0].IsSelected, Is.False);
            Assert.That(viewModel.Cards[1].IsSelected, Is.True);
        });
    }

    [Test]
    public void CreateLoaded_UsesFirstTitleWhenSelectionIsMissing()
    {
        var model = new BeHomeNativeBrowseModel("production", "https://api.boardenthusiasts.com");
        var page = new BeHomeCatalogPage(
        [
            new BeHomeCatalogTitleSummary(
                "title-1",
                "studio-1",
                "pine-lantern-labs",
                "signal-signal",
                "game",
                "Signal Signal",
                "A co-op timing challenge.",
                "Cooperative Puzzle",
                "1-4 players",
                "ESRB E10+",
                string.Empty,
                string.Empty),
        ],
        1,
        48,
        1,
        1);

        var viewModel = BeHomeNativeBrowseViewModelBuilder.CreateLoaded(model, page, "missing-title");

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.SelectedTitleHeading, Is.EqualTo("Signal Signal"));
            Assert.That(viewModel.Cards[0].IsSelected, Is.True);
            Assert.That(viewModel.MessageText, Does.Contain("production"));
        });
    }

    [Test]
    public void CreateError_ShowsUserFacingFailureState()
    {
        var model = new BeHomeNativeBrowseModel("staging", "https://api.staging.boardenthusiasts.com");

        var viewModel = BeHomeNativeBrowseViewModelBuilder.CreateError(model, "Catalog request timed out.");

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.ShowEmptyState, Is.True);
            Assert.That(viewModel.StatusText, Is.EqualTo("Catalog load failed."));
            Assert.That(viewModel.EmptyBodyText, Is.EqualTo("Catalog request timed out."));
            Assert.That(viewModel.SelectedTitleHeading, Is.EqualTo("Catalog unavailable"));
        });
    }
}
