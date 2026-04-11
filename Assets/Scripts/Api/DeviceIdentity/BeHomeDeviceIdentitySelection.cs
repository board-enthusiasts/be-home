#nullable enable
using System;

using BoardEnthusiasts.BeHome.Api.Models;

namespace BoardEnthusiasts.BeHome.Api.DeviceIdentity
{
/// <summary>
/// Provides testable selection and normalization logic for BE Home device identity candidates.
/// </summary>
public static class BeHomeDeviceIdentitySelection
{
    /// <summary>
    /// Normalizes Android's secure device identifier when it appears usable for BE Home analytics.
    /// </summary>
    /// <param name="rawValue">The raw Android identifier candidate.</param>
    /// <returns>The normalized identifier, or <see langword="null"/> when the candidate should be rejected.</returns>
    public static string? NormalizeAndroidId(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        string normalized = rawValue.Trim();
        if (string.Equals(normalized, "9774d56d682e549c", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "0000000000000000", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "unknown", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return normalized;
    }

    /// <summary>
    /// Chooses the preferred BE Home device identity candidate when it appears usable.
    /// </summary>
    /// <param name="androidId">The normalized Android secure identifier candidate.</param>
    /// <returns>The selected BE Home device identity, or <see langword="null"/> when the supplied candidate is unusable.</returns>
    public static BeHomeDeviceIdentity? SelectPreferredIdentity(string? androidId)
    {
        string? normalizedAndroidId = NormalizeAndroidId(androidId);
        if (!string.IsNullOrWhiteSpace(normalizedAndroidId))
        {
            return new BeHomeDeviceIdentity(normalizedAndroidId, BeHomeDeviceIdSource.AndroidSecureAndroidId);
        }

        return null;
    }

    /// <summary>
    /// Formats an Android identifier candidate for device-side diagnostics without logging the full raw value.
    /// </summary>
    /// <param name="rawValue">The raw Android identifier candidate.</param>
    /// <returns>A masked diagnostics string describing the candidate.</returns>
    public static string DescribeAndroidIdCandidate(string? rawValue)
    {
        string? normalized = NormalizeAndroidId(rawValue);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return "unavailable";
        }

        if (normalized.Length <= 8)
        {
            return normalized;
        }

        return $"{normalized.Substring(0, 4)}...{normalized.Substring(normalized.Length - 4, 4)}";
    }

    /// <summary>
}
}
#nullable restore
