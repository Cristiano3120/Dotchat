using System.Diagnostics.CodeAnalysis;

namespace DotchatServer.src.Infrastructure;

/// <summary>
/// A simple struct to represent the result of an operation, which can either be a success with a value of type T, or a failure with an exception.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly record struct Result<TValueType, TErrorType>
{
    [MemberNotNullWhen(false, nameof(IsOperationSuccess))]
    public TErrorType? Error { get; }

    [MemberNotNullWhen(true, nameof(IsOperationSuccess))]
    public TValueType? Value { get; }

    /// <summary>
    /// Indicates whether the result represents a successful operation (true) or a failure (false). 
    /// If true, the Value property will contain the result of the operation; 
    /// if false, the Error property will contain the exception that occurred.
    /// </summary>
    public bool IsOperationSuccess { get; }

    private Result(TValueType? value) { Value = value; IsOperationSuccess = true; }
    private Result(TErrorType error) { Error = error; IsOperationSuccess = false; }

    /// <summary>
    /// Makes it possible to return just "true" for example instead of Result<bool, Exception>.Success(true);
    /// </summary>
    public static implicit operator Result<TValueType, TErrorType>(TValueType value) => Success(value);

    /// <summary>
    /// Makes it possible to return just "ex" for example instead of Result<bool, Exception>.Failure(ex);
    /// </summary>
    public static implicit operator Result<TValueType, TErrorType>(TErrorType value) => Failure(value);

    public static Result<TValueType, TErrorType> Success(TValueType? value) => new(value);
    public static Result<TValueType, TErrorType> Failure(TErrorType error) => new(error);
}