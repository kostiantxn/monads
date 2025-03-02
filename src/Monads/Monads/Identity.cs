namespace Monads;

/// <summary>
///     The identity monad.
/// </summary>
/// <param name="value">The plain value.</param>
/// <typeparam name="T">The plain value type.</typeparam>
public partial class Identity<T>(T value) : IMonad<T>
{
    /// <inheritdoc cref="IMonad{T}.Docs.Bind"/>
    public Identity<U> Bind<U>(Func<T, Identity<U>> map) =>
        map(value);

    /// <inheritdoc cref="IMonad{T}.Docs.Return"/>
    public static Identity<T> Return(T value) =>
        new(value);
}
