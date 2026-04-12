namespace BoardEnthusiasts.BeHome.UI.NativeBrowse
{
/// <summary>
/// Defines the static copy and fixed configuration for the native BE Home browse spike.
/// </summary>
internal sealed class BeHomeNativeBrowseModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomeNativeBrowseModel"/> class.
    /// </summary>
    /// <param name="appEnvironmentName">The configured BE environment name for the current build.</param>
    /// <param name="apiBaseUrl">The configured BE API base URL for the current build.</param>
    public BeHomeNativeBrowseModel(string appEnvironmentName, string apiBaseUrl)
    {
        AppEnvironmentName = string.IsNullOrWhiteSpace(appEnvironmentName) ? "production" : appEnvironmentName;
        ApiBaseUrl = apiBaseUrl ?? string.Empty;
    }

    /// <summary>
    /// Gets the current default native catalog page size.
    /// </summary>
    public int PageSize => 48;

    /// <summary>
    /// Gets the heading copy for the native browse spike.
    /// </summary>
    public string HeadingText => "Native Browse Spike";

    /// <summary>
    /// Gets the subtitle copy for the native browse spike.
    /// </summary>
    public string SubtitleText => "UI Toolkit browse proof-of-concept backed directly by the public BE API.";

    /// <summary>
    /// Gets the configured BE environment name for the current build.
    /// </summary>
    public string AppEnvironmentName { get; }

    /// <summary>
    /// Gets the configured BE API base URL for the current build.
    /// </summary>
    public string ApiBaseUrl { get; }
}
}
