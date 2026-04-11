using System;

using BoardEnthusiasts.BeHome.Api.Models;
using BoardEnthusiasts.BeHome.Api.Services;

using NUnit.Framework;

namespace BoardEnthusiasts.BeHome.Tests;

[TestFixture]
public sealed class BeHomePresenceCoordinatorTests
{
    [Test]
    public void CreateHeartbeat_UsesStableSessionAndDeviceIdentity()
    {
        var coordinator = new BeHomePresenceCoordinator(
            new StubDeviceIdentityProvider(new BeHomeDeviceIdentity("board-123", BeHomeDeviceIdSource.InstallId)),
            "1.2.3",
            "staging",
            "session-abc");

        var firstHeartbeat = coordinator.CreateHeartbeat();
        var secondHeartbeat = coordinator.CreateHeartbeat();

        Assert.Multiple(() =>
        {
            Assert.That(firstHeartbeat.SessionId, Is.EqualTo("session-abc"));
            Assert.That(secondHeartbeat.SessionId, Is.EqualTo("session-abc"));
            Assert.That(firstHeartbeat.DeviceIdentity.RawIdentifier, Is.EqualTo("board-123"));
            Assert.That(secondHeartbeat.DeviceIdentity.RawIdentifier, Is.EqualTo("board-123"));
            Assert.That(firstHeartbeat.AuthState, Is.EqualTo(BeHomeAuthState.Anonymous));
            Assert.That(firstHeartbeat.ClientVersion, Is.EqualTo("1.2.3"));
            Assert.That(firstHeartbeat.AppEnvironment, Is.EqualTo("staging"));
        });
    }

    [Test]
    public void SetAuthState_ChangesSubsequentHeartbeatAuthState()
    {
        var coordinator = new BeHomePresenceCoordinator(
            new StubDeviceIdentityProvider(new BeHomeDeviceIdentity("board-123", BeHomeDeviceIdSource.AndroidSecureAndroidId)),
            "2.0.0",
            "production");

        coordinator.SetAuthState(BeHomeAuthState.SignedIn);
        var heartbeat = coordinator.CreateHeartbeat();

        Assert.That(heartbeat.AuthState, Is.EqualTo(BeHomeAuthState.SignedIn));
    }

    [Test]
    public void ApplySessionStatus_UpdatesRecommendedIntervalsAndAuthState()
    {
        var coordinator = new BeHomePresenceCoordinator(
            new StubDeviceIdentityProvider(new BeHomeDeviceIdentity("board-123", BeHomeDeviceIdSource.InstallId)),
            "2.0.0",
            "production",
            "session-xyz");

        coordinator.ApplySessionStatus(
            new BeHomePresenceSessionStatus(
                "session-xyz",
                BeHomeAuthState.SignedIn,
                DateTimeOffset.Parse("2026-04-10T12:00:00Z"),
                45,
                150));

        Assert.Multiple(() =>
        {
            Assert.That(coordinator.CurrentAuthState, Is.EqualTo(BeHomeAuthState.SignedIn));
            Assert.That(coordinator.HeartbeatIntervalSeconds, Is.EqualTo(45));
            Assert.That(coordinator.ActiveTtlSeconds, Is.EqualTo(150));
        });
    }

    [Test]
    public void ApplySessionStatus_WithDifferentSessionId_Throws()
    {
        var coordinator = new BeHomePresenceCoordinator(
            new StubDeviceIdentityProvider(new BeHomeDeviceIdentity("board-123", BeHomeDeviceIdSource.InstallId)),
            "2.0.0",
            "production",
            "session-xyz");

        Assert.That(
            () => coordinator.ApplySessionStatus(
                new BeHomePresenceSessionStatus(
                    "different-session",
                    BeHomeAuthState.Anonymous,
                    DateTimeOffset.Parse("2026-04-10T12:00:00Z"),
                    60,
                    180)),
            Throws.ArgumentException);
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
