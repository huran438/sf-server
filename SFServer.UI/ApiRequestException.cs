using System.Net;

namespace SFServer.UI;

public class ApiRequestException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? ResponseBody { get; }

    public ApiRequestException(string requestUri, HttpStatusCode statusCode, string? responseBody)
        : base($"Request to '{requestUri}' failed with status code {statusCode}")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
