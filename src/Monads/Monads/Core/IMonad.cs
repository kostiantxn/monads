namespace Monads.Core;

public interface IMonad
{
    IMonad<TMap> Bind<TMap>(Func<object?, IMonad<TMap>> map);
}

public interface IMonad<out T> : IMonad
{
    IMonad<TMap> IMonad.Bind<TMap>(Func<object?, IMonad<TMap>> map) =>
        Bind<TMap>(x => map(x));

    IMonad<TMap> Bind<TMap>(Func<T, IMonad<TMap>> map);
}