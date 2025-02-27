using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;

namespace Monads;

// TODO: Auto-generate.
[AsyncMethodBuilder(typeof(ResultMonadMethodBuilder<>))]
readonly partial record struct Result<T>
{
    public ResultMonadAwaiter<T> GetAwaiter() =>
        new(this);
}

// TODO: Auto-generate.
public abstract class ResultMonadAwaiter : INotifyCompletion
{
    private bool _set;
    private object _value;

    public object Result
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _set ? _value : throw new InvalidOperationException("Value has not been set");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            _value = value;
            _set = true;
        }
    }

    public bool IsCompleted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }

    public abstract Result<TMap> Bind<TMap>(Func<object, Result<TMap>> map);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnCompleted(Action continuation) =>
        throw new NotSupportedException("`OnCompleted` must not be called");
}

// TODO: Auto-generate.
public class ResultMonadAwaiter<T>(Result<T> container) : ResultMonadAwaiter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetResult() =>
        (T) Result;

    public override Result<TMap> Bind<TMap>(Func<object, Result<TMap>> map) =>
        container.Bind((T x) => map(x!));
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
        where TAwaiter : ResultMonadAwaiter, INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        var capturedAwaiter = awaiter;
        var capturedStateMachine = stateMachine;

        Task = awaiter.Bind(x =>
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
        where TAwaiter : ResultMonadAwaiter, ICriticalNotifyCompletion
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
