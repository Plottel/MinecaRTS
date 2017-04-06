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
        private State<T> _prevState;
        // TODO: Add global state if needed.

        public StateMachine(T owner)
        {
            _owner = owner;
            _currentState = null;
            _prevState = null;
        }

        public void Update()
        {
            if (_currentState != null)
                _currentState.Execute(_owner);
        }

        public ChangeState(State<T>)
        {

        }

    }
}
