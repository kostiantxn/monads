using Monads.Generators;
using Monads.Tests.Generators.Core;

namespace Monads.Tests.Generators;

public class CaptureGeneratorTests
{
    [Fact]
    public Task Ignores_WhenTypeDoesNotCapture() =>
        Verify(
            // lang=C#
            """
            public class Whatever
            {
            }
            """);

    [Fact]
    public Task Ignores_WhenTypeIsMonad() =>
        Verify(
            // lang=C#
            """
            using Monads;
            
            public class Whatever<T> : IMonad<T>
            {
            }
            """);

    [Fact]
    public Task Generates_WhenTypeCapturesTypeParameters() =>
        Verify(
            // lang=C#
            """
            using Monads;
            
            public abstract partial class Environment<E> : ICapture
            {
                public partial class Reader<T> : IMonad<T>
                {
                }
            }
            
            public class Configuration : Environment<Configuration>
            {
                public string Value { get; set; }
            }
            """);

    private static Task Verify(string source) =>
        Runner.Verify<CaptureGenerator>(
            source,
            assemblies: [typeof(IMonad<>).Assembly]);
}
