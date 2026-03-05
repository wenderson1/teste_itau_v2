namespace Application.Common;

public class Result<T>
{
    public T? Value { get; }
    public Error? Error { get; }
    public bool IsSuccess => Error is null;
    public bool IsFailure => !IsSuccess;

    private Result(T value)
    {
        Value = value;
        Error = null;
    }

    private Result(Error error)
    {
        Value = default;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error!);
}

public record Error(string Mensagem, string Codigo, ErrorType Tipo = ErrorType.Validation)
{
    public static Error Validation(string mensagem, string codigo) => new(mensagem, codigo, ErrorType.Validation);
    public static Error NotFound(string mensagem, string codigo) => new(mensagem, codigo, ErrorType.NotFound);
}

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Internal
}
