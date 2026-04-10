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
/// Defines which hosted BE website environment BE Home should target.
/// </summary>
public enum BeHomeTargetEnvironment
{
    /// <summary>
    /// Targets the public production BE website.
    /// </summary>
    Production = 0,

    /// <summary>
    /// Targets the staging BE website.
    /// </summary>
    Staging = 1,
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
    /// The hosted browse URL for the production full website experience.
    /// </summary>
    public const string ProductionFullWebsiteBrowsePageUrl = "https://boardenthusiasts.com/browse";

    /// <summary>
    /// The hosted browse URL for the production Board-embedded web experience.
    /// </summary>
    public const string ProductionEmbeddedBrowsePageUrl = "https://boardenthusiasts.com/browse?embed=board";

    /// <summary>
    /// The hosted browse URL for the staging full website experience.
    /// </summary>
    public const string StagingFullWebsiteBrowsePageUrl = "https://staging.boardenthusiasts.com/browse";

    /// <summary>
    /// The hosted browse URL for the staging Board-embedded web experience.
    /// </summary>
    public const string StagingEmbeddedBrowsePageUrl = "https://staging.boardenthusiasts.com/browse?embed=board";

    [SerializeField]
    private BeHomeTargetEnvironment m_targetEnvironment = BeHomeTargetEnvironment.Production;

    [SerializeField]
    private BeHomeBrowsePresentationMode m_browsePresentationMode = BeHomeBrowsePresentationMode.FullWebsite;

    /// <summary>
    /// Gets the configured hosted website environment for the current build.
    /// </summary>
    public BeHomeTargetEnvironment TargetEnvironment => m_targetEnvironment;

    /// <summary>
    /// Gets the configured hosted browse presentation mode for the current build.
    /// </summary>
    public BeHomeBrowsePresentationMode BrowsePresentationMode => m_browsePresentationMode;

    /// <summary>
    /// Gets the configured hosted browse URL for the current build.
    /// </summary>
    public string BrowsePageUrl => ResolveBrowsePageUrl(m_targetEnvironment, m_browsePresentationMode);

#if UNITY_EDITOR
    /// <summary>
    /// Updates the configured hosted website environment for build-time asset synchronization.
    /// </summary>
    /// <param name="targetEnvironment">The hosted website environment to persist into the runtime settings asset.</param>
    public void SetTargetEnvironment(BeHomeTargetEnvironment targetEnvironment)
    {
        m_targetEnvironment = targetEnvironment;
    }
#endif

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
        return settings != null
            ? settings.BrowsePageUrl
            : ProductionFullWebsiteBrowsePageUrl;
    }

    /// <summary>
    /// Resolves the hosted browse URL for the given environment and presentation mode.
    /// </summary>
    /// <param name="targetEnvironment">The configured hosted website environment.</param>
    /// <param name="browsePresentationMode">The configured website presentation mode.</param>
    /// <returns>The fully resolved browse URL for the supplied settings.</returns>
    public static string ResolveBrowsePageUrl(
        BeHomeTargetEnvironment targetEnvironment,
        BeHomeBrowsePresentationMode browsePresentationMode)
    {
        if (targetEnvironment == BeHomeTargetEnvironment.Staging)
        {
            return browsePresentationMode == BeHomeBrowsePresentationMode.EmbeddedBoardShell
                ? StagingEmbeddedBrowsePageUrl
                : StagingFullWebsiteBrowsePageUrl;
        }

        return browsePresentationMode == BeHomeBrowsePresentationMode.EmbeddedBoardShell
            ? ProductionEmbeddedBrowsePageUrl
            : ProductionFullWebsiteBrowsePageUrl;
    }
}
