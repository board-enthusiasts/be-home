using UnityEngine;

/// <summary>
/// Defines how BE Home should present the hosted Board Enthusiasts browse experience.
/// </summary>
public enum BeHomeBrowsePresentationMode
{
    /// <summary>
    /// Loads the full hosted website, including the website-owned shell and sign-in UI.
    /// </summary>
    FullWebsite = 0,

    /// <summary>
    /// Loads the hosted website with the BE Home embed query enabled so the web shell hides its own chrome.
    /// </summary>
    EmbeddedBoardShell = 1,
}

/// <summary>
/// Runtime-configurable project settings used by the BE Home Unity client.
/// </summary>
public sealed class BeHomeProjectSettings : ScriptableObject
{
    /// <summary>
    /// The resource path used to load the maintained BE Home settings asset at runtime.
    /// </summary>
    public const string ResourcePath = "Settings/BeHomeProjectSettings";

    /// <summary>
    /// The hosted browse URL for the full website experience.
    /// </summary>
    public const string FullWebsiteBrowsePageUrl = "https://staging.boardenthusiasts.com/browse";

    /// <summary>
    /// The hosted browse URL for the Board-embedded web experience.
    /// </summary>
    public const string EmbeddedBrowsePageUrl = "https://staging.boardenthusiasts.com/browse?embed=board";

    [SerializeField]
    private BeHomeBrowsePresentationMode m_browsePresentationMode = BeHomeBrowsePresentationMode.FullWebsite;

    /// <summary>
    /// Gets the configured hosted browse presentation mode for the current build.
    /// </summary>
    public BeHomeBrowsePresentationMode BrowsePresentationMode => m_browsePresentationMode;

    /// <summary>
    /// Gets the configured hosted browse URL for the current build.
    /// </summary>
    public string BrowsePageUrl => m_browsePresentationMode == BeHomeBrowsePresentationMode.EmbeddedBoardShell
        ? EmbeddedBrowsePageUrl
        : FullWebsiteBrowsePageUrl;

    /// <summary>
    /// Loads the maintained BE Home settings asset from resources when it is available.
    /// </summary>
    /// <returns>The configured settings asset, or <see langword="null"/> when no asset has been created.</returns>
    public static BeHomeProjectSettings Load()
    {
        return Resources.Load<BeHomeProjectSettings>(ResourcePath);
    }

    /// <summary>
    /// Resolves the hosted browse URL that BE Home should open at runtime.
    /// </summary>
    /// <returns>The configured browse URL, or the full website URL when no settings asset is present.</returns>
    public static string GetConfiguredBrowsePageUrl()
    {
        var settings = Load();
        return settings != null ? settings.BrowsePageUrl : FullWebsiteBrowsePageUrl;
    }
}
