using System.Collections;

namespace Monads;

/// <summary>
///     An immutable linked list.
/// </summary>
/// <param name="items">The list contents.</param>
/// <typeparam name="T">The type of list items.</typeparam>
public partial class List<T>(IEnumerable<T> items) : IEnumerable<T>
{
    private readonly LinkedList<T> _items = new(items);

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() =>
        _items.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    /// <inheritdoc/>
    public override string ToString() =>
       "[" + string.Join(", ", _items) + "]";
}

// Pretending that C# supports type class instances. T_T
public partial class List<T> : IMonad<T>
{
    /// <inheritdoc cref="IMonad{T}.Docs.Bind"/>
    public List<U> Bind<U>(Func<T, List<U>> map) =>
        new(_items.SelectMany(map));

    /// <inheritdoc cref="IMonad{T}.Docs.Return"/>
    public static List<T> Return(T value) =>
        new([value]);
}

/// <summary>
///     A static facade for creating <see cref="List{T}"/> instances.
/// </summary>
public static class List
{
    /// <summary>
    ///     Returns an empty list.
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="List{T}"/></typeparam>
    /// <returns>An empty list.</returns>
    public static List<T> Empty<T>() =>
        new([]);

    /// <inheritdoc cref="From{T}(IEnumerable{T})"/>
    public static List<T> From<T>(params T[] items) =>
        new(items);

    /// <summary>
    ///     Returns a list that contains the specified items.
    /// </summary>
    /// <param name="items"><inheritdoc cref="List{T}"/></param>
    /// <typeparam name="T"><inheritdoc cref="List{T}"/></typeparam>
    /// <returns>A list that contains the specified items.</returns>
    public static List<T> From<T>(IEnumerable<T> items) =>
        new(items);
}
