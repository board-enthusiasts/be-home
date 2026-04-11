#nullable enable
using System;

using BoardEnthusiasts.BeHome.Api.Models;

namespace BoardEnthusiasts.BeHome.Api.Services
{
/// <summary>
/// Coordinates the local BE Home presence session state that drives heartbeat requests.
/// </summary>
public sealed class BeHomePresenceCoordinator
{
    private readonly BeHomeDeviceIdentity _deviceIdentity;
    private readonly string _clientVersion;
    private readonly string _appEnvironment;

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
        string? sessionId = null)
    {
        if (deviceIdentityProvider == null)
        {
            throw new ArgumentNullException(nameof(deviceIdentityProvider));
        }

        _deviceIdentity = deviceIdentityProvider.GetDeviceIdentity() ?? throw new ArgumentException("A device identity is required.", nameof(deviceIdentityProvider));
        _clientVersion = clientVersion ?? string.Empty;
        _appEnvironment = appEnvironment ?? string.Empty;
        SessionId = !string.IsNullOrWhiteSpace(sessionId)
            ? sessionId
            : Guid.NewGuid().ToString("N");
        CurrentAuthState = BeHomeAuthState.Anonymous;
        HeartbeatIntervalSeconds = 60;
        ActiveTtlSeconds = 180;
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
    /// Gets the recommended heartbeat interval in seconds.
    /// </summary>
    public int HeartbeatIntervalSeconds { get; private set; }

    /// <summary>
    /// Gets the active-session TTL in seconds.
    /// </summary>
    public int ActiveTtlSeconds { get; private set; }

    /// <summary>
    /// Updates the current BE Home auth state.
    /// </summary>
    /// <param name="authState">The new mirrored auth state.</param>
    public void SetAuthState(BeHomeAuthState authState)
    {
        CurrentAuthState = authState;
    }

    /// <summary>
    /// Builds the next BE Home heartbeat payload from the current local session state.
    /// </summary>
    /// <returns>The next presence heartbeat payload.</returns>
    public BeHomePresenceHeartbeat CreateHeartbeat()
    {
        return new BeHomePresenceHeartbeat(SessionId, _deviceIdentity, CurrentAuthState, _clientVersion, _appEnvironment);
    }

    /// <summary>
    /// Applies the accepted session status returned by the backend.
    /// </summary>
    /// <param name="sessionStatus">The accepted session status returned by the backend.</param>
    public void ApplySessionStatus(BeHomePresenceSessionStatus sessionStatus)
    {
        if (sessionStatus == null)
        {
            throw new ArgumentNullException(nameof(sessionStatus));
        }

        if (!string.Equals(sessionStatus.SessionId, SessionId, StringComparison.Ordinal))
        {
            throw new ArgumentException("The session status does not match the current BE Home session.", nameof(sessionStatus));
        }

        CurrentAuthState = sessionStatus.AuthState;
        HeartbeatIntervalSeconds = Math.Max(1, sessionStatus.HeartbeatIntervalSeconds);
        ActiveTtlSeconds = Math.Max(1, sessionStatus.ActiveTtlSeconds);
    }
}
}
#nullable restore
