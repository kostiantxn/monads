using System.Diagnostics.CodeAnalysis;

namespace Monads;

/// <summary>
///     A value with two possibilities: left or right.
/// </summary>
/// <typeparam name="S">The type of the left value.</typeparam>
public partial class Either<S> : ICapture
{
    /// <summary>
    ///     A value with two possibilities: left or right.
    /// </summary>
    /// <typeparam name="T">The type of the right value.</typeparam>
    public partial class Or<T>
    {
        private readonly S? _left;
        private readonly T? _right;
        private readonly bool _side;

        /// <summary>
        ///     Constructs a <see cref="Either{S}.Or{T}"/> that contains
        ///     the left value.
        /// </summary>
        /// <param name="left">The left value.</param>
        public Or(S left)
        {
            _left = left;
            _right = default;
            _side = false;
        }

        /// <summary>
        ///     Constructs a <see cref="Either{S}.Or{T}"/> that contains
        ///     the right value.
        /// </summary>
        /// <param name="right">The right value.</param>
        public Or(T right)
        {
            _left = default;
            _right = right;
            _side = true;
        }

        /// <inheritdoc cref="Either{S}.Or{T}(S)"/>
        /// <returns>The constructed <see cref="Either{S}.Or{T}"/>.</returns>
        public static Or<T> Left(S value) =>
            new(value);

        /// <inheritdoc cref="Either{S}.Or{T}(T)"/>
        /// <returns>The constructed <see cref="Either{S}.Or{T}"/>.</returns>
        public static Or<T> Right(T value) =>
            new(value);

        /// <summary>
        ///     Returns <c>true</c> if <see cref="Either{S}.Or{T}"/> contains
        ///     the left value; otherwise, returns <c>false</c>.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if <see cref="Either{S}.Or{T}"/> contains
        ///     the left value, and <c>false</c> otherwise.
        /// </returns>
        public bool IsLeft() =>
            IsLeft(out _);

        /// <inheritdoc cref="IsLeft()"/>
        /// <param name="value">The left value.</param>
        public bool IsLeft([NotNullWhen(true)] out S? value) =>
            IsLeft(out value, out _);

        /// <inheritdoc cref="IsLeft()"/>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        public bool IsLeft([NotNullWhen(true)] out S? left, [NotNullWhen(false)] out T? right)
        {
            left = _left;
            right = _right;

            return !_side;
        }

        /// <summary>
        ///     Returns <c>true</c> if <see cref="Either{S}.Or{T}"/> contains
        ///     the right value; otherwise, returns <c>false</c>.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if <see cref="Either{S}.Or{T}"/> contains
        ///     the right value, and <c>false</c> otherwise.
        /// </returns>
        public bool IsRight() =>
            IsRight(out _);

        /// <inheritdoc cref="IsRight()"/>
        /// <param name="value">The right value.</param>
        public bool IsRight([NotNullWhen(true)] out T? value)
        {
            value = _right;

            return _side;
        }

        /// <inheritdoc/>
        public override string ToString() =>
            _side ? $"Right {_right}" : $"Left {_left}";
    }

    // Pretending that C# supports type class instances. T_T
    public partial class Or<T> : IMonad<T>
    {
        /// <inheritdoc cref="IMonad{T}.Docs.Bind"/>
        public Or<U> Bind<U>(Func<T, Or<U>> map) =>
            IsLeft(out var left, out var right)
                ? Or<U>.Left(left)
                : map(right);

        /// <inheritdoc cref="IMonad{T}.Docs.Return"/>
        public static Or<T> Return(T value) =>
            Right(value);
    }
}
