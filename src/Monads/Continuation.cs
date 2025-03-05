namespace Monads;

/// <summary>
///     The continuation return value.
/// </summary>
/// <typeparam name="S">The continuation return type.</typeparam>
public partial class Returns<S> : ICapture
{
    /// <summary>
    ///     A computation in a continuation-passing style.
    /// </summary>
    /// <param name="run">
    ///     The computation that takes a continuation function as a parameter.
    /// </param>
    /// <typeparam name="T">The computation output type.</typeparam>
    public partial class Continuation<T>(Func<Func<T, S>, S> run)
    {
        /// <summary>
        ///     Runs the computation.
        /// </summary>
        /// <param name="continuation">
        ///     The continuation function to pass to the computation.
        /// </param>
        /// <returns>The computed value.</returns>
        public S Run(Func<T, S> continuation) =>
            run(continuation);
    }

    // Pretending that C# supports type class instances. T_T
    public partial class Continuation<T> : IMonad<T>
    {
        /// <inheritdoc cref="IMonad{T}.Docs.Bind"/>
        public Continuation<U> Bind<U>(Func<T, Continuation<U>> map) =>
            new(f => Run(g => map(g).Run(f)));

        /// <inheritdoc cref="IMonad{T}.Docs.Return"/>
        public static Continuation<T> Return(T value) =>
            new(f => f(value));
    }
}

/// <summary>
///     Extensions for <see cref="Returns{S}.Continuation{T}"/>.
/// </summary>
public static class Continuation
{
    /// <summary>
    ///     Runs the computations and passes the <c>id</c> function
    ///     (<c>x => x</c>) as a continuation.
    /// </summary>
    /// <param name="continuation">The computation to run.</param>
    /// <typeparam name="T">The computation output type.</typeparam>
    /// <returns>The computed value.</returns>
    public static T Run<T>(this Returns<T>.Continuation<T> continuation) =>
        continuation.Run(x => x);
}
