using System.Threading;
using System.Threading.Tasks;

using BoardEnthusiasts.BeHome.Api.Contracts;
using BoardEnthusiasts.BeHome.Api.Http;
using BoardEnthusiasts.BeHome.Api.Models;
using BoardEnthusiasts.BeHome.Api.Services;

using NUnit.Framework;

namespace BoardEnthusiasts.BeHome.Tests;

[TestFixture]
public sealed class BeHomeTitleAnalyticsServiceTests
{
    [Test]
    public async Task RecordTitleDetailViewAsync_PostsMaintainedTitleDetailViewPayload()
    {
        var transport = new StubTransport(new BeHomeTitleDetailViewResponseDto
        {
            accepted = true,
        });
        var service = new BeHomeTitleAnalyticsService(transport);

        await service.RecordTitleDetailViewAsync(
            new BeHomeTitleDetailViewRecord(
                "title-1",
                "blue-harbor-games",
                "lantern-drift",
                "/browse/blue-harbor-games/lantern-drift?embed=board"));

        Assert.That(transport.LastRoute, Is.EqualTo("/internal/be-home/title-detail-views"));
        Assert.That(transport.LastTitleDetailViewRequest, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(transport.LastTitleDetailViewRequest!.titleId, Is.EqualTo("title-1"));
            Assert.That(transport.LastTitleDetailViewRequest.studioSlug, Is.EqualTo("blue-harbor-games"));
            Assert.That(transport.LastTitleDetailViewRequest.titleSlug, Is.EqualTo("lantern-drift"));
            Assert.That(transport.LastTitleDetailViewRequest.route, Is.EqualTo("/browse/blue-harbor-games/lantern-drift?embed=board"));
            Assert.That(transport.LastTitleDetailViewRequest.surface, Is.EqualTo("title-detail"));
        });
    }

    private sealed class StubTransport : IBeHomeApiTransport
    {
        private readonly object _response;

        public StubTransport(object response)
        {
            _response = response;
        }

        public string LastRoute { get; private set; }

        public BeHomeTitleDetailViewRequestDto LastTitleDetailViewRequest { get; private set; }

        public Task<TResponse> GetAsync<TResponse>(string relativePath, CancellationToken cancellationToken = default)
        {
            throw new System.NotSupportedException();
        }

        public Task<TResponse> PostJsonAsync<TRequest, TResponse>(string relativePath, TRequest request, CancellationToken cancellationToken = default)
        {
            LastRoute = relativePath;
            if (request is BeHomeTitleDetailViewRequestDto titleDetailViewRequest)
            {
                LastTitleDetailViewRequest = titleDetailViewRequest;
            }

            return Task.FromResult((TResponse)_response);
        }
    }
}
