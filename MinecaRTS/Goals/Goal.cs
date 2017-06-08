using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    public abstract class Goal<T>
    {
        protected T owner;

        private GoalState _state;

        public GoalState State
        {
            get { return _state; }
            protected set { _state = value; }
        }

        protected Goal(T owner)
        {
            this.owner = owner;
        }

        public abstract void Activate();
        public abstract GoalState Process();
        public abstract void Terminate();
        public abstract void AddSubgoal(Goal<T> goal);
    }
}
