using BoardEnthusiasts.BeHome.Api.DeviceIdentity;
using BoardEnthusiasts.BeHome.Api.Models;

using NUnit.Framework;

namespace BoardEnthusiasts.BeHome.Tests;

[TestFixture]
public sealed class BeHomeDeviceIdentitySelectionTests
{
    [Test]
    public void SelectPreferredIdentity_PrefersAndroidIdBeforeMacCandidates()
    {
        var selectedIdentity = BeHomeDeviceIdentitySelection.SelectPreferredIdentity("abc123def4567890");

        Assert.Multiple(() =>
        {
            Assert.That(selectedIdentity, Is.Not.Null);
            Assert.That(selectedIdentity!.Source, Is.EqualTo(BeHomeDeviceIdSource.AndroidSecureAndroidId));
            Assert.That(selectedIdentity.RawIdentifier, Is.EqualTo("abc123def4567890"));
        });
    }

    [Test]
    public void SelectPreferredIdentity_ReturnsNullWhenAndroidIdIsUnavailable()
    {
        var selectedIdentity = BeHomeDeviceIdentitySelection.SelectPreferredIdentity("0000000000000000");

        Assert.That(selectedIdentity, Is.Null);
    }
}
