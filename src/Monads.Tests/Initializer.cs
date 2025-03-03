using System.Runtime.CompilerServices;

namespace Monads.Tests;

public static class Initializer
{
    [ModuleInitializer]
    public static void Init() =>
        VerifySourceGenerators.Initialize();
}
