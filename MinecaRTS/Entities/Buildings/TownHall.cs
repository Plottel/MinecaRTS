using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MinecaRTS
{
    public class TownHall : ProductionBuilding, ICanAcceptResources
    {
        public static Texture2D ACTIVE_TEXTURE;
        public static Texture2D CONSTRUCTION_TEXTURE;

        public TownHall(Vector2 pos, Vector2 scale, List<Type> productionTypes, PlayerData data) : base (pos, scale, productionTypes, data, ACTIVE_TEXTURE, CONSTRUCTION_TEXTURE)
        { }

        public void AcceptResources(uint woodAmount, uint stoneAmount)
        {
            _data.GiveResources(woodAmount, ResourceType.Wood);
            _data.GiveResources(stoneAmount, ResourceType.Stone);
        }
    }
}
