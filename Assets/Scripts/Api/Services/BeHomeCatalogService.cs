using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using BoardEnthusiasts.BeHome.Api.Contracts;
using BoardEnthusiasts.BeHome.Api.Http;
using BoardEnthusiasts.BeHome.Api.Models;

namespace BoardEnthusiasts.BeHome.Api.Services
{
/// <summary>
/// Defines public catalog operations consumed by native BE Home browse experiences.
/// </summary>
public interface IBeHomeCatalogService
{
    /// <summary>
    /// Fetches a single page of public catalog titles.
    /// </summary>
    /// <param name="pageNumber">The 1-based page number to fetch.</param>
    /// <param name="pageSize">The page size to request.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The current page of public catalog titles.</returns>
    Task<BeHomeCatalogPage> ListTitlesAsync(int pageNumber = 1, int pageSize = 48, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implements public catalog operations for native BE Home browse experiences.
/// </summary>
public sealed class BeHomeCatalogService : IBeHomeCatalogService
{
    private const string CatalogRoute = "/catalog";
    private readonly IBeHomeApiTransport _transport;

    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomeCatalogService"/> class.
    /// </summary>
    /// <param name="transport">The maintained BE Home API transport.</param>
    public BeHomeCatalogService(IBeHomeApiTransport transport)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    }

    /// <inheritdoc/>
    public async Task<BeHomeCatalogPage> ListTitlesAsync(int pageNumber = 1, int pageSize = 48, CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "The page number must be greater than zero.");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "The page size must be greater than zero.");
        }

        string route = string.Format(
            CultureInfo.InvariantCulture,
            "{0}?pageNumber={1}&pageSize={2}",
            CatalogRoute,
            pageNumber,
            pageSize);
        var response = await _transport
            .GetAsync<BeHomeCatalogTitleListResponseDto>(route, cancellationToken)
            .ConfigureAwait(false);

        if (response?.paging == null || response.titles == null)
        {
            throw new BeHomeApiException("The BE API returned an empty public catalog payload.");
        }

        var titles = new List<BeHomeCatalogTitleSummary>(response.titles.Length);
        for (var index = 0; index < response.titles.Length; index++)
        {
            var title = response.titles[index];
            if (title == null)
            {
                continue;
            }

            titles.Add(new BeHomeCatalogTitleSummary(
                title.id,
                title.studioId,
                title.studioSlug,
                title.slug,
                title.contentKind,
                title.displayName,
                title.shortDescription,
                title.genreDisplay,
                title.playerCountDisplay,
                title.ageDisplay,
                title.cardImageUrl,
                title.acquisitionUrl));
        }

        return new BeHomeCatalogPage(
            titles,
            response.paging.pageNumber,
            response.paging.pageSize,
            response.paging.totalCount,
            response.paging.totalPages);
    }
}
}
