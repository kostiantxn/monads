using System.Numerics;
using System.Runtime.CompilerServices;

Console.WriteLine(Maybe());
Console.WriteLine();
Console.WriteLine(Do("x", "y"));
Console.WriteLine(Do("5", "y"));
Console.WriteLine(Do("5", "0"));
Console.WriteLine(Do("5", "2"));
Console.WriteLine();
Console.WriteLine(Triples(3));
Console.WriteLine();
Console.WriteLine(Product(5));
Console.WriteLine();
Console.WriteLine(Injection().Run(new Configuration(value: "1984")));
Console.WriteLine();
Console.WriteLine(Computation());

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
    // This code will try to parse the parameter `a` and, if successful,
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

// Unfortunately, C# doesn't allow `AsyncMethodBuilder`s to have more than
// 1 generic parameter, so we have to add the attribute to each method
// individually in order to capture the extra generic parameters.
//
// For example, here, instead of writing `Reader<Configuration, int>`,
// we have to "capture" the `Configuration` and then mark the method with
// `Configuration.ReaderMonadMethodBuilder<>`, which only has 1 generic
// parameter.
[AsyncMethodBuilder(typeof(Configuration.ReaderMonadMethodBuilder<>))]
async Configuration.Reader<int> Injection()
{
    var configuration = await Configuration.Reader.Ask();

    return int.Parse(configuration.Value);
}

[AsyncMethodBuilder(typeof(History.WriterMonadMethodBuilder<>))]
async History.Writer<int> Computation()
{
    await History.Writer.Tell("subtracting numbers");

    var a = await History.Writer.Tells(3, "got number 3");
    var b = await History.Writer.Tells(5, "got number 5");

    return a - b;
}

public partial class Configuration(string value)
    : Monads.Environment<Configuration>
{
    public string Value { get; } = value;
}

public partial class History(IEnumerable<string> items)
    : Monads.Log<History>, IAdditionOperators<History, History, History>, IAdditiveIdentity<History, History>
{
    public IReadOnlyList<string> Items { get; } = items.ToList();

    public override string ToString() =>
        "History [" + string.Join(", ", items.Select(x => '"' + x + '"')) + "]";

    public static History AdditiveIdentity =>
        new([]);

    public static History operator +(History left, History right) =>
        new(left.Items.Concat(right.Items));

    public static implicit operator History(string item) =>
        new([item]);
}
