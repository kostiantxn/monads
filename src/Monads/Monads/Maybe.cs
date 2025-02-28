using System.Diagnostics.CodeAnalysis;

namespace Monads;

/// <summary>
///     An optional value.
/// </summary>
/// <typeparam name="T">The type of the contained value.</typeparam>
public readonly partial record struct Maybe<T>
{
    private readonly bool _just;
    private readonly T? _value;

    /// <summary>
    ///     Constructs a <see cref="Maybe{T}"/> that contains a value.
    /// </summary>
    /// <param name="value">The contained value.</param>
    public Maybe(T value)
    {
        _just = true;
        _value = value;
    }

    /// <summary>
    ///     Constructs a <see cref="Maybe{T}"/> that contains nothing.
    /// </summary>
    public Maybe()
    {
        _just = false;
        _value = default;
    }

    /// <inheritdoc cref="Maybe{T}(T)"/>
    /// <returns>The constructed <see cref="Maybe{T}"/>.</returns>
    public static Maybe<T> Just(T value) =>
        new(value);

    /// <inheritdoc cref="Maybe{T}()"/>
    /// <returns>The constructed <see cref="Maybe{T}"/>.</returns>
    public static Maybe<T> Nothing() =>
        new();

    /// <summary>
    ///     Returns <c>true</c> if the <see cref="Maybe{T}"/> contains a value;
    ///     otherwise, returns <c>false</c>.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the <see cref="Maybe{T}"/> contains a value,
    ///     and <c>false</c> otherwise.
    /// </returns>
    public bool IsJust() =>
        IsJust(out _);

    /// <inheritdoc cref="IsJust()"/>
    /// <param name="value">The contained value.</param>
    public bool IsJust([NotNullWhen(true)] out T? value)
    {
        if (_just)
        {
            value = _value!;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    ///     Returns <c>true</c> if the <see cref="Maybe{T}"/> contains nothing;
    ///     otherwise, returns <c>false</c>.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the <see cref="Maybe{T}"/> contains nothing,
    ///     and <c>false</c> otherwise.
    /// </returns>
    public bool IsNothing() =>
        !_just;

    /// <inheritdoc/>
    public override string ToString() =>
        _just ? "Just " + _value : "Nothing";
}

// Pretending that C# supports type class instances. T_T
public readonly partial record struct Maybe<T> : IMonad<T>
{
    /// <inheritdoc cref="IMonad{T}.Docs.Bind"/>
    public Maybe<U> Bind<U>(Func<T, Maybe<U>> map) =>
        IsJust(out var value)
            ? map(value)
            : Maybe<U>.Nothing();

    /// <inheritdoc cref="IMonad{T}.Docs.Return"/>
    public static Maybe<T> Return(T value) =>
        Just(value);
}

/// <summary>
///     A static facade for creating <see cref="Maybe{T}"/> instances.
/// </summary>
public static class Maybe
{
    /// <inheritdoc cref="Maybe{T}.Just(T)"/>
    public static Maybe<T> Just<T>(T value) =>
        Maybe<T>.Just(value);

    /// <inheritdoc cref="Maybe{T}.Nothing()"/>
    public static Maybe<T> Nothing<T>() =>
        Maybe<T>.Nothing();
}
