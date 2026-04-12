/// <summary>
/// Tracks whether a browse-surface load failure should be treated as a full-page outage.
/// </summary>
public sealed class BeHomeBrowseNavigationPolicy
{
    private bool _hasLoadedPrimaryContent;
    private bool _topLevelNavigationPending;
    private bool _blankPageRecoveryAttempted;

    /// <summary>
    /// Records that the shell has initiated a top-level browse navigation.
    /// </summary>
    public void BeginTopLevelNavigation()
    {
        _topLevelNavigationPending = true;
    }

    /// <summary>
    /// Records that the primary BE browse content finished loading successfully.
    /// </summary>
    public void MarkPrimaryContentLoaded()
    {
        _hasLoadedPrimaryContent = true;
        _topLevelNavigationPending = false;
        _blankPageRecoveryAttempted = false;
    }

    /// <summary>
    /// Records that the primary BE browse content is unavailable.
    /// </summary>
    public void MarkPrimaryContentUnavailable()
    {
        _hasLoadedPrimaryContent = false;
        _topLevelNavigationPending = false;
        _blankPageRecoveryAttempted = false;
    }

    /// <summary>
    /// Gets a value indicating whether the current browse load failure should surface the offline shell.
    /// </summary>
    /// <returns><see langword="true"/> when the failure applies to the primary page load; otherwise, <see langword="false"/>.</returns>
    public bool ShouldTreatLoadErrorAsUnavailable()
    {
        return !_hasLoadedPrimaryContent || _topLevelNavigationPending;
    }

    /// <summary>
    /// Gets a value indicating whether a sudden blank-page load should trigger a one-time WebView recovery attempt.
    /// </summary>
    /// <returns><see langword="true"/> when the browse surface had already loaded successfully and has not used its recovery attempt yet; otherwise, <see langword="false"/>.</returns>
    public bool ShouldAttemptBlankPageRecovery()
    {
        return _hasLoadedPrimaryContent
            && !_topLevelNavigationPending
            && !_blankPageRecoveryAttempted;
    }

    /// <summary>
    /// Records that the browse surface has spent its current blank-page recovery attempt.
    /// </summary>
    public void MarkBlankPageRecoveryAttempted()
    {
        _blankPageRecoveryAttempted = true;
    }
}
