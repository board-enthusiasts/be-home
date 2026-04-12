#nullable enable
using System;

using BoardEnthusiasts.BeHome.Api.Models;

namespace BoardEnthusiasts.BeHome.Api.Services
{
/// <summary>
/// Coordinates the local BE Home presence session state used for initial registration and passive request headers.
/// </summary>
public sealed class BeHomePresenceCoordinator : IBeHomePresenceSnapshotProvider
{
    private const string PresenceEndRoute = "/internal/be-home/presence/end";
    private const string MetricsRoute = "/internal/be-home/metrics";
    private static readonly TimeSpan DefaultCommunityMetricsOptInWindow = TimeSpan.FromSeconds(5);
    private readonly BeHomeDeviceIdentity _deviceIdentity;
    private readonly string _clientVersion;
    private readonly string _appEnvironment;
    private readonly Func<DateTimeOffset> _utcNow;
    private readonly TimeSpan _communityMetricsOptInWindow;
    private DateTimeOffset _lastUserInteractionAt;

    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomePresenceCoordinator"/> class.
    /// </summary>
    /// <param name="deviceIdentityProvider">The provider used to resolve the current BE Home device identity.</param>
    /// <param name="clientVersion">The BE Home client version to report in presence payloads.</param>
    /// <param name="appEnvironment">The BE environment targeted by this BE Home build.</param>
    /// <param name="sessionId">The optional session identifier to reuse for tests or restoration.</param>
    public BeHomePresenceCoordinator(
        IBeHomeDeviceIdentityProvider deviceIdentityProvider,
        string clientVersion,
        string appEnvironment,
        string? sessionId = null,
        Func<DateTimeOffset>? utcNow = null,
        TimeSpan? communityMetricsOptInWindow = null)
    {
        if (deviceIdentityProvider == null)
        {
            throw new ArgumentNullException(nameof(deviceIdentityProvider));
        }

        _deviceIdentity = deviceIdentityProvider.GetDeviceIdentity() ?? throw new ArgumentException("A device identity is required.", nameof(deviceIdentityProvider));
        _clientVersion = clientVersion ?? string.Empty;
        _appEnvironment = appEnvironment ?? string.Empty;
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        _communityMetricsOptInWindow = communityMetricsOptInWindow ?? DefaultCommunityMetricsOptInWindow;
        SessionId = !string.IsNullOrWhiteSpace(sessionId)
            ? sessionId
            : Guid.NewGuid().ToString("N");
        CurrentAuthState = BeHomeAuthState.Anonymous;
        _lastUserInteractionAt = _utcNow();
    }

    /// <summary>
    /// Gets the current BE Home session identifier.
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Gets the current BE Home auth state mirrored into the native shell.
    /// </summary>
    public BeHomeAuthState CurrentAuthState { get; private set; }

    /// <summary>
    /// Updates the mirrored BE Home auth state.
    /// </summary>
    /// <param name="authState">The latest auth state reported by the hosted surface.</param>
    /// <returns><see langword="true"/> when the state changed; otherwise, <see langword="false"/>.</returns>
    public bool SetAuthState(BeHomeAuthState authState)
    {
        if (CurrentAuthState == authState)
        {
            return false;
        }

        CurrentAuthState = authState;
        return true;
    }

    /// <inheritdoc/>
    public void MarkUserInteraction()
    {
        _lastUserInteractionAt = _utcNow();
    }

    /// <summary>
    /// Builds the current BE Home presence snapshot from the local session state.
    /// </summary>
    /// <returns>The current BE Home presence snapshot.</returns>
    public BeHomePresenceSnapshot CreatePresenceSnapshot()
    {
        return new BeHomePresenceSnapshot(SessionId, _deviceIdentity, CurrentAuthState, _clientVersion, _appEnvironment);
    }

    /// <inheritdoc/>
    public bool ShouldIncludeCommunityMetrics(string relativePath)
    {
        if (string.Equals(relativePath, PresenceEndRoute, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(relativePath, MetricsRoute, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return (_utcNow() - _lastUserInteractionAt) <= _communityMetricsOptInWindow;
    }
}
}
#nullable restore
