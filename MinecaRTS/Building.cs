﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MinecaRTS
{
    public class Building : Entity
    {
        public static Texture2D townHallTexture;

        public Building(Vector2 pos, Vector2 scale) : base(pos, scale)
        {

        }

        public override void Update()
        {
            return;
        }

        public override void Render(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(townHallTexture, Camera.VecToScreen(Pos), Color.White);
        }

        public override void HandleMessage(Message message)
        {
            return;
        }

        public override void ExitState()
        { }

        public override void RenderDebug(SpriteBatch spriteBatch)
        {
            return;
        }
    }
}
