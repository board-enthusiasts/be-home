using System;
using System.Threading;
using System.Threading.Tasks;

using BoardEnthusiasts.BeHome.Api.Contracts;
using BoardEnthusiasts.BeHome.Api.Http;
using BoardEnthusiasts.BeHome.Api.Services;

using NUnit.Framework;

namespace BoardEnthusiasts.BeHome.Tests;

[TestFixture]
public sealed class BeHomeCatalogServiceTests
{
    [Test]
    public async Task ListTitlesAsync_MapsPublicCatalogPage()
    {
        var transport = new StubTransport(new BeHomeCatalogTitleListResponseDto
        {
            titles =
            [
                new BeHomeCatalogTitleSummaryDto
                {
                    id = "title-1",
                    studioId = "studio-1",
                    studioSlug = "pine-lantern-labs",
                    slug = "signal-signal",
                    contentKind = "game",
                    displayName = "Signal Signal",
                    shortDescription = "A co-op timing challenge.",
                    genreDisplay = "Cooperative Puzzle",
                    playerCountDisplay = "1-4 players",
                    ageDisplay = "ESRB E10+",
                    cardImageUrl = "https://assets.example/signal-signal/card.png",
                    acquisitionUrl = "https://store.example/signal-signal",
                },
            ],
            paging = new BeHomeCatalogPagingDto
            {
                pageNumber = 2,
                pageSize = 24,
                totalCount = 61,
                totalPages = 3,
            },
        });
        var service = new BeHomeCatalogService(transport);

        var page = await service.ListTitlesAsync(2, 24);

        Assert.Multiple(() =>
        {
            Assert.That(transport.LastRequestedPath, Is.EqualTo("/catalog?pageNumber=2&pageSize=24"));
            Assert.That(page.PageNumber, Is.EqualTo(2));
            Assert.That(page.PageSize, Is.EqualTo(24));
            Assert.That(page.TotalCount, Is.EqualTo(61));
            Assert.That(page.TotalPages, Is.EqualTo(3));
            Assert.That(page.Titles, Has.Count.EqualTo(1));
            Assert.That(page.Titles[0].DisplayName, Is.EqualTo("Signal Signal"));
            Assert.That(page.Titles[0].StudioSlug, Is.EqualTo("pine-lantern-labs"));
        });
    }

    [Test]
    public void ListTitlesAsync_WithMissingPayload_Throws()
    {
        var transport = new StubTransport(new BeHomeCatalogTitleListResponseDto
        {
            titles = null!,
            paging = null!,
        });
        var service = new BeHomeCatalogService(transport);

        Assert.That(
            async () => await service.ListTitlesAsync(),
            Throws.InstanceOf<BeHomeApiException>());
    }

    [Test]
    public void ListTitlesAsync_WithInvalidPageNumber_Throws()
    {
        var service = new BeHomeCatalogService(new StubTransport(new BeHomeCatalogTitleListResponseDto()));

        Assert.That(
            async () => await service.ListTitlesAsync(0, 48),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    private sealed class StubTransport : IBeHomeApiTransport
    {
        private readonly BeHomeCatalogTitleListResponseDto _response;

        public StubTransport(BeHomeCatalogTitleListResponseDto response)
        {
            _response = response;
        }

        public string LastRequestedPath { get; private set; }

        public Task<TResponse> GetAsync<TResponse>(string relativePath, CancellationToken cancellationToken = default)
        {
            LastRequestedPath = relativePath;
            return Task.FromResult((TResponse)(object)_response);
        }

        public Task<TResponse> PostJsonAsync<TRequest, TResponse>(string relativePath, TRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
