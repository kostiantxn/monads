namespace Monads;

/// <summary>
///     A lazy computation.
/// </summary>
/// <param name="run">The computation function.</param>
/// <typeparam name="T">The function output type.</typeparam>
public partial class Lazy<T>(Func<T> run)
{
    private readonly System.Lazy<T> _lazy = new(run);

    /// <summary>
    ///     Evaluates the lazy computation.
    /// </summary>
    /// <returns>The computed value.</returns>
    public T Run() =>
        _lazy.Value;
}

// Pretending that C# supports type class instances. T_T
public partial class Lazy<T> : IMonad<T>
{
    /// <inheritdoc cref="IMonad{T}.Docs.Bind"/>
    public Lazy<U> Bind<U>(Func<T, Lazy<U>> map) =>
        new(() => map(Run()).Run());

    /// <inheritdoc cref="IMonad{T}.Docs.Return"/>
    public static Lazy<T> Return(T value) =>
        new(() => value);
}
