using System.Diagnostics.CodeAnalysis;

namespace DotchatServer.src.Infrastructure;

/// <summary>
/// A simple struct to represent the result of an operation, which can either be a success with a value of type T, or a failure with an exception.
/// </summary>
/// <typeparam name="T"></typeparam>
public record Result<T>
{
    public T? Value { get; }

    [MemberNotNullWhen(true, nameof(IsOperationSuccess))]
    public Exception? Error { get; }

    /// <summary>
    /// Indicates whether the result represents a successful operation (true) or a failure (false). 
    /// If true, the Value property will contain the result of the operation; 
    /// if false, the Error property will contain the exception that occurred.
    /// </summary>
    public bool IsOperationSuccess { get; }

    private Result(T value) { Value = value; IsOperationSuccess = true; }
    private Result(Exception error) { Error = error; IsOperationSuccess = false; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Exception error) => new(error);
}