using System.Threading;
using System.Threading.Tasks;

namespace BoardEnthusiasts.BeHome.Api.Http;

/// <summary>
/// Minimal HTTP abstractions for exercising the BE Home API services in edit-mode tests.
/// </summary>
public interface IBeHomeApiTransport
{
    Task<TResponse> GetAsync<TResponse>(string relativePath, CancellationToken cancellationToken = default);

    Task<TResponse> PostJsonAsync<TRequest, TResponse>(string relativePath, TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a BE Home API client failure in edit-mode tests.
/// </summary>
public sealed class BeHomeApiException : System.Exception
{
    public BeHomeApiException(string message)
        : base(message)
    {
    }
}
