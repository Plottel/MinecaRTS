using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    public abstract class Goal<T1, T2> where T2 : Bot
    {
        protected T1 owner;
        protected T2 bot;

        private GoalState _state;

        public GoalState State
        {
            get { return _state; }
            protected set { _state = value; }
        }

        protected Goal(T1 owner, T2 bot)
        {
            this.owner = owner;
            this.bot = bot;
        }

        public abstract void Activate();
        public abstract GoalState Process();
        public virtual void Terminate() { }
        public virtual void AddSubgoal(Goal<T1, T2> goal) { }

        public string ToString(int depth)
        {
            string result = "";

            for (int i = 0; i < depth; i++)
                result += "\t";

            result += this.GetType().Name;

            return result;
        }
    }
}
