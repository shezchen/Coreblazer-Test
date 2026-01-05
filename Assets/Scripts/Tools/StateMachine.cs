using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Tools
{
    public class StateMachine<TState> where TState : notnull
    {
        private sealed class StateNode
        {
            public readonly Action OnEnter;
            public readonly Action OnExit;
            public readonly Func<UniTask> OnEnterAsync;
            public readonly Func<UniTask> OnExitAsync;

            public StateNode(Action onEnter, Action onExit, Func<UniTask> onEnterAsync, Func<UniTask> onExitAsync)
            {
                OnEnter = onEnter;
                OnExit = onExit;
                OnEnterAsync = onEnterAsync ?? (() => UniTask.CompletedTask);
                OnExitAsync = onExitAsync ?? (() => UniTask.CompletedTask);
            }
        }

        private readonly Dictionary<TState, StateNode> states = new();

        public TState CurrentState { get; private set; }
        public bool InCurrentState { get; private set; }
        public bool IsTransitioning { get; private set; }

        public event Action<TState, TState> OnStateChanging;
        public event Action<TState, TState> OnStateChanged;
        public event Action<TState> OnStateEntered;
        public event Action<TState> OnStateExited;

        public StateMachine(TState initialState = default)
        {
            if (initialState is not null)
            {
                CurrentState = initialState;
                InCurrentState = false;
            }
        }

        public void Register(TState state,
            Action onEnter = null,
            Action onExit = null,
            Func<UniTask> onEnterAsync = null,
            Func<UniTask> onExitAsync = null)
        {
            states[state] = new StateNode(onEnter, onExit, onEnterAsync, onExitAsync);
        }

        public bool Unregister(TState state)
        {
            if (InCurrentState && EqualityComparer<TState>.Default.Equals(state, CurrentState))
                throw new InvalidOperationException("Cannot unregister the current active state. Change to another state first.");
            return states.Remove(state);
        }

        public bool Contains(TState state) => states.ContainsKey(state);

        public async UniTask<bool> ChangeStateAsync(TState newState)
        {
            if (InCurrentState && EqualityComparer<TState>.Default.Equals(newState, CurrentState))
                return false;
            if (!states.ContainsKey(newState))
                throw new KeyNotFoundException($"State '{newState}' not registered.");
            if (IsTransitioning)
            {
                UnityEngine.Debug.LogError($"StateMachine is already transitioning from '{CurrentState}' when trying to change to '{newState}'. Re-entrant state changes are not allowed.");
                return false;
            }
            await DoChangeStateAsync(newState);
            return true;
        }

        private async UniTask DoChangeStateAsync(TState newState)
        {
            IsTransitioning = true;
            try
            {
                var previous = CurrentState;
                var prevNode = InCurrentState && states.TryGetValue(previous, out var pn) ? pn : null;
                var nextNode = states[newState];
                if (!InCurrentState)
                {
                    CurrentState = newState;
                    InCurrentState = true;
                    OnStateChanging?.Invoke(default, newState);
                    nextNode.OnEnter?.Invoke();
                    await nextNode.OnEnterAsync();
                    OnStateEntered?.Invoke(newState);
                    OnStateChanged?.Invoke(default, newState);
                }
                else
                {
                    OnStateChanging?.Invoke(previous, newState);
                    prevNode?.OnExit?.Invoke();
                    if (prevNode != null) await prevNode.OnExitAsync();
                    OnStateExited?.Invoke(previous);
                    CurrentState = newState;
                    nextNode.OnEnter?.Invoke();
                    await nextNode.OnEnterAsync();
                    OnStateEntered?.Invoke(newState);
                    OnStateChanged?.Invoke(previous, newState);
                }
            }
            finally
            {
                IsTransitioning = false;
            }
        }
    }
}