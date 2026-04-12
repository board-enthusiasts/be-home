using NUnit.Framework;

namespace BoardEnthusiasts.BeHome.Tests;

[TestFixture]
public sealed class BeHomeBrowseNavigationPolicyTests
{
    [Test]
    public void ShouldTreatLoadErrorAsUnavailable_BeforeAnySuccessfulLoad_ReturnsTrue()
    {
        var policy = new BeHomeBrowseNavigationPolicy();

        Assert.That(policy.ShouldTreatLoadErrorAsUnavailable(), Is.True);
    }

    [Test]
    public void ShouldTreatLoadErrorAsUnavailable_AfterPrimaryContentLoadedWithoutPendingNavigation_ReturnsFalse()
    {
        var policy = new BeHomeBrowseNavigationPolicy();
        policy.MarkPrimaryContentLoaded();

        Assert.That(policy.ShouldTreatLoadErrorAsUnavailable(), Is.False);
    }

    [Test]
    public void ShouldTreatLoadErrorAsUnavailable_DuringTopLevelNavigationAfterPriorSuccess_ReturnsTrue()
    {
        var policy = new BeHomeBrowseNavigationPolicy();
        policy.MarkPrimaryContentLoaded();
        policy.BeginTopLevelNavigation();

        Assert.That(policy.ShouldTreatLoadErrorAsUnavailable(), Is.True);
    }

    [Test]
    public void MarkPrimaryContentUnavailable_ResetsPolicyBackToUnavailable()
    {
        var policy = new BeHomeBrowseNavigationPolicy();
        policy.MarkPrimaryContentLoaded();
        policy.MarkPrimaryContentUnavailable();

        Assert.That(policy.ShouldTreatLoadErrorAsUnavailable(), Is.True);
    }

    [Test]
    public void ShouldAttemptBlankPageRecovery_AfterPrimaryContentLoaded_ReturnsTrueUntilAttemptIsSpent()
    {
        var policy = new BeHomeBrowseNavigationPolicy();
        policy.MarkPrimaryContentLoaded();

        Assert.That(policy.ShouldAttemptBlankPageRecovery(), Is.True);

        policy.MarkBlankPageRecoveryAttempted();

        Assert.That(policy.ShouldAttemptBlankPageRecovery(), Is.False);
    }

    [Test]
    public void ShouldAttemptBlankPageRecovery_DuringTopLevelNavigation_ReturnsFalse()
    {
        var policy = new BeHomeBrowseNavigationPolicy();
        policy.MarkPrimaryContentLoaded();
        policy.BeginTopLevelNavigation();

        Assert.That(policy.ShouldAttemptBlankPageRecovery(), Is.False);
    }

    [Test]
    public void MarkPrimaryContentLoaded_AfterRecoveryAttempt_RearmsBlankPageRecovery()
    {
        var policy = new BeHomeBrowseNavigationPolicy();
        policy.MarkPrimaryContentLoaded();
        policy.MarkBlankPageRecoveryAttempted();

        policy.MarkPrimaryContentLoaded();

        Assert.That(policy.ShouldAttemptBlankPageRecovery(), Is.True);
    }
}
