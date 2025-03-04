using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Monads.Tests.Generators.Core;

internal static class Runner
{
    private const string Assembly = "Tests";
    private const string Directory = "Snapshots";

    public static GeneratorDriver Run<T>(string source, Assembly[]? assemblies = null)
        where T : IIncrementalGenerator, new() =>
        Run(source, generators: [new T()], assemblies);

    public static GeneratorDriver Run(string source, IIncrementalGenerator[] generators, Assembly[]? assemblies = null)
    {
        var references = (assemblies ?? [])
            .Append(typeof(object).Assembly)
            .Select(x => x.Location)
            .Distinct()
            .Select(x => MetadataReference.CreateFromFile(x));

        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var tree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(Assembly, [tree], references, options);

        var errors = compilation.GetDiagnostics().RemoveAll(x => x.Severity is not DiagnosticSeverity.Error);
        if (errors.Any())
            throw new InvalidOperationException(
                "Could not compile the provided source: \n" + string.Join("\n", errors));

        var driver = CSharpGeneratorDriver.Create(generators);
        var result = driver.RunGenerators(compilation);

        return result;
    }

    public static Task Verify<T>(string source, Assembly[]? assemblies = null)
        where T : IIncrementalGenerator, new() =>
        Verifier.Verify(Run<T>(source, assemblies)).UseDirectory(Directory);

    public static Task Verify(string source, IIncrementalGenerator[] generators, Assembly[]? assemblies = null) =>
        Verifier.Verify(Run(source, generators, assemblies)).UseDirectory(Directory);
}
