//HintName: Configuration.cs
using System.Runtime.CompilerServices;
using System.Security;

#nullable disable

public partial class Configuration
{
    public new class ReaderMonadMethodBuilder<T> : Environment<Configuration>.ReaderMonadMethodBuilder<T>
    {
        public new static ReaderMonadMethodBuilder<T> Create() =>
            new();
    
        public new void SetResult(T result) =>
            base.SetResult(result);
    
        public new Reader<T> Task
        {
            get => base.Task;
            protected set => base.Task = value;
        }
    
        public new void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine =>
            base.Start(ref stateMachine);
    
        public new void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ReaderMonadAwaiter, INotifyCompletion
            where TStateMachine : IAsyncStateMachine =>
            base.AwaitOnCompleted(ref awaiter, ref stateMachine);
    
        public new void SetException(Exception exception) =>
            base.SetException(exception);
    
        [SecuritySafeCritical]
        public new void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ReaderMonadAwaiter, ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine =>
            base.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
    
        public new void SetStateMachine(IAsyncStateMachine stateMachine) =>
            base.SetStateMachine(stateMachine);
    }
}