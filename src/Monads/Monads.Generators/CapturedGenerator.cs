using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Monads.Generators.Constants;
using Monads.Generators.Extensions;

namespace Monads.Generators;

/// <summary>
///     An incremental source generator that collects types that capture any
///     extra generic parameters that a monad requires, and generates custom
///     captured <c>AsyncMethodBuilder</c> types for them.
/// </summary>
[Generator]
public class CapturedGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (x, _) => x is TypeDeclarationSyntax { BaseList.Types.Count: > 0 },
                (x, _) => x.SemanticModel.GetDeclaredSymbol(x.Node) as INamedTypeSymbol)
            .Where(x => x is { IsAbstract: false, BaseType: not null, TypeParameters.Length: 0 } && x.Implements(Types.ICapture))
            .Collect();

        context.RegisterSourceOutput(provider, Generate!);
    }

    private static void Generate(SourceProductionContext cx, ImmutableArray<INamedTypeSymbol> types)
    {
        var processed = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var type in types)
            if (processed.Add(type))
                Generate(cx, type);
    }

    private static void Generate(SourceProductionContext cx, INamedTypeSymbol type)
    {
        var processed = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var contents = new StringBuilder();

        foreach (var nested in type.BaseType!.GetTypeMembers())
        {
            if (!nested.Implements(Types.IMonad))
                continue;

            if (!processed.Add(nested))
                continue;

            if (processed.Count > 1)
                contents.AppendLine();

            contents.Append(MonadMethodBuilder(type, nested));
        }

        var source = new StringBuilder();

        if (!type.ContainingNamespace.IsGlobalNamespace)
        {
            source.AppendLine(
                // lang=C#
                $$"""
                namespace {{type.ContainingNamespace.Name}};

                """);
        }

        source
            .AppendLine(
                // lang=C#
                """
                using System.Runtime.CompilerServices;
                using System.Security;

                #nullable disable

                """)
            .AppendContainingType(type, contents.ToString());

        cx.AddSource(type.Name, source.ToString());
    }

    private static string MonadMethodBuilder(INamedTypeSymbol capture, INamedTypeSymbol nested) =>
        // lang=C#
        $$"""
        public new class {{nested.Name}}MonadMethodBuilder<T> : {{capture.BaseType!.ToDisplayString()}}.{{nested.Name}}MonadMethodBuilder<T>
        {
            public new static {{nested.Name}}MonadMethodBuilder<T> Create() =>
                new();

            public new void SetResult(T result) =>
                base.SetResult(result);

            public new {{nested.Name}}<T> Task
            {
                get => base.Task;
                protected set => base.Task = value;
            }

            public new void Start<TStateMachine>(ref TStateMachine stateMachine)
                where TStateMachine : IAsyncStateMachine =>
                base.Start(ref stateMachine);

            public new void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : {{nested.Name}}MonadAwaiter, INotifyCompletion
                where TStateMachine : IAsyncStateMachine =>
                base.AwaitOnCompleted(ref awaiter, ref stateMachine);

            public new void SetException(Exception exception) =>
                base.SetException(exception);

            [SecuritySafeCritical]
            public new void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : {{nested.Name}}MonadAwaiter, ICriticalNotifyCompletion
                where TStateMachine : IAsyncStateMachine =>
                base.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);

            public new void SetStateMachine(IAsyncStateMachine stateMachine) =>
                base.SetStateMachine(stateMachine);
        }
        """;
}