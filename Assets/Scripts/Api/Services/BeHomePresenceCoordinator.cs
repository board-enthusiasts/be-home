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
    /// <inheritdoc/>
    public void SetAuthState(BeHomeAuthState authState)
    {
        CurrentAuthState = authState;
    }

    /// <summary>
    /// Builds the current BE Home presence snapshot from the local session state.
    /// </summary>
    /// <returns>The current BE Home presence snapshot.</returns>
    public BeHomePresenceSnapshot CreatePresenceSnapshot()
    {
        return new BeHomePresenceSnapshot(SessionId, _deviceIdentity, CurrentAuthState, _clientVersion, _appEnvironment);
    }
}
}
#nullable restore
