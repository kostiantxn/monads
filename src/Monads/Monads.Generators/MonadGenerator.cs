using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Monads.Generators.Constants;
using Monads.Generators.Extensions;

namespace Monads.Generators;

/// <summary>
///     An incremental source generator that collects all types that implement
///     the <c>IMonad&lt;T&gt;</c> interface to generate a custom
///     <c>AsyncMethodBuilder</c> type and the <c>GetAwaiter</c> method for them.
/// </summary>
[Generator]
public class MonadGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (x, _) => x is TypeDeclarationSyntax { BaseList.Types.Count: > 0 },
                (x, _) => x.SemanticModel.GetDeclaredSymbol(x.Node) as INamedTypeSymbol)
            .Where(x => x?.TypeParameters.Length is 1 && x.OriginalDefinition.Implements(Types.IMonad))
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
        var contents = new StringBuilder();

        contents
            .AppendLine( 
                // lang=C#
                $$"""
                [AsyncMethodBuilder(typeof({{type.Name}}MonadMethodBuilder<>))]
                {{type.Definition()}}
                {
                    public {{type.Name}}MonadAwaiter<{{type.TypeParameters[0].Name}}> GetAwaiter() =>
                        new(this);
                }

                """)
            .AppendLine(MonadAwaiter(type))
            .AppendLine()
            .Append(MonadMethodBuilder(type));

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
                using System.Reflection;
                using System.Runtime.CompilerServices;
                using System.Security;

                #nullable disable

                """)
            .AppendContainingType(type.ContainingType, contents.ToString());

        cx.AddSource(type.Name, source.ToString());
    }

    private static string MonadAwaiter(INamedTypeSymbol type)
    {
        return
            // lang=C#
            $$"""
            public abstract class {{type.Name}}MonadAwaiter : INotifyCompletion
            {
                private bool _set;
                private object _value;

                public object Result
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _set ? _value : throw new InvalidOperationException("Value has not been set");

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    set
                    {
                        _value = value;
                        _set = true;
                    }
                }

                public bool IsCompleted
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => false;
                }

                public abstract {{type.Name}}<U> Bind<U>(Func<object, {{type.Name}}<U>> map);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void OnCompleted(Action continuation) =>
                    throw new NotSupportedException("`OnCompleted` must not be called");
            }

            public class {{type.Name}}MonadAwaiter<T>({{type.Name}}<T> container) : {{type.Name}}MonadAwaiter
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public T GetResult() =>
                    (T) Result;

                public override {{type.Name}}<U> Bind<U>(Func<object, {{type.Name}}<U>> map) =>
                    container.Bind((T x) => map(x!));
            }
            """;
    }

    private static string MonadMethodBuilder(INamedTypeSymbol type)
    {
        return
            // lang=C#
            $$"""
            public class {{type.Name}}MonadMethodBuilder<T>
            {
                public static {{type.Name}}MonadMethodBuilder<T> Create() =>
                    new();

                public void SetResult(T result) =>
                    Task = {{type.Name}}<T>.Return(result);

                public {{type.Name}}<T> Task { get; protected set; } = default!;

                public void Start<TStateMachine>(ref TStateMachine stateMachine)
                    where TStateMachine : IAsyncStateMachine =>
                    stateMachine.MoveNext();

                public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                    where TAwaiter : {{type.Name}}MonadAwaiter, INotifyCompletion
                    where TStateMachine : IAsyncStateMachine
                {
                    var capturedAwaiter = awaiter;
                    var capturedStateMachine = stateMachine;

                    Task = awaiter.Bind(x =>
                    {
                        // Create a shallow copy of the state machine and run it from
                        // the current state.
                        var clonedStateMachine = Clone<TStateMachine>.Func(capturedStateMachine);

                        // Update the `MonadAwaiter` to return the value from the monad
                        // container, so that when the state machine invokes `GetResult`,
                        // the set value will be returned and used in the next state
                        // machine step.
                        capturedAwaiter.Result = x;

                        // Run the cloned state machine.
                        clonedStateMachine.MoveNext();

                        // Return the final result that was computed by the cloned state
                        // machine (since the state machine was only shallow copied, it
                        // reuses the same `MonadMethodBuilder` instance, so its result
                        // will be stored in the `this.Task` property).
                        return Task;
                    });
                }

                #region Unsupported

                public void SetException(Exception exception) =>
                    throw new NotSupportedException("`SetException` is not supported", exception);

                [SecuritySafeCritical]
                public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                    where TAwaiter : {{type.Name}}MonadAwaiter, ICriticalNotifyCompletion
                    where TStateMachine : IAsyncStateMachine =>
                    throw new NotSupportedException("`AwaitUnsafeOnCompleted` is not supported");

                public void SetStateMachine(IAsyncStateMachine stateMachine)
                {
                }

                #endregion

                private static class Clone<TStateMachine>
                    where TStateMachine : IAsyncStateMachine
                {
                    public static readonly Func<TStateMachine, TStateMachine> Func;

                    static Clone()
                    {
                        // C# compiler generates a `struct` state machine for `Release`,
                        // and a `class` for `Debug` build configuration.
                        if (typeof(TStateMachine).IsValueType)
                        {
                            Func = x => x;
                        }
                        else
                        {
                            var method =
                                typeof(TStateMachine)
                                    .GetMethod(nameof(MemberwiseClone), BindingFlags.Instance | BindingFlags.NonPublic) 
                                ?? throw new InvalidOperationException("Couldn't find the `MemberwiseClone` method");

                            var @delegate =
                                (Func<object, object>) Delegate.CreateDelegate(typeof(Func<object, object>), method);

                            Func = x => (TStateMachine) @delegate(x);
                        }
                    }
                }
            }
            """;
    }
}