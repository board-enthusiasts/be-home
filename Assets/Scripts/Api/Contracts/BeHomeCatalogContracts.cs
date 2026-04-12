using System;

namespace BoardEnthusiasts.BeHome.Api.Contracts
{
/// <summary>
/// Wire-format response payload for the public catalog title list.
/// </summary>
[Serializable]
public sealed class BeHomeCatalogTitleListResponseDto
{
    /// <summary>
    /// The current page of public catalog titles.
    /// </summary>
    public BeHomeCatalogTitleSummaryDto[] titles;

    /// <summary>
    /// The paging metadata for the current catalog request.
    /// </summary>
    public BeHomeCatalogPagingDto paging;
}

/// <summary>
/// Wire-format paging metadata returned from public catalog list requests.
/// </summary>
[Serializable]
public sealed class BeHomeCatalogPagingDto
{
    /// <summary>
    /// The 1-based page number that was returned.
    /// </summary>
    public int pageNumber;

    /// <summary>
    /// The requested page size.
    /// </summary>
    public int pageSize;

    /// <summary>
    /// The total number of matching public catalog titles.
    /// </summary>
    public int totalCount;

    /// <summary>
    /// The total number of pages available for the current query.
    /// </summary>
    public int totalPages;
}

/// <summary>
/// Wire-format summary payload returned for each public catalog title.
/// </summary>
[Serializable]
public sealed class BeHomeCatalogTitleSummaryDto
{
    /// <summary>
    /// The stable title identifier.
    /// </summary>
    public string id;

    /// <summary>
    /// The stable owning studio identifier.
    /// </summary>
    public string studioId;

    /// <summary>
    /// The public studio slug.
    /// </summary>
    public string studioSlug;

    /// <summary>
    /// The public title slug.
    /// </summary>
    public string slug;

    /// <summary>
    /// The public content kind.
    /// </summary>
    public string contentKind;

    /// <summary>
    /// The public lifecycle status.
    /// </summary>
    public string lifecycleStatus;

    /// <summary>
    /// The public visibility value.
    /// </summary>
    public string visibility;

    /// <summary>
    /// Indicates whether the title is currently reported.
    /// </summary>
    public bool isReported;

    /// <summary>
    /// The current metadata revision number.
    /// </summary>
    public int currentMetadataRevision;

    /// <summary>
    /// The public title display name.
    /// </summary>
    public string displayName;

    /// <summary>
    /// The short public summary for the title.
    /// </summary>
    public string shortDescription;

    /// <summary>
    /// The display string for the title genre.
    /// </summary>
    public string genreDisplay;

    /// <summary>
    /// The minimum supported player count.
    /// </summary>
    public int minPlayers;

    /// <summary>
    /// The maximum supported player count.
    /// </summary>
    public int maxPlayers;

    /// <summary>
    /// The public player-count display string.
    /// </summary>
    public string playerCountDisplay;

    /// <summary>
    /// The optional age-rating authority.
    /// </summary>
    public string ageRatingAuthority;

    /// <summary>
    /// The optional age-rating value.
    /// </summary>
    public string ageRatingValue;

    /// <summary>
    /// The minimum recommended player age.
    /// </summary>
    public int minAgeYears;

    /// <summary>
    /// The public age-display string.
    /// </summary>
    public string ageDisplay;

    /// <summary>
    /// The optional card image URL for the title.
    /// </summary>
    public string cardImageUrl;

    /// <summary>
    /// The optional acquisition URL for the title.
    /// </summary>
    public string acquisitionUrl;
}
}
