using System.Diagnostics.CodeAnalysis;

namespace Library.Lending;

public sealed record Error(string Code, string Message);

public readonly record struct Result<T>
{
    private static readonly Error NotSet =
        new("result.not_set", "The result was never set.");

    private readonly Error? _error;

    private Result(bool isSuccess, T? value, Error? error)
    {
        (IsSuccess, Value, _error) = (isSuccess, value, error);
    }

    public T? Value { get; }

    public Error? Error => IsSuccess ? null : _error ?? NotSet;

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(Error error) => new(false, default, error);

    public static Result<T> Failure(string code, string message) =>
        Failure(new Error(code, message));
}
