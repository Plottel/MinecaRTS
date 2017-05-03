using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    using Registry = Dictionary<ulong, IHandleMessages>;

    public static class MsgHandlerRegistry
    {
        private static Registry registry = new Registry();

        /// <summary>
        /// The shared unique id counter. Given and incremented on IHandleMessages object creation.
        /// </summary>
        private static ulong _nextUniqueID = 0;

        public static ulong NextID
        {
            get { return _nextUniqueID++; }
        }

        public static void Register(IHandleMessages msgHandler)
        {
            registry.Add(msgHandler.ID, msgHandler);
        }

        public static void RemoveEntity(IHandleMessages msgHandler)
        {
            registry.Remove(msgHandler.ID);
        }

        public static IHandleMessages GetMsgHandlerFromID(ulong id)
        {
            return registry[id];
        }
    }
}
