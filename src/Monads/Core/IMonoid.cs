using System.Numerics;

namespace Monads;

/// <summary>
///     The monoid type class.
///     <para/>
///     Monoids are types equipped with an associative binary operation
///     (<c>+</c>) and an identity element (<c>0</c>).
/// </summary>
/// <typeparam name="T">The type that forms a monoid.</typeparam>
public interface IMonoid<T> : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
    where T : IMonoid<T>;
