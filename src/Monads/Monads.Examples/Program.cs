using System.Runtime.CompilerServices;
using Unit = System.ValueTuple;

Console.WriteLine(Maybe());
Console.WriteLine();
Console.WriteLine(Result("x", "y"));
Console.WriteLine(Result("5", "y"));
Console.WriteLine(Result("5", "0"));
Console.WriteLine(Result("5", "2"));
Console.WriteLine();
Console.WriteLine(List(3));
Console.WriteLine();
Console.WriteLine(Reader().Run(new Configuration { Value = "1984" }));
Console.WriteLine();
Console.WriteLine(Writer());
Console.WriteLine();
Console.WriteLine(State().Run(0));
Console.WriteLine(State().Run(7));
Console.WriteLine();

_ = Main().Run();

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

async Monads.Result<int> Result(string a, string b)
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

async Monads.List<(int, int, int)> List(int limit)
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

// Unfortunately, C# doesn't allow `AsyncMethodBuilder`s to have more than
// 1 generic parameter, so we have to add the attribute to each method
// individually in order to capture the extra generic parameters.
//
// For example, here, instead of writing `Reader<Configuration, int>`,
// we have to "capture" the `Configuration` and then mark the method with
// `Configuration.ReaderMonadMethodBuilder<>`, which only has 1 generic
// parameter.
[AsyncMethodBuilder(typeof(Configuration.ReaderMonadMethodBuilder<>))]
async Configuration.Reader<int> Reader()
{
    var configuration = await Configuration.Reader.Ask();

    return int.Parse(configuration.Value);
}

[AsyncMethodBuilder(typeof(History.WriterMonadMethodBuilder<>))]
async History.Writer<int> Writer()
{
    await History.Writer.Tell("subtracting numbers");

    var a = await History.Writer.Tell(3, "got number 3");
    var b = await History.Writer.Tell(5, "got number 5");

    return a - b;
}

[AsyncMethodBuilder(typeof(Machine.StateMonadMethodBuilder<>))]
async Machine.State<string> State()
{
    var state = await Machine.State.Get();
    if (state.Value is 0)
    {
        await Machine.State.Put(+1);
        return "0 is the initial state!";
    }
    else
    {
        await Machine.State.Put(-1);
        return $"idk what {state.Value} is";
    }
}

async Monads.IO<Unit> Main()
{
    await Monads.IO.Console.Write("Your name: ");

    var name = await Monads.IO.Console.ReadLine();

    await Monads.IO.Console.WriteLine($"Hello, {name}");

    return default;
}

public partial class Configuration
    : Monads.Environment<Configuration>
{
    public required string Value { get; init; }
}

public partial class History(IEnumerable<string> items)
    : Monads.Log<History>, Monads.IMonoid<History>
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

public partial class Machine
    : Monads.Given<Machine>
{
    public int Value { get; init; }

    public override string ToString() =>
        $"Machine {{ Value = {Value} }}";

    public static implicit operator Machine(int value) =>
        new() { Value = value };
}
