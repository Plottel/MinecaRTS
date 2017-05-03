using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    using Registry = Dictionary<ulong, Entity>;

    public static class EntityRegistry
    {
        private static Registry registry = new Registry();

        public static void RegisterEntity(Entity entity)
        {
            registry.Add(entity.ID, entity);
        }

        public static void RemoveEntity(Entity entity)
        {
            registry.Remove(entity.ID);
        }

        public static Entity GetEntityFromID(ulong id)
        {
            return registry[id];
        }
    }
}
