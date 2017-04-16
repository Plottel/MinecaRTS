using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    public class StateMachine<T>
    {
        private T _owner;
        private State<T> _currentState;
        // TODO: Add previous state if needed.
        // TODO: Add global state if needed.

        public State<T> CurrentState
        {
            get { return _currentState; }
        }

        public StateMachine(T owner)
        {
            _owner = owner;
            _currentState = null;
        }

        public void Execute()
        {
            if (_currentState != null)
                _currentState.Execute(_owner);
        }

        public void ChangeState(State<T> newState)
        {
            if (_currentState != null)
                _currentState.Exit(_owner);

            _currentState = newState;

            if (_currentState != null)
                _currentState.Enter(_owner);
        }

        public void HandleMessage(Message message)
        {
            if (_currentState != null)
                _currentState.HandleMessage(_owner, message);
            else
                throw new Exception("Message Type: " + message.msg.ToString() + " could not be handled.");
        }
    }
}
