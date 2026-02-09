namespace BuildingBlocks.Abstractions.Exceptions;

/// <summary>
/// Domain/use-case hataları için taban. RFC7807 ProblemDetails ile eşleştirilebilir.
/// </summary>
public abstract class DomainException : Exception
{
    public string Code { get; }
    public int? HttpStatus { get; }

    protected DomainException(string code, string message, int? httpStatus = null) : base(message)
    {
        Code = code;
        HttpStatus = httpStatus;
    }
}
