// See https://aka.ms/new-console-template for more information

Console.WriteLine(Do("1", "b"));
Console.WriteLine();
Console.WriteLine(Product(3));
Console.WriteLine();
Console.WriteLine(Triples(3));

Monads.Result<T> Parse<T>(string input) where T : IParsable<T> =>
    Monads.Result.Try(() => T.Parse(input, null));

Monads.Result<double> Divide(double x, double y) =>
    Monads.Result.Try(() => x / y);

async Monads.Result<double> Do(string a, string b)
{
    Console.Write("Parsing `a`... ");
    var x = await Parse<int>(a);
    Console.WriteLine("Done!");

    Console.Write("Parsing `b`... ");
    var y = await Parse<double>(b);
    Console.WriteLine("Done!");

    return await Divide(x, y);
}

async Monads.List<int> Product(int limit)
{
    return
        await new Monads.List<int>(Enumerable.Range(1, limit)) *
        await new Monads.List<int>([-1, +1]);
}

async Monads.List<(int, int, int)> Triples(int limit)
{
    var n = await new Monads.List<int>(Enumerable.Range(1, limit));

    Console.WriteLine($"n: {n}");

    var m = await new Monads.List<int>(Enumerable.Range(n + 1, limit - n));

    Console.WriteLine($"- m: {m}");

    return (m*m - n*n, 2*m*n, m*m + n*n);
}
