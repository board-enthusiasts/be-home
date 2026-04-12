using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using BoardEnthusiasts.BeHome.Api.Contracts;
using BoardEnthusiasts.BeHome.Api.Http;
using BoardEnthusiasts.BeHome.Api.Models;

namespace BoardEnthusiasts.BeHome.Api.Services
{
/// <summary>
/// Defines presence-specific API operations for BE Home.
/// </summary>
public interface IBeHomePresenceService
{
    /// <summary>
    /// Registers the current BE Home session when the native shell first loads.
    /// </summary>
    /// <param name="presence">The current BE Home presence snapshot.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A task that completes when the registration request has finished.</returns>
    Task RegisterSessionAsync(BeHomePresenceSnapshot presence, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a best-effort disconnect request for the supplied BE Home session.
    /// </summary>
    /// <param name="sessionId">The current BE Home session identifier.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A task that completes when the disconnect request has finished.</returns>
    Task EndSessionAsync(string sessionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines aggregate metrics operations for BE Home.
/// </summary>
public interface IBeHomeMetricsService
{
    /// <summary>
    /// Fetches the current aggregate BE Home metrics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The current aggregate BE Home metrics payload.</returns>
    Task<BeHomeAggregateMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines title analytics operations for BE Home.
/// </summary>
public interface IBeHomeTitleAnalyticsService
{
    /// <summary>
    /// Records that the hosted BE Home surface opened a title detail page.
    /// </summary>
    /// <param name="record">The title detail view to record.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>A task that completes when the backend has accepted the event.</returns>
    Task RecordTitleDetailViewAsync(BeHomeTitleDetailViewRecord record, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implements presence-specific API operations for BE Home.
/// </summary>
public sealed class BeHomePresenceService : IBeHomePresenceService
{
    private const string PresenceRoute = "/internal/be-home/presence";
    private const string PresenceEndRoute = "/internal/be-home/presence/end";
    private readonly IBeHomeApiTransport _transport;

    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomePresenceService"/> class.
    /// </summary>
    /// <param name="transport">The maintained BE Home API transport.</param>
    public BeHomePresenceService(IBeHomeApiTransport transport)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    }

    /// <inheritdoc/>
    public async Task RegisterSessionAsync(BeHomePresenceSnapshot presence, CancellationToken cancellationToken = default)
    {
        if (presence == null)
        {
            throw new ArgumentNullException(nameof(presence));
        }

        var response = await _transport
            .PostJsonAsync<BeHomePresenceRequestDto, BeHomePresenceResponseDto>(
                PresenceRoute,
                new BeHomePresenceRequestDto
                {
                    sessionId = presence.SessionId,
                    deviceId = presence.DeviceIdentity.RawIdentifier,
                    deviceIdSource = MapDeviceIdSource(presence.DeviceIdentity.Source),
                    authState = MapAuthState(presence.AuthState),
                    clientVersion = presence.ClientVersion,
                    appEnvironment = presence.AppEnvironment,
                },
                cancellationToken)
            .ConfigureAwait(false);

        if (response == null || !response.accepted || response.session == null)
        {
            throw new BeHomeApiException("The BE API did not accept the BE Home initial presence registration.");
        }
    }

    /// <inheritdoc/>
    public async Task EndSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("A session id is required.", nameof(sessionId));
        }

        var response = await _transport
            .PostJsonAsync<BeHomePresenceEndRequestDto, BeHomePresenceEndResponseDto>(
                PresenceEndRoute,
                new BeHomePresenceEndRequestDto
                {
                    sessionId = sessionId,
                },
                cancellationToken)
            .ConfigureAwait(false);

        if (response == null || !response.accepted || response.session == null || string.IsNullOrWhiteSpace(response.session.sessionId))
        {
            throw new BeHomeApiException("The BE API did not accept the BE Home disconnect request.");
        }
    }

    private static string MapAuthState(BeHomeAuthState authState)
    {
        return authState == BeHomeAuthState.SignedIn
            ? "signed_in"
            : "anonymous";
    }
    private static string MapDeviceIdSource(BeHomeDeviceIdSource source)
    {
        return source switch
        {
            BeHomeDeviceIdSource.AndroidSecureAndroidId => "android_secure_android_id",
            _ => "install_id",
        };
    }
}

/// <summary>
/// Implements aggregate metrics operations for BE Home.
/// </summary>
public sealed class BeHomeMetricsService : IBeHomeMetricsService
{
    private const string MetricsRoute = "/internal/be-home/metrics";
    private readonly IBeHomeApiTransport _transport;

    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomeMetricsService"/> class.
    /// </summary>
    /// <param name="transport">The maintained BE Home API transport.</param>
    public BeHomeMetricsService(IBeHomeApiTransport transport)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    }

    /// <inheritdoc/>
    public async Task<BeHomeAggregateMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _transport
            .GetAsync<BeHomeMetricsResponseDto>(MetricsRoute, cancellationToken)
            .ConfigureAwait(false);

        if (response?.metrics == null)
        {
            throw new BeHomeApiException("The BE API returned an empty BE Home metrics payload.");
        }

        return new BeHomeAggregateMetrics(
            response.metrics.activeNowTotal,
            response.metrics.activeNowAnonymous,
            response.metrics.activeNowSignedIn,
            response.metrics.totalBoardsSeen,
            response.metrics.dailyActiveDevices,
            response.metrics.weeklyActiveDevices,
            response.metrics.monthlyActiveDevices,
            ParseTimestamp(response.metrics.updatedAt, nameof(response.metrics.updatedAt)));
    }

    private static DateTimeOffset ParseTimestamp(string value, string fieldName)
    {
        if (DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            return parsed;
        }

        throw new BeHomeApiException($"The BE API returned an invalid {fieldName} timestamp.");
    }
}

/// <summary>
/// Implements title analytics operations for BE Home.
/// </summary>
public sealed class BeHomeTitleAnalyticsService : IBeHomeTitleAnalyticsService
{
    private const string TitleDetailViewsRoute = "/internal/be-home/title-detail-views";
    private readonly IBeHomeApiTransport _transport;

    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomeTitleAnalyticsService"/> class.
    /// </summary>
    /// <param name="transport">The maintained BE Home API transport.</param>
    public BeHomeTitleAnalyticsService(IBeHomeApiTransport transport)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    }

    /// <inheritdoc/>
    public async Task RecordTitleDetailViewAsync(BeHomeTitleDetailViewRecord record, CancellationToken cancellationToken = default)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        var response = await _transport
            .PostJsonAsync<BeHomeTitleDetailViewRequestDto, BeHomeTitleDetailViewResponseDto>(
                TitleDetailViewsRoute,
                new BeHomeTitleDetailViewRequestDto
                {
                    titleId = record.TitleId,
                    studioSlug = record.StudioSlug,
                    titleSlug = record.TitleSlug,
                    route = record.Route,
                    surface = record.Surface,
                },
                cancellationToken)
            .ConfigureAwait(false);

        if (response == null || !response.accepted)
        {
            throw new BeHomeApiException("The BE API did not accept the BE Home title detail view.");
        }
    }
}
}
