using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    public class TownHall : ProductionBuilding, ICanAcceptResources
    {
        public TownHall(Vector2 pos, Vector2 scale, List<Type> productionTypes, PlayerData data) : base (pos, scale, productionTypes, data)
        { }

        public void AcceptResources(uint woodAmount, uint stoneAmount)
        {
            _data.GiveResources(woodAmount, ResourceType.Wood);
            _data.GiveResources(stoneAmount, ResourceType.Stone);
        }
    }
}
