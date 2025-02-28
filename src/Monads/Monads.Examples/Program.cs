Console.WriteLine(Maybe());
Console.WriteLine();
Console.WriteLine(Do("1", "a"));
Console.WriteLine();
Console.WriteLine(Triples(3));
Console.WriteLine();
Console.WriteLine(Product(3));

async Monads.Maybe<int> Maybe()
{
    // This line will unwrap the underlying 123456 value and print it.
    Console.WriteLine(await Monads.Maybe.Just(123456));

    // This line will stop the execution, since there is nothing to unwrap.
    // The method will immediately return `Nothing` to the caller.
    Console.WriteLine(await Monads.Maybe.Nothing<string>());

    // This line will never be reached because of the line above.
    return 0xDEAD;
}

Monads.Result<T> Parse<T>(string input) where T : IParsable<T> =>
    Monads.Result.Try(() => T.Parse(input, null));

Monads.Result<int> Divide(int x, int y) =>
    Monads.Result.Try(() => x / y);

async Monads.Result<int> Do(string a, string b)
{
    // This code will try to parse the parameter `b` and, if successful,
    // unwrap the parsed value into the local variable `x`. Otherwise,
    // the execution will stop, and the error will be returned to the caller
    // in an erroneous result.
    Console.Write("Parsing `a`... ");
    var x = await Parse<int>(a);
    Console.WriteLine("Done!");

    // This code will try to parse the parameter `b` and, if successful,
    // unwrap the parsed value into the local variable `y`. Otherwise,
    // the execution will stop, and the error will be returned to the caller
    // in an erroneous result.
    Console.Write("Parsing `b`... ");
    var y = await Parse<int>(b);
    Console.WriteLine("Done!");

    // This line will try to divide the parsed values and return the result.
    return await Divide(x, y);
}

async Monads.List<(int, int, int)> Triples(int limit)
{
    // The variable `n` will loop through values 1 to `limit`, and for each
    // value, the rest of the method will be executed.
    var n = await Monads.List.From(Enumerable.Range(1, limit));

    Console.WriteLine($"n: {n}");

    // For each value of `n`, the variable `m` will loop through values `n` + 1
    // to `limit`, and for each value, the rest of the method will be executed.
    var m = await Monads.List.From(Enumerable.Range(n + 1, limit - n));

    Console.WriteLine($"  - m: {m}");

    // The returned scalar value is wrapped into a list that contains
    // the singleton value. All the lists that were produced for each pair
    // of `n` and `m` will be concatenated into a single flat list that
    // contains the generated Pythagorean triples.
    return (m*m - n*n, 2*m*n, m*m + n*n);
}

async Monads.List<int> Product(int limit)
{
    return
        await Monads.List.From(Enumerable.Range(1, limit)) *
        await Monads.List.From(-1, +1);
}
