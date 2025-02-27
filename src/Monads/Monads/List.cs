using System.Collections;

namespace Monads;

public partial class List<T>(IEnumerable<T> items) : IEnumerable<T>, IMonad<T>
{
    private readonly System.Collections.Generic.List<T> _items = items.ToList();

    public IEnumerator<T> GetEnumerator() =>
        _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public List<TMap> Bind<TMap>(Func<T, List<TMap>> map) =>
        new(_items.SelectMany(map));

    public static List<T> Return(T value) =>
        new([value]);

    public override string ToString() =>
       "[" + string.Join(", ", _items) + "]";
}
