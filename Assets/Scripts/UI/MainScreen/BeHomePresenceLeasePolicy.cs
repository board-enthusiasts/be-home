using System;

/// <summary>
/// Encapsulates the lightweight lease-renewal cadence used for BE Home presence refreshes.
/// </summary>
public sealed class BeHomePresenceLeasePolicy
{
    private readonly float _renewAfterSeconds;

    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomePresenceLeasePolicy"/> class.
    /// </summary>
    /// <param name="renewAfterSeconds">The minimum elapsed seconds before presence should be renewed again.</param>
    public BeHomePresenceLeasePolicy(float renewAfterSeconds)
    {
        if (renewAfterSeconds <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(renewAfterSeconds), "The renew interval must be positive.");
        }

        _renewAfterSeconds = renewAfterSeconds;
    }

    /// <summary>
    /// Returns whether the BE Home presence lease should be renewed.
    /// </summary>
    /// <param name="lastSuccessfulRefreshAt">The timestamp of the last successful refresh in unscaled seconds.</param>
    /// <param name="now">The current unscaled timestamp in seconds.</param>
    /// <param name="refreshInFlight">Whether a refresh request is already running.</param>
    /// <returns><see langword="true"/> when a renewal should be attempted; otherwise, <see langword="false"/>.</returns>
    public bool ShouldRenew(float lastSuccessfulRefreshAt, float now, bool refreshInFlight)
    {
        if (refreshInFlight)
        {
            return false;
        }

        if (lastSuccessfulRefreshAt <= 0f)
        {
            return true;
        }

        return (now - lastSuccessfulRefreshAt) >= _renewAfterSeconds;
    }
}
