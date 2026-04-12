using System;
using System.Collections.Generic;

namespace BoardEnthusiasts.BeHome.UI.NativeBrowse
{
/// <summary>
/// Represents a single catalog card summary rendered by the native BE Home browse spike.
/// </summary>
internal sealed class BeHomeNativeBrowseCardViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomeNativeBrowseCardViewModel"/> class.
    /// </summary>
    /// <param name="titleId">The stable title identifier.</param>
    /// <param name="displayName">The public display name.</param>
    /// <param name="studioSlug">The public studio slug.</param>
    /// <param name="contentKind">The public content kind.</param>
    /// <param name="genreDisplay">The public genre display string.</param>
    /// <param name="playerCountDisplay">The public player-count display string.</param>
    /// <param name="ageDisplay">The public age-display string.</param>
    /// <param name="description">The short public summary.</param>
    /// <param name="isSelected">Indicates whether the card is currently selected.</param>
    public BeHomeNativeBrowseCardViewModel(
        string titleId,
        string displayName,
        string studioSlug,
        string contentKind,
        string genreDisplay,
        string playerCountDisplay,
        string ageDisplay,
        string description,
        bool isSelected)
    {
        TitleId = titleId ?? string.Empty;
        DisplayName = displayName ?? string.Empty;
        StudioSlug = studioSlug ?? string.Empty;
        ContentKind = contentKind ?? string.Empty;
        GenreDisplay = genreDisplay ?? string.Empty;
        PlayerCountDisplay = playerCountDisplay ?? string.Empty;
        AgeDisplay = ageDisplay ?? string.Empty;
        Description = description ?? string.Empty;
        IsSelected = isSelected;
    }

    /// <summary>
    /// Gets the stable title identifier.
    /// </summary>
    public string TitleId { get; }

    /// <summary>
    /// Gets the public display name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the public studio slug.
    /// </summary>
    public string StudioSlug { get; }

    /// <summary>
    /// Gets the public content kind.
    /// </summary>
    public string ContentKind { get; }

    /// <summary>
    /// Gets the public genre display string.
    /// </summary>
    public string GenreDisplay { get; }

    /// <summary>
    /// Gets the public player-count display string.
    /// </summary>
    public string PlayerCountDisplay { get; }

    /// <summary>
    /// Gets the public age-display string.
    /// </summary>
    public string AgeDisplay { get; }

    /// <summary>
    /// Gets the short public summary.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets a value indicating whether the card is currently selected.
    /// </summary>
    public bool IsSelected { get; }
}

/// <summary>
/// Represents the view model consumed by the native BE Home browse spike view.
/// </summary>
internal sealed class BeHomeNativeBrowseViewModel
{
    /// <summary>
    /// Gets or sets the heading copy.
    /// </summary>
    public string HeadingText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subtitle copy.
    /// </summary>
    public string SubtitleText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status text.
    /// </summary>
    public string StatusText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current supporting status message.
    /// </summary>
    public string MessageText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the empty-state title text.
    /// </summary>
    public string EmptyTitleText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the empty-state body text.
    /// </summary>
    public string EmptyBodyText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected title heading.
    /// </summary>
    public string SelectedTitleHeading { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected title studio line.
    /// </summary>
    public string SelectedTitleStudio { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected title metadata line.
    /// </summary>
    public string SelectedTitleMeta { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected title description.
    /// </summary>
    public string SelectedTitleDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rendered catalog cards.
    /// </summary>
    public IReadOnlyList<BeHomeNativeBrowseCardViewModel> Cards { get; set; } = Array.Empty<BeHomeNativeBrowseCardViewModel>();

    /// <summary>
    /// Gets or sets a value indicating whether the view should show a loading state.
    /// </summary>
    public bool IsLoading { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the view should show the empty-state panel.
    /// </summary>
    public bool ShowEmptyState { get; set; }
}
}
