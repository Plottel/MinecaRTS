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
    public class Minecart : Unit
    {
        public Minecart(PlayerData data, Team team, Vector2 pos, Vector2 scale) : base(data, team, pos, scale)
        {
            Speed = 2;
        }

        public override void Update()
        {
            if (Data.CellHasTrack(Data.Grid.CellAt(Mid)))
            {
                Speed = 10;
                Steering.separationOn = false;
            }
            else
            {
                Speed = 2;
                Steering.separationOn = true;
            }

            base.Update();
        }

        public override void MoveTowards(Vector2 pos)
        {
            Cell cellAtPos = Data.Grid.CellAt(pos);

            if (cellAtPos.Passable)
            {
                pathHandler.GetPathToPosFollowingTracks(pos);
                ExitState(); // Change to "Neutral state"
            }
        }

        public override void Render(SpriteBatch spriteBatch)
        {
            spriteBatch.FillRectangle(RenderRect, Color.Black);
        }
    }
}
