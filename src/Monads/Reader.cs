namespace Monads;

/// <summary>
///     The <see cref="Reader{T}"/> environment.
/// </summary>
/// <typeparam name="E">
///     The environment type that <see cref="Reader{T}"/> reads from.
/// </typeparam>
public abstract partial class Environment<E> : ICapture
{
    /// <summary>
    ///     A computation that reads values from an environment.
    /// </summary>
    /// <param name="run">A function that runs in the environment.</param>
    /// <typeparam name="T">The function output type.</typeparam>
    public partial class Reader<T>(Func<E, T> run)
    {
        /// <summary>
        ///     Runs the <see cref="Reader{T}"/> in the provided environment.
        /// </summary>
        /// <param name="environment">The environment to run the computation in.</param>
        /// <returns>The computation result.</returns>
        public T Run(E environment) =>
            run(environment);

        /// <summary>
        ///     Modifies the environment.
        /// </summary>
        /// <param name="map">The function that modifies the environment.</param>
        /// <returns>A <see cref="Reader{T}"/> that reads from the modified environment.</returns>
        public Reader<T> With(Func<E, E> map) =>
            new(x => Run(map(x)));

        /// <inheritdoc cref="With(Func{E, E})"/>
        /// <typeparam name="F">The type of the modified environment.</typeparam>
        public Environment<F>.Reader<T> With<F>(Func<F, E> map) =>
            new(x => Run(map(x)));

        /// <inheritdoc/>
        public override string ToString() =>
            $"Reader {typeof(E).Name}";
    }

    // Pretending that C# supports type class instances. T_T
    public partial class Reader<T> : IMonad<T>
    {
        /// <inheritdoc cref="IMonad{T}.Docs.Bind"/>
        public Reader<U> Bind<U>(Func<T, Reader<U>> map) =>
            new(x => map(Run(x)).Run(x));

        /// <inheritdoc cref="IMonad{T}.Docs.Return"/>
        public static Reader<T> Return(T value) =>
            new(_ => value);
    }

    /// <summary>
    ///     A static facade for creating <see cref="Reader{T}"/> instances.
    /// </summary>
    public static class Reader
    {
        /// <summary>
        ///     Constructs a <see cref="Reader{T}"/> that retrieves the environment.
        /// </summary>
        /// <returns>The constructed <see cref="Reader{T}"/>.</returns>
        public static Reader<E> Ask() =>
            new(x => x);

        /// <summary>
        ///     Constructs a <see cref="Reader{T}"/> that applies the provided
        ///     function to the environment.
        /// </summary>
        /// <param name="run">The function to apply to the environment.</param>
        /// <typeparam name="T">The function output type.</typeparam>
        /// <returns>The constructed <see cref="Reader{T}"/>.</returns>
        public static Reader<T> Ask<T>(Func<E, T> run) =>
            new(run);
    }
}
