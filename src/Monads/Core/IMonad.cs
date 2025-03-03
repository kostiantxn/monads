namespace Monads;

/// <summary>
///     The monad type class.
/// </summary>
/// <typeparam name="T">The type that the container (implementing) type contains.</typeparam>
/// <remarks>
///     This is a marker interface used by <c>Monads.Generators</c> to generate
///     an <c>AsyncMethodBuilder</c>, as well as the <c>GetAwaiter</c> method,
///     which allow the types that implement <see cref="IMonad{T}"/> to be used
///     in <c>async</c> methods.
///     <para/>
///     Thus, the implementing type <c>I&lt;T&gt;</c> must define
///     the <see cref="Docs.Bind"/> operation:
///     <code>
///     I&lt;U&gt; Bind&lt;U&gt;(Func&lt;T, I&lt;U&gt;&gt; map)
///     </code>
///     as well as the <see cref="Docs.Return"/> operation:
///     <code>
///     static I&lt;T&gt; Return(T value)
///     </code>
///     <para/>
///     The generated <c>AsyncMethodBuilder</c> invokes the <c>Bind</c> method
///     defined by the implementing type. This essentially makes <c>async</c>
///     monadic methods equivalent to the
///     <a href="https://wiki.haskell.org/Monad#do-notation"><c>do</c> notation</a>
///     in Haskell.
/// </remarks>
public interface IMonad<T>
{
    protected static class Docs
    {
        /// <summary>
        ///     The <c>bind</c> operation (<c>>>=</c>) that combines two
        ///     monadic values.
        /// </summary>
        /// <remarks>
        ///     In Haskell, <c>bind</c> has the following definition:
        ///     <code>
        ///     >>= :: m a -> (a -> m b) -> m b
        ///     </code>
        ///     In this definition, <c>m</c> is a higher-kinded type (i.e.,
        ///     generic generic). We can think of it as any type that has one
        ///     generic parameter, such as <c>List&lt;T&gt;</c>,
        ///     <c>Maybe&lt;T&gt;</c>, <c>Result&lt;T&gt;</c>, etc.
        ///     <para/>
        ///     Since C# does not support higher-kinded types, we can't define
        ///     this method in the interface, unfortunately.
        /// </remarks>
        /// <example>
        ///     In C#, an equivalent definition for <c>Maybe&lt;T&gt;</c> would
        ///     look like so:
        ///     <code>
        ///     Maybe&lt;U&gt; Bind&lt;U&gt;(Func&lt;T, Maybe&lt;U&gt;&gt; map) =>
        ///         IsJust(out var value)
        ///             ? map(value)
        ///             : Maybe&lt;U&gt;.Nothing();
        ///     </code>
        ///     The <c>Return</c> method simply wraps <c>value</c> into
        ///     a <c>Maybe</c> that contains the specified value.
        /// </example>
        public static class Bind;

        /// <summary>
        ///     The <c>return</c> operation that injects a value into the monad.
        /// </summary>
        /// <remarks>
        ///     In Haskell, <c>return</c> has the following definition:
        ///     <code>
        ///     return :: a -> m a
        ///     </code>
        ///     In this definition, <c>m</c> is a higher-kinded type (i.e.,
        ///     generic generic). We can think of it as any type that has one
        ///     generic parameter, such as <c>List&lt;T&gt;</c>,
        ///     <c>Maybe&lt;T&gt;</c>, <c>Result&lt;T&gt;</c>, etc.
        ///     <para/>
        ///     Since C# does not support higher-kinded types, we can't define
        ///     this method in the interface, unfortunately.
        /// </remarks>
        /// <example>
        ///     In C#, an equivalent definition for <c>Maybe&lt;T&gt;</c> would
        ///     look like so:
        ///     <code>
        ///     static Maybe&lt;T&gt; Return(T value) =>
        ///         Just(value);
        ///     </code>
        ///     The <c>Return</c> method simply wraps <c>value</c> into
        ///     a <c>Maybe</c> that contains the specified value.
        /// </example>
        public static class Return;
    }
}

public interface ICapture;
