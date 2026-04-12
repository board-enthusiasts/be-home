using System.Threading;
using System.Threading.Tasks;

using BoardEnthusiasts.BeHome.Api.Contracts;
using BoardEnthusiasts.BeHome.Api.Http;
using BoardEnthusiasts.BeHome.Api.Models;
using BoardEnthusiasts.BeHome.Api.Services;

using NUnit.Framework;

namespace BoardEnthusiasts.BeHome.Tests;

[TestFixture]
public sealed class BeHomePresenceServiceTests
{
    [Test]
    public async Task RegisterSessionAsync_PostsInitialPresenceRegistration()
    {
        var transport = new StubTransport(new BeHomePresenceResponseDto
        {
            accepted = true,
            session = new BeHomePresenceSessionDto
            {
                sessionId = "session-1",
                authState = "anonymous",
                lastSeenAt = "2026-04-10T18:30:00Z",
                heartbeatIntervalSeconds = 60,
                activeTtlSeconds = 180,
            },
        });
        var service = new BeHomePresenceService(transport);

        await service.RegisterSessionAsync(
            new BeHomePresenceSnapshot(
                "session-1",
                new BeHomeDeviceIdentity("board-123", BeHomeDeviceIdSource.InstallId),
                BeHomeAuthState.Anonymous,
                "1.2.3",
                "production"));

        Assert.That(transport.LastPresenceRequest, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(transport.LastRoute, Is.EqualTo("/internal/be-home/presence"));
            Assert.That(transport.LastPresenceRequest!.sessionId, Is.EqualTo("session-1"));
            Assert.That(transport.LastPresenceRequest.deviceId, Is.EqualTo("board-123"));
            Assert.That(transport.LastPresenceRequest.deviceIdSource, Is.EqualTo("install_id"));
            Assert.That(transport.LastPresenceRequest.authState, Is.EqualTo("anonymous"));
            Assert.That(transport.LastPresenceRequest.clientVersion, Is.EqualTo("1.2.3"));
            Assert.That(transport.LastPresenceRequest.appEnvironment, Is.EqualTo("production"));
        });
    }

    [Test]
    public async Task EndSessionAsync_PostsDisconnectRequest()
    {
        var transport = new StubTransport(new BeHomePresenceEndResponseDto
        {
            accepted = true,
            session = new BeHomeEndedSessionDto
            {
                sessionId = "session-1",
                endedAt = "2026-04-10T18:35:00Z",
            },
        });
        var service = new BeHomePresenceService(transport);

        await service.EndSessionAsync("session-1");

        Assert.That(transport.LastRoute, Is.EqualTo("/internal/be-home/presence/end"));
        Assert.That(transport.LastEndRequest, Is.Not.Null);
        Assert.That(transport.LastEndRequest!.sessionId, Is.EqualTo("session-1"));
    }

    private sealed class StubTransport : IBeHomeApiTransport
    {
        private readonly object _response;

        public StubTransport(object response)
        {
            _response = response;
        }

        public string? LastRoute { get; private set; }

        public BeHomePresenceRequestDto? LastPresenceRequest { get; private set; }

        public BeHomePresenceEndRequestDto? LastEndRequest { get; private set; }

        public Task<TResponse> GetAsync<TResponse>(string relativePath, CancellationToken cancellationToken = default)
        {
            throw new System.NotSupportedException();
        }

        public Task<TResponse> PostJsonAsync<TRequest, TResponse>(string relativePath, TRequest request, CancellationToken cancellationToken = default)
        {
            LastRoute = relativePath;
            if (request is BeHomePresenceRequestDto presenceRequest)
            {
                LastPresenceRequest = presenceRequest;
            }

            if (request is BeHomePresenceEndRequestDto endRequest)
            {
                LastEndRequest = endRequest;
            }

            return Task.FromResult((TResponse)_response);
        }
    }
}
