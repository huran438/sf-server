using System;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Linq;
using MemoryPack;
using MemoryPack.Compression;

namespace SFServer.UI {
    public static class HttpClientMessagePackExtensions {
        // Set the default content type for MessagePack.
        private static readonly MediaTypeHeaderValue _contentType = new("application/x-memorypack");

        // Configure MessagePack options to use ContractlessStandardResolver.
        private static readonly MemoryPackSerializerOptions DefaultOptions = MemoryPackSerializerOptions.Default;

        public static HttpClient CreateApiClient(this ClaimsPrincipal user, IConfiguration config, Guid projectId = default) {
            var client = new HttpClient { BaseAddress = new Uri(config["API_BASE_URL"]) };

            var token = user.FindFirst("JwtToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var userId = user.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                client.DefaultRequestHeaders.Add("UserId", userId);
            }

            client.DefaultRequestHeaders.Add("ProjectId", projectId != Guid.Empty ? projectId.ToString() : Guid.Empty.ToString());

            return client;
        }

        /// <summary>
        /// Sends a POST request with a MessagePack-serialized body and returns an HttpResponseMessage.
        /// Use this method when you only care about the HTTP response status.
        /// </summary>
        public static async Task<HttpResponseMessage> PostMessagePackAsync<TRequest>(
            this HttpClient httpClient,
            string requestUri,
            TRequest request,
            MemoryPackSerializerOptions options = null,
            CancellationToken cancellationToken = default) {
            options ??= DefaultOptions;


            using var compressor = new BrotliCompressor();
            MemoryPackSerializer.Serialize(compressor, request, options);
            var requestData = compressor.ToArray();
            using var content = new ByteArrayContent(requestData);
            content.Headers.ContentType = _contentType;

            return await httpClient.PostAsync(requestUri, content, cancellationToken);
        }

        /// <summary>
        /// Sends a POST request with a MessagePack-serialized body and deserializes the MessagePack response.
        /// </summary>
        public static async Task<TResponse> PostAsMessagePackAsync<TRequest, TResponse>(
            this HttpClient httpClient,
            string requestUri,
            TRequest request,
            MemoryPackSerializerOptions options = null,
            CancellationToken cancellationToken = default) {
            options ??= DefaultOptions;

            using var compressor = new BrotliCompressor();
            MemoryPackSerializer.Serialize(compressor, request, options);
            var requestData = compressor.ToArray();
            using var content = new ByteArrayContent(requestData);
            content.Headers.ContentType = _contentType;


            using var response = await httpClient.PostAsync(requestUri, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new ApiRequestException(requestUri, response.StatusCode, body);
            }

            var stream = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            using var decompressor = new BrotliDecompressor();
            var decompressedBuffer = decompressor.Decompress(stream);
            return MemoryPackSerializer.Deserialize<TResponse>(decompressedBuffer, options) ?? throw new InvalidOperationException();
        }

        /// <summary>
        /// Sends a GET request and deserializes the MessagePack response.
        /// </summary>
        public static async Task<T> GetFromMessagePackAsync<T>(this HttpClient httpClient, string requestUri, MemoryPackSerializerOptions options = null, CancellationToken cancellationToken = default) {
            options ??= DefaultOptions;

            httpClient.DefaultRequestHeaders.Add("ProjectId", Guid.Empty.ToString());

            using var response = await httpClient.GetAsync(requestUri, cancellationToken);

            if (response.IsSuccessStatusCode == false)
            {
                string body = null;
                try
                {
                    body = await response.Content.ReadAsStringAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to read error body: {ex.Message}");
                }

                Console.WriteLine($"GET {requestUri} failed with {(int)response.StatusCode} {response.StatusCode}");
                if (!string.IsNullOrWhiteSpace(body))
                {
                    Console.WriteLine(body);
                }

                Console.WriteLine(new ApiRequestException(
                    $"Request to '{requestUri}' failed with status code {(int)response.StatusCode}",
                    response.StatusCode,
                    body).ToString());

                return default;
            }

            var stream = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            using var decompressor = new BrotliDecompressor();
            var decompressedBuffer = decompressor.Decompress(stream);
            return MemoryPackSerializer.Deserialize<T>(decompressedBuffer, options);
        }

        /// <summary>
        /// Sends a PUT request with a MessagePack-serialized body.
        /// </summary>
        public static async Task<HttpResponseMessage> PutAsMessagePackAsync<T>(
            this HttpClient httpClient,
            string requestUri,
            T value,
            MemoryPackSerializerOptions options = null,
            CancellationToken cancellationToken = default) {
            options ??= DefaultOptions;
            using var compressor = new BrotliCompressor();
            MemoryPackSerializer.Serialize(compressor, value, options);
            var requestData = compressor.ToArray();
            using var content = new ByteArrayContent(requestData);
            content.Headers.ContentType = _contentType;
            return await httpClient.PutAsync(requestUri, content, cancellationToken);
        }
    }
}