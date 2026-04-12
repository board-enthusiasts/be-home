using System;

/// <summary>
/// Defines hosted website routes that are intentionally unavailable inside BE Home's thin-layer browser experience.
/// </summary>
public static class BeHomeThinLayerRoutePolicy
{
    /// <summary>
    /// The notice title shown when a blocked website workspace is requested inside BE Home.
    /// </summary>
    public const string RestrictedWorkspaceNoticeTitle = "Use the full site for developer and moderation tools";

    /// <summary>
    /// The notice body shown when a blocked website workspace is requested inside BE Home.
    /// </summary>
    public const string RestrictedWorkspaceNoticeMessage = "Developer and moderation workflows are only available on the full Board Enthusiasts website.";

    /// <summary>
    /// Determines whether the supplied hosted BE route or absolute URL points to a workspace that is blocked inside BE Home.
    /// </summary>
    /// <param name="routeOrUrl">The hosted route or absolute URL to inspect.</param>
    /// <returns>True when the path points at the developer or moderation workspace.</returns>
    public static bool IsRestrictedWorkspaceRoute(string routeOrUrl)
    {
        if (string.IsNullOrWhiteSpace(routeOrUrl))
        {
            return false;
        }

        string path = TryExtractPath(routeOrUrl);
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        return MatchesWorkspacePath(path, "/developer")
            || MatchesWorkspacePath(path, "/moderate");
    }

    private static string TryExtractPath(string routeOrUrl)
    {
        string trimmed = routeOrUrl.Trim();
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.AbsolutePath ?? string.Empty;
        }

        int terminatorIndex = trimmed.IndexOfAny(new[] { '?', '#' });
        if (terminatorIndex >= 0)
        {
            trimmed = trimmed.Substring(0, terminatorIndex);
        }

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return string.Empty;
        }

        return trimmed.StartsWith("/", StringComparison.Ordinal) ? trimmed : "/" + trimmed.TrimStart('/');
    }

    private static bool MatchesWorkspacePath(string path, string workspaceRoot)
    {
        string normalizedPath = path.TrimEnd('/');
        return string.Equals(normalizedPath, workspaceRoot, StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(workspaceRoot + "/", StringComparison.OrdinalIgnoreCase);
    }
}
