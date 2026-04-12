using System;
using System.Collections.Generic;

namespace BoardEnthusiasts.BeHome.Api.Models
{
/// <summary>
/// Represents a single public BE catalog title summary for native BE Home browse experiences.
/// </summary>
public sealed class BeHomeCatalogTitleSummary
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomeCatalogTitleSummary"/> class.
    /// </summary>
    /// <param name="id">The stable title identifier.</param>
    /// <param name="studioId">The stable studio identifier.</param>
    /// <param name="studioSlug">The public studio slug.</param>
    /// <param name="slug">The public title slug.</param>
    /// <param name="contentKind">The public content kind.</param>
    /// <param name="displayName">The public title display name.</param>
    /// <param name="shortDescription">The short public title summary.</param>
    /// <param name="genreDisplay">The public genre display string.</param>
    /// <param name="playerCountDisplay">The public player-count display string.</param>
    /// <param name="ageDisplay">The public age-display string.</param>
    /// <param name="cardImageUrl">The optional card image URL.</param>
    /// <param name="acquisitionUrl">The optional acquisition URL.</param>
    public BeHomeCatalogTitleSummary(
        string id,
        string studioId,
        string studioSlug,
        string slug,
        string contentKind,
        string displayName,
        string shortDescription,
        string genreDisplay,
        string playerCountDisplay,
        string ageDisplay,
        string cardImageUrl,
        string acquisitionUrl)
    {
        Id = id ?? string.Empty;
        StudioId = studioId ?? string.Empty;
        StudioSlug = studioSlug ?? string.Empty;
        Slug = slug ?? string.Empty;
        ContentKind = contentKind ?? string.Empty;
        DisplayName = displayName ?? string.Empty;
        ShortDescription = shortDescription ?? string.Empty;
        GenreDisplay = genreDisplay ?? string.Empty;
        PlayerCountDisplay = playerCountDisplay ?? string.Empty;
        AgeDisplay = ageDisplay ?? string.Empty;
        CardImageUrl = cardImageUrl ?? string.Empty;
        AcquisitionUrl = acquisitionUrl ?? string.Empty;
    }

    /// <summary>
    /// Gets the stable title identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the stable studio identifier.
    /// </summary>
    public string StudioId { get; }

    /// <summary>
    /// Gets the public studio slug.
    /// </summary>
    public string StudioSlug { get; }

    /// <summary>
    /// Gets the public title slug.
    /// </summary>
    public string Slug { get; }

    /// <summary>
    /// Gets the public content kind.
    /// </summary>
    public string ContentKind { get; }

    /// <summary>
    /// Gets the public title display name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the short public title summary.
    /// </summary>
    public string ShortDescription { get; }

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
    /// Gets the optional card image URL.
    /// </summary>
    public string CardImageUrl { get; }

    /// <summary>
    /// Gets the optional acquisition URL.
    /// </summary>
    public string AcquisitionUrl { get; }
}

/// <summary>
/// Represents a single page of public BE catalog browse results.
/// </summary>
public sealed class BeHomeCatalogPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomeCatalogPage"/> class.
    /// </summary>
    /// <param name="titles">The current page of public titles.</param>
    /// <param name="pageNumber">The returned 1-based page number.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <param name="totalCount">The total number of matching public titles.</param>
    /// <param name="totalPages">The total number of pages available.</param>
    public BeHomeCatalogPage(
        IReadOnlyList<BeHomeCatalogTitleSummary> titles,
        int pageNumber,
        int pageSize,
        int totalCount,
        int totalPages)
    {
        Titles = titles ?? Array.Empty<BeHomeCatalogTitleSummary>();
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = totalPages;
    }

    /// <summary>
    /// Gets the current page of public titles.
    /// </summary>
    public IReadOnlyList<BeHomeCatalogTitleSummary> Titles { get; }

    /// <summary>
    /// Gets the returned 1-based page number.
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Gets the requested page size.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets the total number of matching public titles.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Gets the total number of pages available.
    /// </summary>
    public int TotalPages { get; }
}
}
