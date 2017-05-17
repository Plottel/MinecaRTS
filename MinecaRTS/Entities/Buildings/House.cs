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
    public class House : Building, IBoostsSupply
    {
        public const int MAX_HEALTH = 100;

        public static Texture2D ACTIVE_TEXTURE;
        public static Texture2D CONSTRUCTION_TEXTURE;

        public uint SupplyBoostAmount
        {
            get { return 5; }
        }

        public House(Vector2 pos, Team team) : base(pos, new Vector2(63, 63), team, MAX_HEALTH, ACTIVE_TEXTURE, CONSTRUCTION_TEXTURE)
        {
        }
    }
}
