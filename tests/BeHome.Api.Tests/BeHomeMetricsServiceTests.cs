using System;
using System.Threading;
using System.Threading.Tasks;

using BoardEnthusiasts.BeHome.Api.Contracts;
using BoardEnthusiasts.BeHome.Api.Http;
using BoardEnthusiasts.BeHome.Api.Services;

using NUnit.Framework;

namespace BoardEnthusiasts.BeHome.Tests;

[TestFixture]
public sealed class BeHomeMetricsServiceTests
{
    [Test]
    public async Task GetMetricsAsync_MapsAggregateDeviceTrendMetrics()
    {
        var transport = new StubTransport(new BeHomeMetricsResponseDto
        {
            metrics = new BeHomeMetricsDto
            {
                activeNowTotal = 12,
                activeNowAnonymous = 7,
                activeNowSignedIn = 5,
                totalBoardsSeen = 42,
                dailyActiveDevices = 16,
                weeklyActiveDevices = 28,
                monthlyActiveDevices = 37,
                updatedAt = "2026-04-10T18:30:00Z",
            },
        });
        var service = new BeHomeMetricsService(transport);

        var metrics = await service.GetMetricsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(metrics.ActiveNowTotal, Is.EqualTo(12));
            Assert.That(metrics.ActiveNowAnonymous, Is.EqualTo(7));
            Assert.That(metrics.ActiveNowSignedIn, Is.EqualTo(5));
            Assert.That(metrics.TotalBoardsSeen, Is.EqualTo(42));
            Assert.That(metrics.DailyActiveDevices, Is.EqualTo(16));
            Assert.That(metrics.WeeklyActiveDevices, Is.EqualTo(28));
            Assert.That(metrics.MonthlyActiveDevices, Is.EqualTo(37));
            Assert.That(metrics.UpdatedAt, Is.EqualTo(DateTimeOffset.Parse("2026-04-10T18:30:00Z")));
        });
    }

    [Test]
    public void GetMetricsAsync_WithInvalidTimestamp_Throws()
    {
        var transport = new StubTransport(new BeHomeMetricsResponseDto
        {
            metrics = new BeHomeMetricsDto
            {
                activeNowTotal = 1,
                activeNowAnonymous = 1,
                activeNowSignedIn = 0,
                totalBoardsSeen = 1,
                dailyActiveDevices = 1,
                weeklyActiveDevices = 1,
                monthlyActiveDevices = 1,
                updatedAt = "not-a-timestamp",
            },
        });
        var service = new BeHomeMetricsService(transport);

        Assert.That(
            async () => await service.GetMetricsAsync(),
            Throws.InstanceOf<BeHomeApiException>());
    }

    private sealed class StubTransport : IBeHomeApiTransport
    {
        private readonly BeHomeMetricsResponseDto _response;

        public StubTransport(BeHomeMetricsResponseDto response)
        {
            _response = response;
        }

        public Task<TResponse> GetAsync<TResponse>(string relativePath, CancellationToken cancellationToken = default)
        {
            Assert.That(relativePath, Is.EqualTo("/internal/be-home/metrics"));
            return Task.FromResult((TResponse)(object)_response);
        }

        public Task<TResponse> PostJsonAsync<TRequest, TResponse>(string relativePath, TRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
