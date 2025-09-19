using System;
using System.Collections.Generic;

namespace Tech.StateMachine
{
    public class StateMachine<StateID, BState> where StateID : Enum where BState : StateBase
    {
       public readonly Dictionary<StateID, BState> _states = new();
        public StateID CurrentStateID { get; private set; }
        public BState CurrentState { get; private set; }

        public void AddNewState(StateID state, BState newState)
        {
            _states.Add(state, newState);
        }

        public virtual void Initialize(StateID startState)
        {
            CurrentState = _states[startState];
            CurrentStateID = startState;
            CurrentState.Enter();
        }

        public virtual void ChangeState(StateID newStateID)
        {
            var newState = _states[newStateID];
            if (CurrentState == newState) return;
            CurrentState.Exit();
            CurrentStateID = newStateID;
            CurrentState = newState;
            CurrentState.Enter();
        }
    }

}

