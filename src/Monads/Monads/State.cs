using Unit = System.ValueTuple;

namespace Monads;

/// <summary>
///     The <see cref="State{T}"/> state.
/// </summary>
/// <typeparam name="S">The state type that <see cref="State{T}"/> carries.</typeparam>
public abstract partial class Given<S> : ICapture
{
    /// <summary>
    ///     A computation that carries an internal state.
    /// </summary>
    /// <param name="run">A function that depends on the internal state.</param>
    /// <typeparam name="T">The function output type.</typeparam>
    public partial class State<T>(Func<S, (T, S)> run)
    {
        /// <summary>
        ///     Runs the <see cref="State{T}"/> given the initial state.
        /// </summary>
        /// <param name="state">The initial state.</param>
        /// <returns>The computed value and the final state.</returns>
        public (T Value, S State) Run(S state) =>
            run(state);

        /// <summary>
        ///     Runs the <see cref="State{T}"/> given the initial state,
        ///     and returns the computed value.
        /// </summary>
        /// <param name="state">The initial state.</param>
        /// <returns>The computed value.</returns>
        public T Evaluate(S state) =>
            Run(state).Value;

        /// <summary>
        ///     Runs the <see cref="State{T}"/> given the initial state,
        ///     and returns the final state.
        /// </summary>
        /// <param name="state">The initial state.</param>
        /// <returns>The final state.</returns>
        public S Execute(S state) =>
            Run(state).State;

        /// <summary>
        ///     Modifies the internal state.
        /// </summary>
        /// <param name="map">The function that modifies the state.</param>
        /// <returns>The modified <see cref="State{T}"/>.</returns>
        public State<T> With(Func<S, S> map) =>
            new(x => Run(map(x)));

        /// <inheritdoc/>
        public override string ToString() =>
            $"State {typeof(S).Name} {typeof(T).Name}";
    }

    // Pretending that C# supports type class instances. T_T
    public partial class State<T> : IMonad<T>
    {
        /// <inheritdoc cref="IMonad{T}.Docs.Bind"/>
        public State<U> Bind<U>(Func<T, State<U>> map) =>
            new(x =>
            {
                var next = Run(x);

                return map(next.Value).Run(next.State);
            });

        /// <inheritdoc cref="IMonad{T}.Docs.Return"/>
        public static State<T> Return(T value) =>
            new(x => (value, x));
    }

    /// <summary>
    ///     A static facade for creating <see cref="State{T}"/> instances.
    /// </summary>
    public static class State
    {
        /// <summary>
        ///     Constructs a <see cref="State{T}"/> with
        ///     the specified internal state.
        /// </summary>
        /// <param name="state">The state to set.</param>
        /// <returns>The constructed <see cref="State{T}"/>.</returns>
        public static State<Unit> Put(S state) =>
            new(_ => (default, state));

        /// <summary>
        ///     Constructs a <see cref="State{T}"/> that applies the specified
        ///     function to modify the internal state.
        /// </summary>
        /// <param name="map">The function to apply to the internal state.</param>
        /// <returns>The constructed <see cref="State{T}"/>.</returns>
        public static State<Unit> Put(Func<S, S> map) =>
            new(x => (default, map(x)));

        /// <summary>
        ///     Constructs a <see cref="State{T}"/> that returns
        ///     the internal state.
        /// </summary>
        /// <returns>The constructed <see cref="State{T}"/>.</returns>
        public static State<S> Get() =>
            new(x => (x, x));

        /// <summary>
        ///     Constructs a <see cref="State{T}"/> that applies
        ///     the specified function to the internal state.
        /// </summary>
        /// <param name="map">The function to apply to the internal state.</param>
        /// <typeparam name="T">The function output type.</typeparam>
        /// <returns>The constructed <see cref="State{T}"/>.</returns>
        public static State<T> Get<T>(Func<S, T> map) =>
            new(x => (map(x), x));
    }
}
