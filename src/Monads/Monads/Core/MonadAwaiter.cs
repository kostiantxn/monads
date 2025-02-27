using System.Runtime.CompilerServices;

namespace Monads.Core;

public class MonadAwaiter(IMonad container) : INotifyCompletion
{
    private bool _set;
    private object? _value;

    public object? Result
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

    public IMonad Container { get; } = container;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnCompleted(Action continuation) =>
        throw new NotSupportedException();
}

public class MonadAwaiter<T>(IMonad<T> container) : MonadAwaiter(container)
{
    public new IMonad<T> Container { get; } = container;

    public new T? Result
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (T?) base.Result;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => base.Result = value;
    }

    public bool IsCompleted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? GetResult() =>
        Result;
}
