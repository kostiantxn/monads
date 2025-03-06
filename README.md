# Monads

Abusing compiler-generated `async` state machines to implement monads in C#.

## Abstract

Even though C# is not exactly a functional language, it provides some very powerful features that make it easy to write functional programs.
For example, [query expressions](https://learn.microsoft.com/en-us/dotnet/csharp/linq/get-started/query-expression-basics) are so rich that they allow us to implement monads [quite easily](https://ericlippert.com/category/monads/).

However, query expressions are not the only language feature that is rich enough to do that.
It is easy to [show](https://devblogs.microsoft.com/pfxteam/tasks-monads-and-linq/) that the standard [`Task<T>`](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1) type also forms a monad, and it has special syntax designed for it: namely, `async`/`await`, which allows us to combine multiple tasks, just like the `>>=` bind operator combines monadic values in Haskell.

## Example

Consider the [`Maybe<T>`](src/Monads/Maybe.cs) type, which represents an optional value.

If we define the `Parse` function, which parses type `T` from a string:
```cs
Maybe<T> Parse<T>(string input) where T : IParsable<T> =>
    T.TryParse(input, provider: null, out var value)
        ? Maybe.Just(value) 
        : Maybe.Nothing<T>();
```
and the `Divide` function, which divides two integers:
```cs
Maybe<int> Divide(int x, int y) =>
    y != 0
        ? Maybe.Just(x / y)
        : Maybe.Nothing<int>();
```
we can then combine them like so:
```cs
async Maybe<int> Do(string x, string y)
{
    var a = await Parse<int>(x);
    var b = await Parse<int>(y);

    return await Divide(a, b);
}
```

The `await` operator binds the value from the monad to the variable, depending on the monad type.
In this example, `await` will try to "unwrap" a value from `Maybe<T>`; however, if `Maybe<T>` contains nothing, the method execution will be terminated, and `Nothing` will be immediately returned.
So, if we call `Do("a", "b")`, `Do("7", "b")`, or `Do("7", "0")`, the method will return `Nothing` (because it will either fail to parse a number, or will not be able to divide by 0), but `Do("7", "2")` will succeed and return `Just(3)`.

So, how does this work?

## Implementation

C# allows us to override the behaviour of `async` and `await`.
The compiler generates a state machine for `async` methods, and types marked with the [`AsyncMethodBuilderAttribute`](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.asyncmethodbuilderattribute) can control how the state machine is executed.

This library defines a Roslyn source generator, which automatically generates a custom `AsyncMethodBuilder` for types that implement the [`IMonad<T>`](src/Monads/Core/IMonad.cs) interface.

Even though `IMonad<T>` does not define any methods, the implementing types must implement the `Bind` method:
```cs
public M<U> Bind<U>(Func<T, M<U>> map);
```
and the static `Return` method:
```cs
public static M<T> Return(T value);
```
where `M` is the monad type (e.g., `Maybe<T>`, `Result<T>`, `List<T>`, etc.).
Unfortunately, these methods cannot be defined in the interface since C# does not support higher-kinded types; however, the methods are used by the generated custom `AsyncMethodBuilder`, so that when a monadic value is awaited, its corresponding `Bind` method is invoked under the hood.

## License

The project is licensed under the [MIT](LICENSE) license.
