using System;

using Android;

using BoardEnthusiasts.BeHome.Api.Models;

using UnityEngine;

namespace BoardEnthusiasts.BeHome.Api.DeviceIdentity
{
/// <summary>
/// Resolves the preferred BE Home device identity for analytics.
/// </summary>
public sealed class BeHomeDeviceIdentityProvider : IBeHomeDeviceIdentityProvider
{
    private readonly IBeHomeInstallIdStore _installIdStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomeDeviceIdentityProvider"/> class.
    /// </summary>
    /// <param name="installIdStore">The fallback install-id store used when no stable Android identifier is available.</param>
    public BeHomeDeviceIdentityProvider(IBeHomeInstallIdStore installIdStore)
    {
        _installIdStore = installIdStore ?? throw new ArgumentNullException(nameof(installIdStore));
    }

    /// <inheritdoc/>
    public BeHomeDeviceIdentity GetDeviceIdentity()
    {
        string androidId = TryGetAndroidSecureAndroidId();
        BeHomeDeviceIdentity selectedIdentity = BeHomeDeviceIdentitySelection.SelectPreferredIdentity(androidId);

        if (selectedIdentity != null)
        {
            LogDeviceIdentityProbe(androidId, selectedIdentity);
            return selectedIdentity;
        }

        BeHomeDeviceIdentity installIdentity = new BeHomeDeviceIdentity(_installIdStore.GetOrCreateInstallId(), BeHomeDeviceIdSource.InstallId);
        LogDeviceIdentityProbe(androidId, installIdentity);
        return installIdentity;
    }

    private static string TryGetAndroidSecureAndroidId()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var resolver = AndroidUtility.GetContentResolver();
            if (resolver == null)
            {
                return null;
            }

            using var secureSettings = new AndroidJavaClass(AndroidClassNames.SecureSettings);
            return BeHomeDeviceIdentitySelection.NormalizeAndroidId(
                secureSettings.CallStatic<string>(
                    AndroidMethodNames.GetString,
                    resolver,
                    AndroidSecureSettingsNames.AndroidId));
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[BeHomeDeviceIdentityProvider] Falling back to install id after ANDROID_ID lookup failed: {ex.Message}");
            return null;
        }
#else
        return null;
#endif
    }

    private static void LogDeviceIdentityProbe(
        string androidId,
        BeHomeDeviceIdentity selectedIdentity)
    {
        UnityEngine.Debug.Log(
            $"[BeHomeDeviceIdentityProvider] Device identity probe selected={selectedIdentity.Source} " +
            $"androidId={BeHomeDeviceIdentitySelection.DescribeAndroidIdCandidate(androidId)}");
    }
}

/// <summary>
/// Persists a generated BE Home install identifier in local Unity preferences.
/// </summary>
public sealed class PlayerPrefsBeHomeInstallIdStore : IBeHomeInstallIdStore
{
    /// <summary>
    /// The maintained player-preferences key for the generated BE Home install identifier.
    /// </summary>
    public const string InstallIdPlayerPrefsKey = "be_home.install_id";

    /// <inheritdoc/>
    public string GetOrCreateInstallId()
    {
        string existingValue = PlayerPrefs.GetString(InstallIdPlayerPrefsKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(existingValue))
        {
            return existingValue.Trim();
        }

        string installId = Guid.NewGuid().ToString("N");
        PlayerPrefs.SetString(InstallIdPlayerPrefsKey, installId);
        PlayerPrefs.Save();
        return installId;
    }
}
}
