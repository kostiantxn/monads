using System.Diagnostics.CodeAnalysis;

namespace Monads;

/// <summary>
///     The result of a computation.
/// </summary>
/// <typeparam name="T">The type of the computed value.</typeparam>
public readonly partial record struct Result<T>
{
    private readonly T? _value;
    private readonly Exception? _error;

    /// <summary>
    ///     Constructs a <see cref="Result{T}"/> that contains a computed value.
    /// </summary>
    /// <param name="value">The computed value.</param>
    public Result(T value)
    {
        _value = value;
        _error = null;
    }

    /// <summary>
    ///     Constructs a <see cref="Result{T}"/> that contains an error.
    /// </summary>
    /// <param name="error">The computation error.</param>
    public Result(Exception error)
    {
        _value = default;
        _error = error;
    }

    /// <inheritdoc cref="Result{T}(T)"/>
    /// <returns>The constructed <see cref="Result{T}"/>.</returns>
    public static Result<T> Ok(T value) =>
        new(value);

    /// <inheritdoc cref="Result{T}(Exception)"/>
    /// <returns>The constructed <see cref="Result{T}"/>.</returns>
    public static Result<T> Error(Exception error) =>
        new(error);

    /// <summary>
    ///     Runs the computation and returns an <see cref="Ok(T)"/> with
    ///     the computed value if the computation succeeds; otherwise, returns
    ///     an <see cref="Error(Exception)"/>.
    /// </summary>
    /// <param name="computation">The computation to run.</param>
    /// <returns>A <see cref="Result{T}"/> with the computation result.</returns>
    public static Result<T> Try(Func<T> computation)
    {
        try
        {
            return Ok(computation());
        }
        catch (Exception error)
        {
            return Error(error);
        }
    }

    /// <summary>
    ///     Returns <c>true</c> if the <see cref="Result{T}"/> contains
    ///     an <see cref="Ok(T)"/>; otherwise, returns <c>false</c>.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the <see cref="Result{T}"/> contains
    ///     an <see cref="Ok(T)"/>, and <c>false</c> otherwise.
    /// </returns>
    public bool IsOk() =>
        IsOk(out _, out _);

    /// <inheritdoc cref="IsOk()"/>
    /// <param name="value">The computed value.</param>
    public bool IsOk([NotNullWhen(true)] out T? value) =>
        IsOk(out value, out _);

    /// <inheritdoc cref="IsOk()"/>
    /// <param name="value">The computed value.</param>
    /// <param name="error">The computation error.</param>
    public bool IsOk([NotNullWhen(true)] out T? value, [NotNullWhen(false)] out Exception? error)
    {
        value = _value!;
        error = _error!;

        return _error is null;
    }

    /// <summary>
    ///     Returns <c>true</c> if the <see cref="Result{T}"/> contains
    ///     an <see cref="Error(Exception)"/>; otherwise, returns <c>false</c>.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the <see cref="Result{T}"/> contains
    ///     an <see cref="Error(Exception)"/>, and <c>false</c> otherwise.
    /// </returns>
    public bool IsError() =>
        IsError(out _);

    /// <inheritdoc cref="IsError()"/>
    /// <param name="error">The computation error.</param>
    public bool IsError([NotNullWhen(true)] out Exception? error)
    {
        error = _error;

        return _error is not null;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        _error is null ? $"Ok {_value}" : $"Error {_error}";
}

// Pretending that C# supports type class instances. T_T
public readonly partial record struct Result<T> : IMonad<T>
{
    /// <inheritdoc cref="IMonad{T}.Docs.Bind"/>
    public Result<U> Bind<U>(Func<T, Result<U>> map) =>
        IsOk(out var value, out var error)
            ? map(value)
            : Result<U>.Error(error);

    /// <inheritdoc cref="IMonad{T}.Docs.Return"/>
    public static Result<T> Return(T value) =>
        Ok(value);
}

/// <summary>
///     A static facade for creating <see cref="Result{T}"/> instances.
/// </summary>
public static class Result
{
    /// <inheritdoc cref="Result{T}.Ok(T)"/>
    public static Result<T> Ok<T>(T value) =>
        Result<T>.Ok(value);

    /// <inheritdoc cref="Result{T}.Error(Exception)"/>
    public static Result<T> Error<T>(Exception error) =>
        Result<T>.Error(error);

    /// <inheritdoc cref="Result{T}.Try(Func{T})"/>
    public static Result<T> Try<T>(Func<T> func) =>
        Result<T>.Try(func);
}
