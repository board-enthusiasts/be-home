using System;

namespace BoardEnthusiasts.BeHome.Api.Models
{
/// <summary>
/// Represents the BE Home authentication state mirrored into the native shell.
/// </summary>
public enum BeHomeAuthState
{
    /// <summary>
    /// The current BE Home session is not authenticated to a BE account.
    /// </summary>
    Anonymous = 0,

    /// <summary>
    /// The current BE Home session is authenticated to a BE account.
    /// </summary>
    SignedIn = 1,
}

/// <summary>
/// Describes the source used to identify a BE Home device.
/// </summary>
public enum BeHomeDeviceIdSource
{
    /// <summary>
    /// The identifier came from Android's <c>Settings.Secure.ANDROID_ID</c>.
    /// </summary>
    AndroidSecureAndroidId = 0,

    /// <summary>
    /// The identifier came from a BE-generated install identifier stored locally.
    /// </summary>
    InstallId = 3,
}

/// <summary>
/// Represents a raw device identity before the backend hashes it.
/// </summary>
public sealed class BeHomeDeviceIdentity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomeDeviceIdentity"/> class.
    /// </summary>
    /// <param name="rawIdentifier">The raw local identifier used to recognize the device.</param>
    /// <param name="source">The source that produced the raw identifier.</param>
    public BeHomeDeviceIdentity(string rawIdentifier, BeHomeDeviceIdSource source)
    {
        RawIdentifier = !string.IsNullOrWhiteSpace(rawIdentifier)
            ? rawIdentifier
            : throw new ArgumentException("A raw device identifier is required.", nameof(rawIdentifier));
        Source = source;
    }

    /// <summary>
    /// Gets the raw local identifier used to recognize the device.
    /// </summary>
    public string RawIdentifier { get; }

    /// <summary>
    /// Gets the source that produced the raw identifier.
    /// </summary>
    public BeHomeDeviceIdSource Source { get; }
}

/// <summary>
/// Represents the current BE Home presence snapshot used for registration and passive request headers.
/// </summary>
public sealed class BeHomePresenceSnapshot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomePresenceSnapshot"/> class.
    /// </summary>
    /// <param name="sessionId">The per-launch BE Home session identifier.</param>
    /// <param name="deviceIdentity">The current device identity for the BE Home install.</param>
    /// <param name="authState">The current BE Home auth state.</param>
    /// <param name="clientVersion">The BE Home client version.</param>
    /// <param name="appEnvironment">The BE environment targeted by this BE Home build.</param>
    public BeHomePresenceSnapshot(
        string sessionId,
        BeHomeDeviceIdentity deviceIdentity,
        BeHomeAuthState authState,
        string clientVersion,
        string appEnvironment)
    {
        SessionId = !string.IsNullOrWhiteSpace(sessionId)
            ? sessionId
            : throw new ArgumentException("A session id is required.", nameof(sessionId));
        DeviceIdentity = deviceIdentity ?? throw new ArgumentNullException(nameof(deviceIdentity));
        AuthState = authState;
        ClientVersion = clientVersion ?? string.Empty;
        AppEnvironment = appEnvironment ?? string.Empty;
    }

    /// <summary>
    /// Gets the per-launch BE Home session identifier.
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Gets the current device identity for the BE Home install.
    /// </summary>
    public BeHomeDeviceIdentity DeviceIdentity { get; }

    /// <summary>
    /// Gets the current BE Home auth state.
    /// </summary>
    public BeHomeAuthState AuthState { get; }

    /// <summary>
    /// Gets the BE Home client version.
    /// </summary>
    public string ClientVersion { get; }

    /// <summary>
    /// Gets the BE environment targeted by this BE Home build.
    /// </summary>
    public string AppEnvironment { get; }
}

/// <summary>
/// Provides the current BE Home presence snapshot for request registration and passive API headers.
/// </summary>
public interface IBeHomePresenceSnapshotProvider
{
    /// <summary>
    /// Gets the current BE Home presence snapshot.
    /// </summary>
    /// <returns>The current BE Home presence snapshot.</returns>
    BeHomePresenceSnapshot CreatePresenceSnapshot();

    /// <summary>
    /// Records that the player just performed an interaction that should allow nearby requests to ask for community metrics.
    /// </summary>
    void MarkUserInteraction();

    /// <summary>
    /// Determines whether the supplied BE API route should opt in to community metrics headers.
    /// </summary>
    /// <param name="relativePath">The path relative to the configured API base URL.</param>
    /// <returns><see langword="true"/> when the request should ask for community metrics headers; otherwise, <see langword="false"/>.</returns>
    bool ShouldIncludeCommunityMetrics(string relativePath);
}

/// <summary>
/// Represents the aggregate BE Home metrics returned for display.
/// Identity-based device counts are directional estimates and not authoritative Board hardware totals.
/// </summary>
public sealed class BeHomeAggregateMetrics
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomeAggregateMetrics"/> class.
    /// </summary>
    /// <param name="activeNowTotal">The total number of active BE Home sessions.</param>
    /// <param name="activeNowAnonymous">The number of active anonymous BE Home sessions.</param>
    /// <param name="activeNowSignedIn">The number of active signed-in BE Home sessions.</param>
    /// <param name="totalBoardsSeen">The estimated number of distinct BE Home device identities observed over time.</param>
    /// <param name="dailyActiveDevices">The estimated number of distinct BE Home device identities seen in the last day.</param>
    /// <param name="weeklyActiveDevices">The estimated number of distinct BE Home device identities seen in the last week.</param>
    /// <param name="monthlyActiveDevices">The estimated number of distinct BE Home device identities seen in the last month.</param>
    /// <param name="updatedAt">The timestamp when the metric payload was generated.</param>
    public BeHomeAggregateMetrics(
        int activeNowTotal,
        int activeNowAnonymous,
        int activeNowSignedIn,
        int totalBoardsSeen,
        int dailyActiveDevices,
        int weeklyActiveDevices,
        int monthlyActiveDevices,
        DateTimeOffset updatedAt)
    {
        ActiveNowTotal = activeNowTotal;
        ActiveNowAnonymous = activeNowAnonymous;
        ActiveNowSignedIn = activeNowSignedIn;
        TotalBoardsSeen = totalBoardsSeen;
        DailyActiveDevices = dailyActiveDevices;
        WeeklyActiveDevices = weeklyActiveDevices;
        MonthlyActiveDevices = monthlyActiveDevices;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Gets the total number of active BE Home sessions.
    /// </summary>
    public int ActiveNowTotal { get; }

    /// <summary>
    /// Gets the number of active anonymous BE Home sessions.
    /// </summary>
    public int ActiveNowAnonymous { get; }

    /// <summary>
    /// Gets the number of active signed-in BE Home sessions.
    /// </summary>
    public int ActiveNowSignedIn { get; }

    /// <summary>
    /// Gets the estimated number of distinct BE Home device identities observed over time.
    /// </summary>
    public int TotalBoardsSeen { get; }

    /// <summary>
    /// Gets the estimated number of distinct BE Home device identities seen in the last day.
    /// </summary>
    public int DailyActiveDevices { get; }

    /// <summary>
    /// Gets the estimated number of distinct BE Home device identities seen in the last week.
    /// </summary>
    public int WeeklyActiveDevices { get; }

    /// <summary>
    /// Gets the estimated number of distinct BE Home device identities seen in the last month.
    /// </summary>
    public int MonthlyActiveDevices { get; }

    /// <summary>
    /// Gets the timestamp when the metric payload was generated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; }
}

/// <summary>
/// Provides a stable raw device identifier for BE Home analytics.
/// </summary>
public interface IBeHomeDeviceIdentityProvider
{
    /// <summary>
    /// Gets the preferred device identity for the current BE Home install.
    /// </summary>
    /// <returns>The raw device identity selected for the current device.</returns>
    BeHomeDeviceIdentity GetDeviceIdentity();
}

/// <summary>
/// Provides storage for a generated BE Home install identifier.
/// </summary>
public interface IBeHomeInstallIdStore
{
    /// <summary>
    /// Gets the persisted install identifier, creating it when needed.
    /// </summary>
    /// <returns>The persisted install identifier.</returns>
    string GetOrCreateInstallId();
}
}
