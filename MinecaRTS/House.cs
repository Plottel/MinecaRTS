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

        public uint BoostAmount
        {
            get { return 10; }
        }
        public House(Vector2 pos, Team team) : base(pos, new Vector2(63, 63), team, MAX_HEALTH)
        {
        }

        public override void Render(SpriteBatch spriteBatch)
        {
            if (!isActive)
            {
                RenderInactive(spriteBatch);
            }
            else
            {
                base.Render(spriteBatch);
                spriteBatch.Draw(houseTexture, RenderRect, Color.White);
            }            
        }
    }
}
