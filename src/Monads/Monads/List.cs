using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using Monads.Core;

namespace Monads;

public partial class List<T>(IEnumerable<T> items) : IMonad<T>, IEnumerable<T>
{
    private readonly System.Collections.Generic.List<T> _items = items.ToList();

    public IEnumerator<T> GetEnumerator() =>
        _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public List<TMap> Bind<TMap>(Func<T, List<TMap>> map) =>
        new(_items.SelectMany(map));

    public static List<T> Return(T value) =>
        new([value]);

    public override string ToString() =>
           "[" + string.Join(", ", _items) + "]";
}

// TODO: Auto-generate.
[AsyncMethodBuilder(typeof(ListMonadMethodBuilder<>))]
partial class List<T>
{
    public MonadAwaiter<T> GetAwaiter() =>
        new(this);

    IMonad<TMap> IMonad<T>.Bind<TMap>(Func<T, IMonad<TMap>> map) =>
        Bind(x => (List<TMap>) map(x));
}

// TODO: Auto-generate.
public class ListMonadMethodBuilder<T>
{
    public static ListMonadMethodBuilder<T> Create() =>
        new();

    public void SetResult(T result) =>
        Task = List<T>.Return(result);

    public List<T> Task { get; protected set; } = default!;

    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine =>
        stateMachine.MoveNext();

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : MonadAwaiter, INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        var capturedAwaiter = awaiter;
        var capturedStateMachine = stateMachine;

        Task = (List<T>) awaiter.Container.Bind(x =>
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
