namespace SFServer.UI;

public class ApiRequestException : HttpRequestException
{
    public System.Net.HttpStatusCode StatusCode { get; }
    public string? ResponseBody { get; }

    public ApiRequestException(string message, System.Net.HttpStatusCode statusCode, string? responseBody)
        : base(message, null, statusCode)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
