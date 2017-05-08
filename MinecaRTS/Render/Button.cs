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
    public class Button
    {
        private string _name;
        private Vector2 _pos;
        private Vector2 _scale;
        public Panel parentPanel;

        public string Name
        {
            get { return _name; }
        }

        public Rectangle RenderRect
        {
            get
            {
                return new Rectangle((_pos + parentPanel.Pos).ToPoint(), _scale.ToPoint());
            }
        }

        public Button(string name, Vector2 pos, Vector2 scale, Panel panel)
        {
            _name = name;
            _pos = pos;
            _scale = scale;
            parentPanel = panel;
        }

        public void Render(SpriteBatch spriteBatch)
        {
            spriteBatch.FillRectangle(RenderRect, Color.Black);
            spriteBatch.DrawRectangle(RenderRect, Color.White, 1);
            spriteBatch.DrawString(MinecaRTS.smallFont, _name, new Vector2(RenderRect.X + 4, RenderRect.Y + 4), Color.White);
        }
    }
}
