using System;
using System.Collections.Generic;

[Serializable]
public sealed class BeHomeBrowseDiagnostics
{
    public string type;
    public string surface;
    public string route;
    public string titleId;
    public string studioId;
    public string studioSlug;
    public string titleSlug;
    public string titleDisplayName;
    public string studioDisplayName;
    public string contentKind;
    public string selectedPreviewKind;
    public string selectedPreviewHost;
    public string heroImageHost;
    public string cardImageHost;
    public string acquisitionHost;
    public int showcaseMediaCount;
    public int showcaseImageCount;
    public int showcaseVideoCount;
    public int searchResultCount;
    public int totalCatalogCount;
    public int currentPage;
    public int searchQueryLength;
    public int selectedStudiosCount;
    public int selectedGenresCount;
    public bool hasHeroImage;
    public bool hasCardImage;
    public bool hasLogoImage;
    public bool hasAcquisitionUrl;
}

public static class BeHomeBrowseDiagnosticsFormatter
{
    public static string Summarize(BeHomeBrowseDiagnostics diagnostics)
    {
        if (diagnostics == null)
        {
            return "(none)";
        }

        var parts = new List<string>();
        AddNamedValue(parts, "surface", diagnostics.surface);
        AddNamedValue(parts, "route", diagnostics.route);

        var titleSummary = BuildEntitySummary(diagnostics.titleDisplayName, diagnostics.titleSlug, diagnostics.titleId);
        if (!string.IsNullOrWhiteSpace(titleSummary))
        {
            parts.Add($"title={titleSummary}");
        }

        var studioSummary = BuildEntitySummary(diagnostics.studioDisplayName, diagnostics.studioSlug, diagnostics.studioId);
        if (!string.IsNullOrWhiteSpace(studioSummary))
        {
            parts.Add($"studio={studioSummary}");
        }

        AddNamedValue(parts, "content", diagnostics.contentKind);
        AddNamedValue(parts, "preview", BuildPreviewSummary(diagnostics.selectedPreviewKind, diagnostics.selectedPreviewHost));
        AddNamedValue(parts, "heroHost", diagnostics.heroImageHost);
        AddNamedValue(parts, "cardHost", diagnostics.cardImageHost);
        AddNamedValue(parts, "acquisitionHost", diagnostics.acquisitionHost);

        if (string.Equals(diagnostics.surface, "browse", StringComparison.Ordinal))
        {
            parts.Add($"results={Math.Max(0, diagnostics.searchResultCount)}/{Math.Max(0, diagnostics.totalCatalogCount)}");
            parts.Add($"page={Math.Max(1, diagnostics.currentPage)}");
            parts.Add(
                $"filters=query:{Math.Max(0, diagnostics.searchQueryLength)},studios:{Math.Max(0, diagnostics.selectedStudiosCount)},genres:{Math.Max(0, diagnostics.selectedGenresCount)}");
        }
        else
        {
            parts.Add(
                $"showcase={Math.Max(0, diagnostics.showcaseMediaCount)} (images={Math.Max(0, diagnostics.showcaseImageCount)}, videos={Math.Max(0, diagnostics.showcaseVideoCount)})");
        }

        var assetFlags = new List<string>();
        if (diagnostics.hasHeroImage)
        {
            assetFlags.Add("hero");
        }

        if (diagnostics.hasCardImage)
        {
            assetFlags.Add("card");
        }

        if (diagnostics.hasLogoImage)
        {
            assetFlags.Add("logo");
        }

        if (diagnostics.hasAcquisitionUrl)
        {
            assetFlags.Add("acquisition");
        }

        if (assetFlags.Count > 0)
        {
            parts.Add($"assets={string.Join(",", assetFlags)}");
        }

        return parts.Count == 0 ? "(empty)" : string.Join(" | ", parts);
    }

    private static void AddNamedValue(List<string> parts, string label, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        parts.Add($"{label}={value}");
    }

    private static string BuildEntitySummary(string displayName, string slug, string id)
    {
        if (!string.IsNullOrWhiteSpace(displayName) && !string.IsNullOrWhiteSpace(slug))
        {
            return $"{displayName} [{slug}]";
        }

        if (!string.IsNullOrWhiteSpace(displayName) && !string.IsNullOrWhiteSpace(id))
        {
            return $"{displayName} [{id}]";
        }

        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName;
        }

        if (!string.IsNullOrWhiteSpace(slug))
        {
            return slug;
        }

        return id;
    }

    private static string BuildPreviewSummary(string kind, string host)
    {
        if (string.IsNullOrWhiteSpace(kind))
        {
            return host;
        }

        if (string.IsNullOrWhiteSpace(host))
        {
            return kind;
        }

        return $"{kind}@{host}";
    }
}
