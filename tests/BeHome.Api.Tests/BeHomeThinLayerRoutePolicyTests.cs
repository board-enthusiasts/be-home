using NUnit.Framework;

namespace BoardEnthusiasts.BeHome.Tests;

[TestFixture]
public sealed class BeHomeThinLayerRoutePolicyTests
{
    [TestCase("/developer", true)]
    [TestCase("/developer?domain=studios&workflow=studios-overview", true)]
    [TestCase("/moderate", true)]
    [TestCase("/moderate?workflow=reports-review", true)]
    [TestCase("https://staging.boardenthusiasts.com/developer", true)]
    [TestCase("https://boardenthusiasts.com/moderate?workflow=reports-review", true)]
    [TestCase("/browse", false)]
    [TestCase("/player", false)]
    [TestCase("https://staging.boardenthusiasts.com/browse/blue-harbor-games/lantern-drift", false)]
    [TestCase("", false)]
    public void IsRestrictedWorkspaceRoute_ReturnsExpectedValue(string routeOrUrl, bool expected)
    {
        bool isRestricted = BeHomeThinLayerRoutePolicy.IsRestrictedWorkspaceRoute(routeOrUrl);

        Assert.That(isRestricted, Is.EqualTo(expected));
    }
}
