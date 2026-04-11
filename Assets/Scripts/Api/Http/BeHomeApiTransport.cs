using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

namespace BoardEnthusiasts.BeHome.Api.Http
{
/// <summary>
/// Defines JSON serialization for the BE Home API client.
/// </summary>
public interface IBeHomeJsonSerializer
{
    /// <summary>
    /// Serializes a BE Home payload to JSON.
    /// </summary>
    /// <typeparam name="TValue">The payload type to serialize.</typeparam>
    /// <param name="value">The payload to serialize.</param>
    /// <returns>The serialized JSON payload.</returns>
    string Serialize<TValue>(TValue value);

    /// <summary>
    /// Deserializes a BE Home payload from JSON.
    /// </summary>
    /// <typeparam name="TValue">The payload type to deserialize.</typeparam>
    /// <param name="json">The JSON payload to parse.</param>
    /// <returns>The deserialized payload.</returns>
    TValue Deserialize<TValue>(string json);
}

/// <summary>
/// Defines the transport surface for the BE Home API client.
/// </summary>
public interface IBeHomeApiTransport
{
    /// <summary>
    /// Sends a JSON <c>GET</c> request to the BE API.
    /// </summary>
    /// <typeparam name="TResponse">The response payload type.</typeparam>
    /// <param name="relativePath">The path relative to the configured API base URL.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The deserialized response payload.</returns>
    Task<TResponse> GetAsync<TResponse>(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a JSON <c>POST</c> request to the BE API.
    /// </summary>
    /// <typeparam name="TRequest">The request payload type.</typeparam>
    /// <typeparam name="TResponse">The response payload type.</typeparam>
    /// <param name="relativePath">The path relative to the configured API base URL.</param>
    /// <param name="request">The request payload to serialize.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    /// <returns>The deserialized response payload.</returns>
    Task<TResponse> PostJsonAsync<TRequest, TResponse>(string relativePath, TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a BE API transport failure.
/// </summary>
public sealed class BeHomeApiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomeApiException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="statusCode">The optional HTTP status code returned by the API.</param>
    /// <param name="responseBody">The optional response body returned by the API.</param>
    public BeHomeApiException(string message, int? statusCode = null, string responseBody = null)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    /// <summary>
    /// Gets the optional HTTP status code returned by the API.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Gets the optional response body returned by the API.
    /// </summary>
    public string ResponseBody { get; }
}

/// <summary>
/// Serializes BE Home API payloads with Unity's JSON utility.
/// </summary>
public sealed class UnityBeHomeJsonSerializer : IBeHomeJsonSerializer
{
    /// <inheritdoc/>
    public string Serialize<TValue>(TValue value)
    {
        return JsonUtility.ToJson(value);
    }

    /// <inheritdoc/>
    public TValue Deserialize<TValue>(string json)
    {
        var value = JsonUtility.FromJson<TValue>(json);
        if (value == null)
        {
            throw new BeHomeApiException("The BE API returned an empty JSON payload.");
        }

        return value;
    }
}

/// <summary>
/// Sends HTTP requests to the maintained BE API for BE Home.
/// </summary>
public sealed class BeHomeApiTransport : IBeHomeApiTransport, IDisposable
{
    private readonly Uri _baseUri;
    private readonly HttpClient _httpClient;
    private readonly IBeHomeJsonSerializer _jsonSerializer;
    private readonly bool _disposeHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomeApiTransport"/> class.
    /// </summary>
    /// <param name="apiBaseUrl">The maintained BE API base URL.</param>
    /// <param name="jsonSerializer">The JSON serializer used for request and response payloads.</param>
    /// <param name="timeout">The request timeout to apply to transport calls.</param>
    /// <param name="httpClient">The optional HTTP client to reuse for transport calls.</param>
    public BeHomeApiTransport(string apiBaseUrl, IBeHomeJsonSerializer jsonSerializer, TimeSpan timeout, HttpClient httpClient = null)
    {
        if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out _baseUri))
        {
            throw new ArgumentException("A valid absolute BE API base URL is required.", nameof(apiBaseUrl));
        }

        _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.Timeout = timeout;
        _disposeHttpClient = httpClient == null;
    }

    /// <inheritdoc/>
    public async Task<TResponse> GetAsync<TResponse>(string relativePath, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, relativePath);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return await ReadResponseAsync<TResponse>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<TResponse> PostJsonAsync<TRequest, TResponse>(string relativePath, TRequest request, CancellationToken cancellationToken = default)
    {
        using var httpRequest = CreateRequest(HttpMethod.Post, relativePath);
        httpRequest.Content = new StringContent(_jsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        return await ReadResponseAsync<TResponse>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("A relative API path is required.", nameof(relativePath));
        }

        var request = new HttpRequestMessage(method, new Uri(_baseUri, relativePath.TrimStart('/')));
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    private async Task<TResponse> ReadResponseAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string responseBody = response.Content != null
            ? await response.Content.ReadAsStringAsync().ConfigureAwait(false)
            : string.Empty;

        if (!response.IsSuccessStatusCode)
        {
            throw new BeHomeApiException(
                $"The BE API returned {(int)response.StatusCode} {response.ReasonPhrase}.",
                (int)response.StatusCode,
                responseBody);
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            throw new BeHomeApiException("The BE API returned an empty response body.", (int)response.StatusCode);
        }

        return _jsonSerializer.Deserialize<TResponse>(responseBody);
    }
}
}
