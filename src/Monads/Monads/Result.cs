using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Monads.Core;

namespace Monads;

public readonly partial record struct Result<T> : IMonad<T>
{
    private readonly T? _value;
    private readonly Exception? _error;

    public Result(T value)
    {
        _value = value;
        _error = null;
    }

    public Result(Exception error)
    {
        _error = error;
        _value = default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSuccess() =>
        _error is null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSuccess([NotNullWhen(true)] out T? value)
    {
        if (IsSuccess())
        {
            value = _value!;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFailure() =>
        _error is not null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFailure([NotNullWhen(true)] out Exception? error)
    {
        if (IsFailure())
        {
            error = _error!;
            return true;
        }
        else
        {
            error = null;
            return false;
        }
    }

    public T OrThrow() =>
        _error is not null ? throw _error : _value!;

    public Result<TMap> Bind<TMap>(Func<T, Result<TMap>> map) =>
        _error is not null ? new(_error) : map(_value!);

    public override string ToString() =>
        _error is not null ? $"F[{_error}]" : $"S[{_value}]";

    public static Result<T> Return(T value) =>
        new(value);

    public static Result<T> Success(T value) =>
        new(value);

    public static Result<T> Failure(Exception error) =>
        new(error);
}

public struct Result
{
    public static Result<T> Try<T>(Func<T> action) 
    {
        try
        {
            return new(action());
        }
        catch (Exception error)
        {
            return new(error);
        }
    }
}

