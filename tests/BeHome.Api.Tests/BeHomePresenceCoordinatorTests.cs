using System;

using BoardEnthusiasts.BeHome.Api.Models;
using BoardEnthusiasts.BeHome.Api.Services;

using NUnit.Framework;

namespace BoardEnthusiasts.BeHome.Tests;

[TestFixture]
public sealed class BeHomePresenceCoordinatorTests
{
    [Test]
    public void CreatePresenceSnapshot_UsesStableSessionAndDeviceIdentity()
    {
        var coordinator = new BeHomePresenceCoordinator(
            new StubDeviceIdentityProvider(new BeHomeDeviceIdentity("board-123", BeHomeDeviceIdSource.InstallId)),
            "1.2.3",
            "staging",
            "session-abc");

        var firstPresence = coordinator.CreatePresenceSnapshot();
        var secondPresence = coordinator.CreatePresenceSnapshot();

        Assert.Multiple(() =>
        {
            Assert.That(firstPresence.SessionId, Is.EqualTo("session-abc"));
            Assert.That(secondPresence.SessionId, Is.EqualTo("session-abc"));
            Assert.That(firstPresence.DeviceIdentity.RawIdentifier, Is.EqualTo("board-123"));
            Assert.That(secondPresence.DeviceIdentity.RawIdentifier, Is.EqualTo("board-123"));
            Assert.That(firstPresence.AuthState, Is.EqualTo(BeHomeAuthState.Anonymous));
            Assert.That(firstPresence.ClientVersion, Is.EqualTo("1.2.3"));
            Assert.That(firstPresence.AppEnvironment, Is.EqualTo("staging"));
        });
    }

    [Test]
    public void SetAuthState_ChangesSubsequentPresenceAuthState()
    {
        var coordinator = new BeHomePresenceCoordinator(
            new StubDeviceIdentityProvider(new BeHomeDeviceIdentity("board-123", BeHomeDeviceIdSource.AndroidSecureAndroidId)),
            "2.0.0",
            "production");

        coordinator.SetAuthState(BeHomeAuthState.SignedIn);
        var presence = coordinator.CreatePresenceSnapshot();

        Assert.That(presence.AuthState, Is.EqualTo(BeHomeAuthState.SignedIn));
    }

    private sealed class StubDeviceIdentityProvider : IBeHomeDeviceIdentityProvider
    {
        private readonly BeHomeDeviceIdentity _deviceIdentity;

        public StubDeviceIdentityProvider(BeHomeDeviceIdentity deviceIdentity)
        {
            _deviceIdentity = deviceIdentity;
        }

        public BeHomeDeviceIdentity GetDeviceIdentity()
        {
            return _deviceIdentity;
        }
    }
}
