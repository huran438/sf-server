using System.Net.Http.Headers;
using MessagePack;
using MessagePack.Resolvers;

namespace SFServer.UI
{
    public static class HttpClientMessagePackExtensions
    {
        // Set the default content type for MessagePack.
        private static readonly MediaTypeHeaderValue _messagePackContentType =
            new MediaTypeHeaderValue("application/x-msgpack");

        // Configure MessagePack options to use ContractlessStandardResolver.
        private static readonly MessagePackSerializerOptions DefaultOptions =
            MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance);

        /// <summary>
        /// Sends a POST request with a MessagePack-serialized body and returns an HttpResponseMessage.
        /// Use this method when you only care about the HTTP response status.
        /// </summary>
        public static async Task<HttpResponseMessage> PostMessagePackAsync<TRequest>(
            this HttpClient httpClient,
            string requestUri,
            TRequest request,
            MessagePackSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            options ??= DefaultOptions;
            var requestData = MessagePackSerializer.Serialize(request, options);
            using var content = new ByteArrayContent(requestData);
            content.Headers.ContentType = _messagePackContentType;

            return await httpClient.PostAsync(requestUri, content, cancellationToken);
        }

        /// <summary>
        /// Sends a POST request with a MessagePack-serialized body and deserializes the MessagePack response.
        /// </summary>
        public static async Task<TResponse> PostAsMessagePackAsync<TRequest, TResponse>(
            this HttpClient httpClient,
            string requestUri,
            TRequest request,
            MessagePackSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            options ??= DefaultOptions;
            var requestData = MessagePackSerializer.Serialize(request, options);
            using var content = new ByteArrayContent(requestData);
            content.Headers.ContentType = _messagePackContentType;
            

            using var response = await httpClient.PostAsync(requestUri, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await MessagePackSerializer.DeserializeAsync<TResponse>(responseStream, options, cancellationToken);
        }

        /// <summary>
        /// Sends a GET request and deserializes the MessagePack response.
        /// </summary>
        public static async Task<T> GetFromMessagePackAsync<T>(
            this HttpClient httpClient,
            string requestUri,
            MessagePackSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            options ??= DefaultOptions;
            using var response = await httpClient.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await MessagePackSerializer.DeserializeAsync<T>(stream, options, cancellationToken);
        }

        /// <summary>
        /// Sends a PUT request with a MessagePack-serialized body.
        /// </summary>
        public static async Task<HttpResponseMessage> PutAsMessagePackAsync<T>(
            this HttpClient httpClient,
            string requestUri,
            T value,
            MessagePackSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            options ??= DefaultOptions;
            var data = MessagePackSerializer.Serialize(value, options);
            using var content = new ByteArrayContent(data);
            content.Headers.ContentType = _messagePackContentType;

            return await httpClient.PutAsync(requestUri, content, cancellationToken);
        }
    }
}