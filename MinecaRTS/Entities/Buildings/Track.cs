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
        public static Texture2D ACTIVE_TEXTURE;
        public static Texture2D CONSTRUCTION_TEXTURE;

        public Track(Vector2 pos, Team team) : base (pos, new Vector2(31, 31), team, 50, ACTIVE_TEXTURE, CONSTRUCTION_TEXTURE)
        {
        }
    }
}
