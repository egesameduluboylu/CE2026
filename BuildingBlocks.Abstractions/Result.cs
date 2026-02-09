namespace BuildingBlocks.Abstractions;

/// <summary>
/// Use-case sonucu: başarı (Value) veya hata (Error). Handler'lar Result<T> döner.
/// </summary>
public readonly record struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ResultError? Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = null;
    }

    private Result(ResultError error)
    {
        IsSuccess = false;
        Value = default;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(value);

    public static Result<T> Fail(string code, string message, IDictionary<string, object?>? extensions = null)
        => new(new ResultError(code, message, extensions));

    public static Result<T> Fail(ResultError error) => new(error);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<ResultError, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error!.Value);
}

public readonly record struct ResultError(string Code, string Message, IDictionary<string, object?>? Extensions = null)
{
    public int? HttpStatus { get; init; }

    public static ResultError Conflict(string message)
        => new("CONFLICT", message) { HttpStatus = 409 };

    public static ResultError Unauthorized(string message)
        => new("UNAUTHORIZED", message) { HttpStatus = 401 };

    public static ResultError Locked(string message, DateTimeOffset? lockedUntil = null)
        => new(
            "LOCKED",
            message,
            lockedUntil is { } u
                ? new Dictionary<string, object?> { ["lockedUntil"] = u }
                : null
        )
        { HttpStatus = 423 };

    public static ResultError Validation(string message, IDictionary<string, string[]>? errors = null)
        => new(
            "VALIDATION",
            message,
            errors is { } e
                ? new Dictionary<string, object?>(e.ToDictionary(x => x.Key, x => (object?)x.Value))
                : null
        )
        { HttpStatus = 422 };
}

/// <summary>
/// Value olmayan (void) use-case sonucu.
/// </summary>
public readonly record struct Result
{
    public bool IsSuccess { get; }
    public ResultError? Error { get; }

    private Result(bool success, ResultError? error)
    {
        IsSuccess = success;
        Error = error;
    }

    public static Result Ok() => new(true, null);

    public static Result Fail(string code, string message, IDictionary<string, object?>? extensions = null)
        => new(false, new ResultError(code, message, extensions));

    public static Result Fail(ResultError error) => new(false, error);
}
