using NUnit.Framework;

public sealed class BeHomePresenceLeasePolicyTests
{
    [Test]
    public void ShouldRenew_ReturnsTrueBeforeFirstSuccessfulRefresh()
    {
        var policy = new BeHomePresenceLeasePolicy(120f);

        Assert.That(policy.ShouldRenew(0f, 10f, refreshInFlight: false), Is.True);
    }

    [Test]
    public void ShouldRenew_ReturnsFalseWhileRefreshIsInFlight()
    {
        var policy = new BeHomePresenceLeasePolicy(120f);

        Assert.That(policy.ShouldRenew(30f, 200f, refreshInFlight: true), Is.False);
    }

    [Test]
    public void ShouldRenew_ReturnsTrueOnlyAfterRenewIntervalElapses()
    {
        var policy = new BeHomePresenceLeasePolicy(120f);

        Assert.Multiple(() =>
        {
            Assert.That(policy.ShouldRenew(30f, 100f, refreshInFlight: false), Is.False);
            Assert.That(policy.ShouldRenew(30f, 150f, refreshInFlight: false), Is.True);
        });
    }
}
