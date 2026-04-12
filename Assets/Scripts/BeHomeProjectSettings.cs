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
/// Defines which BE Home shell implementation should be baked into the current build.
/// </summary>
public enum BeHomeUiImplementationMode
{
    /// <summary>
    /// Uses the maintained hosted website inside the embedded Android WebView shell.
    /// </summary>
    HostedWebView = 0,

    /// <summary>
    /// Uses the experimental native UI Toolkit catalog shell instead of the hosted browse site.
    /// </summary>
    NativeCatalogSpike = 1,
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

    /// <summary>
    /// The hosted API base URL for the production BE environment.
    /// </summary>
    public const string ProductionApiBaseUrl = "https://api.boardenthusiasts.com";

    /// <summary>
    /// The hosted API base URL for the staging BE environment.
    /// </summary>
    public const string StagingApiBaseUrl = "https://api.staging.boardenthusiasts.com";

    [SerializeField]
    private BeHomeTargetEnvironment m_targetEnvironment = BeHomeTargetEnvironment.Production;

    [SerializeField]
    private BeHomeBrowsePresentationMode m_browsePresentationMode = BeHomeBrowsePresentationMode.FullWebsite;

    [SerializeField]
    private BeHomeUiImplementationMode m_uiImplementationMode = BeHomeUiImplementationMode.HostedWebView;

    /// <summary>
    /// Gets the configured hosted website environment for the current build.
    /// </summary>
    public BeHomeTargetEnvironment TargetEnvironment => m_targetEnvironment;

    /// <summary>
    /// Gets the configured hosted browse presentation mode for the current build.
    /// </summary>
    public BeHomeBrowsePresentationMode BrowsePresentationMode => m_browsePresentationMode;

    /// <summary>
    /// Gets the configured BE Home shell implementation for the current build.
    /// </summary>
    public BeHomeUiImplementationMode UiImplementationMode => m_uiImplementationMode;

    /// <summary>
    /// Gets the configured hosted browse URL for the current build.
    /// </summary>
    public string BrowsePageUrl => ResolveBrowsePageUrl(m_targetEnvironment, m_browsePresentationMode);

    /// <summary>
    /// Gets the configured hosted API base URL for the current build.
    /// </summary>
    public string ApiBaseUrl => ResolveApiBaseUrl(m_targetEnvironment);

    /// <summary>
    /// Gets the maintained BE environment name for the current build.
    /// </summary>
    public string AppEnvironmentName => ResolveAppEnvironmentName(m_targetEnvironment);

#if UNITY_EDITOR
    /// <summary>
    /// Updates the configured hosted website environment for build-time asset synchronization.
    /// </summary>
    /// <param name="targetEnvironment">The hosted website environment to persist into the runtime settings asset.</param>
    public void SetTargetEnvironment(BeHomeTargetEnvironment targetEnvironment)
    {
        m_targetEnvironment = targetEnvironment;
    }

    /// <summary>
    /// Updates the configured BE Home shell implementation for build-time asset synchronization.
    /// </summary>
    /// <param name="uiImplementationMode">The shell implementation to persist into the runtime settings asset.</param>
    public void SetUiImplementationMode(BeHomeUiImplementationMode uiImplementationMode)
    {
        m_uiImplementationMode = uiImplementationMode;
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
    /// Resolves the hosted API base URL that BE Home should use at runtime.
    /// </summary>
    /// <returns>The configured API base URL, or the production API base URL when no settings asset is present.</returns>
    public static string GetConfiguredApiBaseUrl()
    {
        var settings = Load();
        return settings != null
            ? settings.ApiBaseUrl
            : ProductionApiBaseUrl;
    }

    /// <summary>
    /// Resolves the configured BE Home shell implementation for the current build.
    /// </summary>
    /// <returns>The configured shell implementation, or <see cref="BeHomeUiImplementationMode.HostedWebView"/> when no settings asset is present.</returns>
    public static BeHomeUiImplementationMode GetConfiguredUiImplementationMode()
    {
        var settings = Load();
        return settings != null
            ? settings.UiImplementationMode
            : BeHomeUiImplementationMode.HostedWebView;
    }

    /// <summary>
    /// Resolves the maintained BE environment name for the current build.
    /// </summary>
    /// <returns>The configured BE environment name, or <c>production</c> when no settings asset is present.</returns>
    public static string GetConfiguredAppEnvironmentName()
    {
        var settings = Load();
        return settings != null
            ? settings.AppEnvironmentName
            : ResolveAppEnvironmentName(BeHomeTargetEnvironment.Production);
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

    /// <summary>
    /// Resolves the hosted API base URL for the given environment.
    /// </summary>
    /// <param name="targetEnvironment">The configured hosted website environment.</param>
    /// <returns>The fully resolved API base URL for the supplied environment.</returns>
    public static string ResolveApiBaseUrl(BeHomeTargetEnvironment targetEnvironment)
    {
        return targetEnvironment == BeHomeTargetEnvironment.Staging
            ? StagingApiBaseUrl
            : ProductionApiBaseUrl;
    }

    /// <summary>
    /// Resolves the maintained BE environment name for the given environment selection.
    /// </summary>
    /// <param name="targetEnvironment">The configured hosted website environment.</param>
    /// <returns>The BE environment name used in internal telemetry payloads.</returns>
    public static string ResolveAppEnvironmentName(BeHomeTargetEnvironment targetEnvironment)
    {
        return targetEnvironment == BeHomeTargetEnvironment.Staging
            ? "staging"
            : "production";
    }
}
