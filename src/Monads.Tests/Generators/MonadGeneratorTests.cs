using Monads.Generators;
using Monads.Tests.Generators.Core;

namespace Monads.Tests.Generators;

public class MonadGeneratorTests
{
    [Fact]
    public Task Ignores_WhenTypeIsNotMonad() =>
        Verify(
            // lang=C#
            """
            public class Whatever
            {
            }
            """);

    [Fact]
    public Task Generates_WhenTypeIsMonad() =>
        Verify(
            // lang=C#
            """
            using Monads;
            
            public partial class Identity<T> : IMonad<T>
            {
            }
            """);

    [Fact]
    public Task Generates_WhenTypeIsMonad_AndHasContainingTypeThatCapturesTypeParameters() =>
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
            """);

    private static Task Verify(string source) =>
        Runner.Verify<MonadGenerator>(
            source,
            assemblies: [typeof(IMonad<>).Assembly]);
}
