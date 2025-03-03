namespace Monads;

/// <summary>
///     The identity monad.
/// </summary>
/// <param name="value">The plain value.</param>
/// <typeparam name="T">The plain value type.</typeparam>
public readonly partial record struct Identity<T>(T value)
{
    /// <inheritdoc/>
    public override string ToString() =>
        "Identity " + value;

    /// <summary>
    ///     Wraps the specified value into an <see cref="Identity{T}"/>.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>The constructed <see cref="Identity{T}"/>.</returns>
    public static implicit operator Identity<T>(T value) =>
        new(value);

    /// <summary>
    ///     Unwraps the value from the <see cref="Identity{T}"/>.
    /// </summary>
    /// <param name="id">The <see cref="Identity{T}"/> to unwrap.</param>
    /// <returns>The unwrapped value.</returns>
    public static implicit operator T(Identity<T> id) =>
        id.value;
}

// Pretending that C# supports type class instances. T_T
public readonly partial record struct Identity<T> : IMonad<T>
{
    /// <inheritdoc cref="IMonad{T}.Docs.Bind"/>
    public Identity<U> Bind<U>(Func<T, Identity<U>> map) =>
        map(value);

    /// <inheritdoc cref="IMonad{T}.Docs.Return"/>
    public static Identity<T> Return(T value) =>
        new(value);
}
