using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
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

    public static Result<T> Return(T value) =>
        new(value);

    public override string ToString() =>
        _error is not null ? $"F[{_error}]" : $"S[{_value}]";

    public static Result<T> Success(T value) =>
        new(value);

    public static Result<T> Failure(Exception error) =>
        new(error);

    IMonad<TMap> IMonad<T>.Bind<TMap>(Func<T, IMonad<TMap>> map) =>
        Bind(x => (Result<TMap>) map(x));
}

[AsyncMethodBuilder(typeof(ResultMonadMethodBuilder<>))]
readonly partial record struct Result<T>
{
    public MonadAwaiter<T> GetAwaiter() =>
        new(this);
}

// TODO: Auto-generate.
public class ResultMonadMethodBuilder<T>
{
    public static ResultMonadMethodBuilder<T> Create() =>
        new();

    public void SetResult(T result) =>
        Task = Result<T>.Return(result);

    public Result<T> Task { get; protected set; } = default!;

    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine =>
        stateMachine.MoveNext();

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : MonadAwaiter, INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        var capturedAwaiter = awaiter;
        var capturedStateMachine = stateMachine;

        Task = (Result<T>) awaiter.Container.Bind(x =>
        {
            // Create a shallow copy of the state machine and run it from
            // the current state.
            var clonedStateMachine = Clone<TStateMachine>.Func(capturedStateMachine);

            // Update the `MonadAwaiter` to return the value from the monad
            // container, so that when the state machine invokes `GetResult`,
            // the set value will be returned and used in the next state
            // machine step.
            capturedAwaiter.Result = x;

            // Run the cloned state machine.
            clonedStateMachine.MoveNext();

            // Return the final result that was computed by the cloned state
            // machine (since the state machine was only shallow copied, it
            // reuses the same `MonadMethodBuilder` instance, so its result
            // will be stored in the `this.Task` property).
            return Task;
        });
    }

    #region Unsupported

    public void SetException(Exception exception) =>
        throw new NotSupportedException("`SetException` is not supported", exception);

    [SecuritySafeCritical]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : MonadAwaiter, ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        throw new NotSupportedException("`AwaitUnsafeOnCompleted` is not supported");

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }

    #endregion

    private static class Clone<TStateMachine>
        where TStateMachine : IAsyncStateMachine
    {
        public static readonly Func<TStateMachine, TStateMachine> Func;

        static Clone()
        {
            // C# compiler generates a `struct` state machine for `Release`,
            // and a `class` for `Debug` build configuration.
            if (typeof(TStateMachine).IsValueType)
            {
                Func = x => x;
            }
            else
            {
                var method =
                    typeof(TStateMachine)
                        .GetMethod(nameof(MemberwiseClone), BindingFlags.Instance | BindingFlags.NonPublic) 
                    ?? throw new InvalidOperationException("Couldn't find the `MemberwiseClone` method");

                var @delegate =
                    (Func<object, object>) Delegate.CreateDelegate(typeof(Func<object, object>), method);

                Func = x => (TStateMachine) @delegate(x);
            }
        }
    }
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

