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
    public class Resource : Entity
    {
        public ResourceType Type;
        private Color _color;
        private int _value = 100;
        private int _harvesters = 0;

        public int Harvesters
        {
            get { return _harvesters; }
        }

        public Resource(Vector2 pos, Vector2 scale, ResourceType t) : base(pos, scale)
        {
            Type = t;

            if (Type == ResourceType.Wood)
                _color = new Color(102, 51, 0);
            else if (Type == ResourceType.Stone)
                _color = new Color(64, 64, 64);
        }

        public void Detach()
        {
            --_harvesters;
        }

        public void Attach()
        {
            ++_harvesters;
        }

        public override void Update()
        {
            return;
        }

        public override void Render(SpriteBatch spriteBatch)
        {
            spriteBatch.FillRectangle(RenderRect.GetInflated(-4, -4), _color);
        }

        public override void HandleMessage()
        {
            return;
        }

        public override void RenderDebug(SpriteBatch spriteBatch)
        {
            return;
        }
    }
}
