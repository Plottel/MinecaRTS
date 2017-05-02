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
        public static Texture2D houseTexture;

        private int _health = 0;
        private int _maxHealth = 0;

        public bool isActive;
        private Team _team;

        public Team Team
        {
            get { return _team; }
        }

        public Building(Vector2 pos, Vector2 scale, Team team, int maxHealth) : base(pos, scale)
        {
            _team = team;
            isActive = false;
            _maxHealth = maxHealth;
        }

        public void Construct()
        {
            if (!isActive)
            {
                if (++_health >= _maxHealth)
                {
                    isActive = true;
                    _health = _maxHealth;
                }
            }
        }

        public override void Update()
        {
            return;
        }

        public override void Render(SpriteBatch spriteBatch)
        {
        }

        public void RenderInactive(SpriteBatch spriteBatch)
        {
            spriteBatch.FillRectangle(RenderRect, Color.BlanchedAlmond);

            float rectWidth = Scale.X * ((float)_health / (float)_maxHealth);
            Rectangle rect = new Rectangle(RenderRect.X, RenderRect.Y - 10, (int)rectWidth, 8);

            spriteBatch.FillRectangle(rect, Color.MediumTurquoise);
        }

        public virtual void HandleInput(int index)
        { }

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
