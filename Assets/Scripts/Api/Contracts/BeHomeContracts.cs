using System;

namespace BoardEnthusiasts.BeHome.Api.Contracts
{
/// <summary>
/// Wire-format request payload for a BE Home presence heartbeat.
/// </summary>
[Serializable]
public sealed class BeHomePresenceRequestDto
{
    /// <summary>
    /// The per-launch BE Home session identifier.
    /// </summary>
    public string sessionId;

    /// <summary>
    /// The raw device identifier to hash server-side.
    /// </summary>
    public string deviceId;

    /// <summary>
    /// The current BE Home auth state.
    /// </summary>
    public string authState;

    /// <summary>
    /// The source used to produce the raw device identifier.
    /// </summary>
    public string deviceIdSource;

    /// <summary>
    /// The BE Home client version.
    /// </summary>
    public string clientVersion;

    /// <summary>
    /// The maintained BE environment name targeted by the current build.
    /// </summary>
    public string appEnvironment;
}

/// <summary>
/// Wire-format response payload for a BE Home presence heartbeat.
/// </summary>
[Serializable]
public sealed class BeHomePresenceResponseDto
{
    /// <summary>
    /// Indicates whether the backend accepted the heartbeat.
    /// </summary>
    public bool accepted;

    /// <summary>
    /// The current BE Home session state.
    /// </summary>
    public BeHomePresenceSessionDto session;
}

/// <summary>
/// Wire-format session state returned for a BE Home presence heartbeat.
/// </summary>
[Serializable]
public sealed class BeHomePresenceSessionDto
{
    /// <summary>
    /// The per-launch BE Home session identifier.
    /// </summary>
    public string sessionId;

    /// <summary>
    /// The current BE Home auth state.
    /// </summary>
    public string authState;

    /// <summary>
    /// The timestamp of the last accepted heartbeat.
    /// </summary>
    public string lastSeenAt;

    /// <summary>
    /// The recommended heartbeat interval in seconds.
    /// </summary>
    public int heartbeatIntervalSeconds;

    /// <summary>
    /// The active-session TTL in seconds.
    /// </summary>
    public int activeTtlSeconds;
}

/// <summary>
/// Wire-format request payload for a BE Home disconnect event.
/// </summary>
[Serializable]
public sealed class BeHomePresenceEndRequestDto
{
    /// <summary>
    /// The per-launch BE Home session identifier.
    /// </summary>
    public string sessionId;
}

/// <summary>
/// Wire-format response payload for a BE Home disconnect event.
/// </summary>
[Serializable]
public sealed class BeHomePresenceEndResponseDto
{
    /// <summary>
    /// Indicates whether the backend accepted the disconnect request.
    /// </summary>
    public bool accepted;

    /// <summary>
    /// The ended BE Home session state.
    /// </summary>
    public BeHomeEndedSessionDto session;
}

/// <summary>
/// Wire-format ended-session payload for a BE Home disconnect event.
/// </summary>
[Serializable]
public sealed class BeHomeEndedSessionDto
{
    /// <summary>
    /// The per-launch BE Home session identifier.
    /// </summary>
    public string sessionId;

    /// <summary>
    /// The timestamp when the backend marked the session as ended.
    /// </summary>
    public string endedAt;
}

/// <summary>
/// Wire-format response payload for a BE Home metrics request.
/// </summary>
[Serializable]
public sealed class BeHomeMetricsResponseDto
{
    /// <summary>
    /// The current BE Home aggregate metrics.
    /// </summary>
    public BeHomeMetricsDto metrics;
}

/// <summary>
/// Wire-format metrics payload for BE Home aggregate analytics.
/// </summary>
[Serializable]
public sealed class BeHomeMetricsDto
{
    /// <summary>
    /// The total number of active BE Home sessions.
    /// </summary>
    public int activeNowTotal;

    /// <summary>
    /// The number of active anonymous BE Home sessions.
    /// </summary>
    public int activeNowAnonymous;

    /// <summary>
    /// The number of active signed-in BE Home sessions.
    /// </summary>
    public int activeNowSignedIn;

    /// <summary>
    /// The total number of distinct devices that have run BE Home.
    /// </summary>
    public int totalBoardsSeen;

    /// <summary>
    /// The number of distinct devices seen in the last day.
    /// </summary>
    public int dailyActiveDevices;

    /// <summary>
    /// The number of distinct devices seen in the last week.
    /// </summary>
    public int weeklyActiveDevices;

    /// <summary>
    /// The number of distinct devices seen in the last month.
    /// </summary>
    public int monthlyActiveDevices;

    /// <summary>
    /// The timestamp when the metric payload was generated.
    /// </summary>
    public string updatedAt;
}
}
