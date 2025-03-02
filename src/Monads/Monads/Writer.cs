using System.Numerics;
using Unit = System.ValueTuple;

namespace Monads;

/// <summary>
///     The <see cref="Writer{T}"/> log.
/// </summary>
/// <typeparam name="W">
///     The type of log entries that <see cref="Writer{T}"/> maintains.
/// </typeparam>
/// <remarks>
///     The type <typeparamref name="W"/> must form a monoid.
/// </remarks>
public partial class Log<W> : ICapture
    where W : IMonoid<W>
{
    /// <summary>
    ///     A computation that maintains a log.
    /// </summary>
    /// <param name="value">The computed value.</param>
    /// <param name="log">The computation log.</param>
    /// <typeparam name="T">The type of the computed value.</typeparam>
    public partial class Writer<T>(T value, W log)
    {
        /// <summary>
        ///     Runs the <see cref="Writer{T}"/>.
        /// </summary>
        /// <returns>The computed value and its corresponding log.</returns>
        public (T Value, W Log) Run() =>
            (value, log);

        /// <summary>
        ///     Applies the provided function to the log.
        /// </summary>
        /// <param name="map">The function to apply.</param>
        /// <returns>A <see cref="Writer{T}"/> with the updated log.</returns>
        public Writer<T> Pass(Func<W, W> map) =>
            new(value, map(log));

        /// <inheritdoc/>
        public override string ToString() =>
            $"Writer ({value}, {log})";
    }

    // Pretending that C# supports type class instances. T_T
    public partial class Writer<T> : IMonad<T>
    {
        /// <inheritdoc cref="IMonad{T}.Docs.Bind"/>
        public Writer<U> Bind<U>(Func<T, Writer<U>> map)
        {
            var writer = map(value).Run();

            return new(writer.Value, log + writer.Log);
        }

        /// <inheritdoc cref="IMonad{T}.Docs.Return"/>
        public static Writer<T> Return(T value) =>
            new(value, W.AdditiveIdentity);
    }

    /// <summary>
    ///     A static facade for creating <see cref="Writer{T}"/> instances.
    /// </summary>
    public static class Writer
    {
        /// <summary>
        ///     Constructs a <see cref="Writer{T}"/> with the specified log.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <returns>The constructed <see cref="Writer{T}"/>.</returns>
        public static Writer<Unit> Tell(W log) =>
            new(default, log);

        /// <summary>
        ///     Constructs a <see cref="Writer{T}"/> with the specified value and log.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="log">The log.</param>
        /// <returns>The constructed <see cref="Writer{T}"/>.</returns>
        public static Writer<T> Tells<T>(T value, W log) =>
            new(value, log);
    }
}
