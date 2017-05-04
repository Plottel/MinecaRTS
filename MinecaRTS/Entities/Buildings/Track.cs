using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MinecaRTS
{
    public class Track : Building
    {
        public const int MAX_HEALTH = 50;

        public static Texture2D ACTIVE_TEXTURE;
        public static Texture2D CONSTRUCTION_TEXTURE;

        public Track(Vector2 pos, Team team) : base (pos, new Vector2(31, 31), team, MAX_HEALTH, ACTIVE_TEXTURE, CONSTRUCTION_TEXTURE)
        {
        }
    }
}
