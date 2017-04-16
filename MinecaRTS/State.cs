using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    public abstract class State<T>
    {
        public abstract void Enter(T owner);
        public abstract void Exit(T owner);
        public abstract void Execute(T owner);
        public abstract void HandleMessage(T owner, Message message);
    }
}