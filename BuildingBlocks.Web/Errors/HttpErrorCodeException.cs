namespace BuildingBlocks.Web.Errors;

public sealed class HttpErrorCodeException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }

    public HttpErrorCodeException(int statusCode, string errorCode, string message, Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
