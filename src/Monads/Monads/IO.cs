using Unit = System.ValueTuple;

namespace Monads;

/// <summary>
///     A computation that performs I/O operations.
/// </summary>
/// <param name="run">The computation function.</param>
/// <typeparam name="T">The function output type.</typeparam>
public partial class IO<T>(Func<IO.World, (T Value, IO.World World)> run)
{
    /// <summary>
    ///     Runs the I/O operation.
    /// </summary>
    /// <returns>The computed I/O value.</returns>
    /// <remarks>
    ///     Should only be run from the <c>Main</c> function.
    /// </remarks>
    public (T Value, IO.World World) Run() =>
        Run(IO.World.Default);

    /// <inheritdoc cref="Run()"/>
    /// <param name="world">The real world to run the computation in.</param>
    public (T Value, IO.World World) Run(IO.World world) =>
        run(world);
}

// Pretending that C# supports type class instances. T_T
public partial class IO<T> : IMonad<T>
{
    /// <inheritdoc cref="IMonad{T}.Docs.Bind"/>
    public IO<U> Bind<U>(Func<T, IO<U>> map) =>
        new(x =>
        {
            var next = Run(x);

            return map(next.Value).Run(next.World);
        });

    /// <inheritdoc cref="IMonad{T}.Docs.Return"/>
    public static IO<T> Return(T value) =>
        new(x => (value, x));
}

/// <summary>
///     A static facade for creating <see cref="IO{T}"/> instances.
/// </summary>
public static class IO
{
    /// <summary>
    ///     The real world where I/O operations can be performed.
    /// </summary>
    public record World
    {
        /// <summary>
        ///     The default world.
        /// </summary>
        public static readonly World Default = new();

        /// <summary>
        ///     The standard console input and output streams.
        /// </summary>
        public Console Console { get; init;  } = new();
    }

    /// <summary>
    ///     A static facade for creating <see cref="IO{T}"/> instances
    ///     that wrap the standard <see cref="System.Console"/> operations.
    /// </summary>
    public class Console
    {
        /// <summary>
        ///     The console input stream.
        /// </summary>
        /// <remarks>
        ///     Defaults to <see cref="System.Console.In"/>.
        /// </remarks>
        public TextReader In { get; init; } = System.Console.In;

        /// <summary>
        ///     The console output stream.
        /// </summary>
        /// <remarks>
        ///     Defaults to <see cref="System.Console.Out"/>.
        /// </remarks>
        public TextWriter Out { get; init; } = System.Console.Out;

        /// <summary>
        ///     Constructs an <see cref="IO{T}"/> instance that wraps
        ///     the <see cref="System.Console.Write(string?)"/> method.
        /// </summary>
        /// <param name="value"><inheritdoc cref="System.Console.Write(string?)"/></param>
        /// <returns>The constructed <see cref="IO{T}"/> instance.</returns>
        public static IO<Unit> Write(string? value) =>
            From(x => x.Console.Out.Write(value));

        /// <summary>
        ///     Constructs an <see cref="IO{T}"/> instance that wraps
        ///     the <see cref="System.Console.WriteLine(string?)"/> method.
        /// </summary>
        /// <param name="value"><inheritdoc cref="System.Console.WriteLine(string?)"/></param>
        /// <returns>The constructed <see cref="IO{T}"/> instance.</returns>
        public static IO<Unit> WriteLine(string? value) =>
            From(x => x.Console.Out.WriteLine(value));

        /// <summary>
        ///     Constructs an <see cref="IO{T}"/> instance that wraps
        ///     the <see cref="System.Console.ReadLine()"/> method.
        /// </summary>
        /// <returns>
        ///     The constructed <see cref="IO{T}"/> instance.
        ///     <para/>
        ///     <inheritdoc cref="System.Console.ReadLine()"/>
        /// </returns>
        public static IO<string?> ReadLine() =>
            From(x => x.Console.In.ReadLine());
    }

    /// <summary>
    ///     Constructs an <see cref="IO{T}"/> instance from the specified
    ///     function that performs I/O operations.
    /// </summary>
    /// <param name="run">The computation function.</param>
    /// <returns>The constructed <see cref="IO{T}"/> instance.</returns>
    public static IO<Unit> From(Action run) =>
        new(x => { run(); return (default, x); });

    /// <inheritdoc cref="From(Action)"/>
    public static IO<Unit> From(Action<World> run) =>
        new(x => { run(x); return (default, x); });

    /// <summary>
    ///     Constructs an <see cref="IO{T}"/> instance from the specified
    ///     computed value.
    /// </summary>
    /// <param name="value">The computed value.</param>
    /// <typeparam name="T">The computed value type.</typeparam>
    /// <returns>The constructed <see cref="IO{T}"/> instance.</returns>
    public static IO<T> From<T>(T value) =>
        IO<T>.Return(value);

    /// <inheritdoc cref="From(Action)"/>
    /// <typeparam name="T">The function output type.</typeparam>
    public static IO<T> From<T>(Func<T> run) =>
        new(x => (run(), x));

    /// <inheritdoc cref="From{T}(Func{T})"/>
    public static IO<T> From<T>(Func<World, T> run) =>
        new(x => (run(x), x));

    /// <inheritdoc cref="From{T}(Func{T})"/>
    public static IO<T> From<T>(Func<World, (T, World)> run) =>
        new(run);
}
