using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Monads.Generators;

[Generator]
public class MonadGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (x, _) => x is TypeDeclarationSyntax { BaseList.Types.Count: > 0 },
                (x, _) => (Node: (TypeDeclarationSyntax) x.Node, Symbol: x.SemanticModel.GetDeclaredSymbol(x.Node) as INamedTypeSymbol))
            .Where(x => x.Symbol is not null && x.Symbol.AllInterfaces.Any(y => y.OriginalDefinition.ToString() is "Monads.IMonad<T>"));

        context.RegisterSourceOutput(provider, (cx, x) => Generate(cx, x.Node, x.Symbol!));
    }

    private static void Generate(SourceProductionContext cx, TypeDeclarationSyntax node, INamedTypeSymbol symbol)
    {
        var definition = new StringBuilder();

        if (symbol.IsReadOnly)
            definition.Append("readonly ");

        definition.Append("partial ");

        if (symbol.IsRecord)
            definition.Append("record ");

        if (symbol.IsValueType)
            definition.Append("struct");
        else
            definition.Append("class");

        var source =
            // lang=C#
            $$"""
            using System.Reflection;
            using System.Runtime.CompilerServices;
            using System.Security;

            #nullable disable

            [AsyncMethodBuilder(typeof({{symbol.Name}}MonadMethodBuilder<>))]
            {{definition}} {{symbol.Name}}<T>
            {
                public {{symbol.Name}}MonadAwaiter<T> GetAwaiter() =>
                    new(this);
            }

            public abstract class {{symbol.Name}}MonadAwaiter : INotifyCompletion
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

                public abstract {{symbol.Name}}<TMap> Bind<TMap>(Func<object, {{symbol.Name}}<TMap>> map);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void OnCompleted(Action continuation) =>
                    throw new NotSupportedException("`OnCompleted` must not be called");
            }

            public class {{symbol.Name}}MonadAwaiter<T>({{symbol.Name}}<T> container) : {{symbol.Name}}MonadAwaiter
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public T GetResult() =>
                    (T) Result;

                public override {{symbol.Name}}<TMap> Bind<TMap>(Func<object, {{symbol.Name}}<TMap>> map) =>
                    container.Bind((T x) => map(x!));
            }

            public class {{symbol.Name}}MonadMethodBuilder<T>
            {
                public static {{symbol.Name}}MonadMethodBuilder<T> Create() =>
                    new();

                public void SetResult(T result) =>
                    Task = {{symbol.Name}}<T>.Return(result);

                public {{symbol.Name}}<T> Task { get; protected set; } = default!;

                public void Start<TStateMachine>(ref TStateMachine stateMachine)
                    where TStateMachine : IAsyncStateMachine =>
                    stateMachine.MoveNext();

                public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                    where TAwaiter : {{symbol.Name}}MonadAwaiter, INotifyCompletion
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
                    where TAwaiter : {{symbol.Name}}MonadAwaiter, ICriticalNotifyCompletion
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

        if (!symbol.ContainingNamespace.IsGlobalNamespace)
        {
            source =
                $$"""
                namespace {{symbol.ContainingNamespace.Name}};
                
                
                """
                + source;
        }

        cx.AddSource(symbol.Name, source);
    }
}