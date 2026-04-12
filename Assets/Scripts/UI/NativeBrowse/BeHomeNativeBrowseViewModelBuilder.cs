using System;
using System.Collections.Generic;
using System.Linq;

using BoardEnthusiasts.BeHome.Api.Models;

namespace BoardEnthusiasts.BeHome.UI.NativeBrowse
{
/// <summary>
/// Creates native BE Home browse view models from public catalog responses and current UI state.
/// </summary>
internal static class BeHomeNativeBrowseViewModelBuilder
{
    /// <summary>
    /// Creates the loading-state view model for the native browse spike.
    /// </summary>
    /// <param name="model">The current native browse model.</param>
    /// <returns>The loading-state view model.</returns>
    public static BeHomeNativeBrowseViewModel CreateLoading(BeHomeNativeBrowseModel model)
    {
        return new BeHomeNativeBrowseViewModel
        {
            HeadingText = model?.HeadingText ?? string.Empty,
            SubtitleText = model?.SubtitleText ?? string.Empty,
            StatusText = "Loading public catalog...",
            MessageText = model != null
                ? $"Environment: {model.AppEnvironmentName} | API: {model.ApiBaseUrl}"
                : string.Empty,
            EmptyTitleText = "Loading catalog...",
            EmptyBodyText = "Pulling the current public BE catalog directly from the maintained API.",
            SelectedTitleHeading = "Select a title",
            SelectedTitleStudio = "Native detail selection will expand from this list-first spike.",
            SelectedTitleMeta = string.Empty,
            SelectedTitleDescription = "The first native slice keeps focus on the public catalog endpoint and UI Toolkit shell behavior.",
            IsLoading = true,
            ShowEmptyState = true,
        };
    }

    /// <summary>
    /// Creates the loaded-state view model for the native browse spike.
    /// </summary>
    /// <param name="model">The current native browse model.</param>
    /// <param name="page">The current public catalog page.</param>
    /// <param name="selectedTitleId">The previously selected title identifier when one exists.</param>
    /// <returns>The loaded-state view model.</returns>
    public static BeHomeNativeBrowseViewModel CreateLoaded(
        BeHomeNativeBrowseModel model,
        BeHomeCatalogPage page,
        string selectedTitleId)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        if (page == null)
        {
            throw new ArgumentNullException(nameof(page));
        }

        var selectedTitle = page.Titles.FirstOrDefault(title => string.Equals(title.Id, selectedTitleId, StringComparison.Ordinal))
            ?? page.Titles.FirstOrDefault();
        var cards = new List<BeHomeNativeBrowseCardViewModel>(page.Titles.Count);
        for (var index = 0; index < page.Titles.Count; index++)
        {
            var title = page.Titles[index];
            cards.Add(new BeHomeNativeBrowseCardViewModel(
                title.Id,
                title.DisplayName,
                title.StudioSlug,
                title.ContentKind,
                title.GenreDisplay,
                title.PlayerCountDisplay,
                title.AgeDisplay,
                title.ShortDescription,
                selectedTitle != null && string.Equals(title.Id, selectedTitle.Id, StringComparison.Ordinal)));
        }

        bool hasTitles = page.Titles.Count > 0;
        return new BeHomeNativeBrowseViewModel
        {
            HeadingText = model.HeadingText,
            SubtitleText = model.SubtitleText,
            StatusText = hasTitles
                ? $"Loaded {page.Titles.Count} public titles ({page.TotalCount} total)."
                : "No public titles matched this request.",
            MessageText = $"Page {Math.Max(1, page.PageNumber)} of {Math.Max(1, page.TotalPages)} | Environment: {model.AppEnvironmentName}",
            EmptyTitleText = "No titles yet",
            EmptyBodyText = "The public catalog request succeeded, but it did not return any listed titles for this page.",
            SelectedTitleHeading = selectedTitle?.DisplayName ?? "Select a title",
            SelectedTitleStudio = selectedTitle != null
                ? $"Studio: {selectedTitle.StudioSlug}"
                : "Choose a title from the list to inspect the native spike state.",
            SelectedTitleMeta = selectedTitle != null
                ? BuildSelectedTitleMeta(selectedTitle)
                : "Public list response only",
            SelectedTitleDescription = selectedTitle?.ShortDescription
                ?? "Title-detail API work will follow after the list-first spike is stable on device.",
            Cards = cards,
            ShowEmptyState = !hasTitles,
        };
    }

    /// <summary>
    /// Creates the error-state view model for the native browse spike.
    /// </summary>
    /// <param name="model">The current native browse model.</param>
    /// <param name="errorMessage">The user-facing error message.</param>
    /// <returns>The error-state view model.</returns>
    public static BeHomeNativeBrowseViewModel CreateError(BeHomeNativeBrowseModel model, string errorMessage)
    {
        return new BeHomeNativeBrowseViewModel
        {
            HeadingText = model?.HeadingText ?? string.Empty,
            SubtitleText = model?.SubtitleText ?? string.Empty,
            StatusText = "Catalog load failed.",
            MessageText = errorMessage ?? "The native catalog spike could not reach the maintained BE API.",
            EmptyTitleText = "Catalog unavailable",
            EmptyBodyText = errorMessage ?? "The native catalog spike could not reach the maintained BE API.",
            SelectedTitleHeading = "Catalog unavailable",
            SelectedTitleStudio = "The native spike could not refresh the public title list.",
            SelectedTitleMeta = "Public list response unavailable",
            SelectedTitleDescription = "This spike intentionally starts with the public catalog list, so network or API failures surface here first.",
            ShowEmptyState = true,
        };
    }

    private static string BuildSelectedTitleMeta(BeHomeCatalogTitleSummary title)
    {
        string contentKind = !string.IsNullOrWhiteSpace(title.ContentKind) ? title.ContentKind : "unknown";
        string genreDisplay = !string.IsNullOrWhiteSpace(title.GenreDisplay) ? title.GenreDisplay : "genre pending";
        string playerCountDisplay = !string.IsNullOrWhiteSpace(title.PlayerCountDisplay) ? title.PlayerCountDisplay : "players pending";
        string ageDisplay = !string.IsNullOrWhiteSpace(title.AgeDisplay) ? title.AgeDisplay : "age guidance pending";
        return $"{contentKind} | {genreDisplay} | {playerCountDisplay} | {ageDisplay}";
    }
}
}
