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
    /// <summary>
    /// Represents a UI element containing a collection of buttons.
    /// Knows the locations of each of its buttons.
    /// </summary>
    public class Panel
    {
        public Vector2 Pos { get; set; }
        public Vector2 Scale { get; set; }
        private string _name;

        private List<Button> _buttons = new List<Button>();

        public Rectangle RenderRect
        {
            get
            {
                return new Rectangle(Pos.ToPoint(), Scale.ToPoint());
            }
        }

        public Panel(string name, Vector2 pos, Vector2 scale)
        {
            _name = name;
            Pos = pos;
            Scale = scale;
        }

        public void AddButton(Button button)
        {
            button.parentPanel = this;
            _buttons.Add(button);
        }

        public Button ButtonAtPos(Vector2 pos)
        {
            foreach (Button b in _buttons)
            {
                if (b.RenderRect.Contains(pos))
                    return b;
            }

            return null;
        }

        public void Render(SpriteBatch spriteBatch)
        {
            spriteBatch.FillRectangle(RenderRect, Color.DarkSlateGray);
            spriteBatch.DrawRectangle(RenderRect, Color.LightGoldenrodYellow, 3);

            foreach (Button b in _buttons)
                b.Render(spriteBatch);

            spriteBatch.DrawString(MinecaRTS.largeFont, _name, new Vector2(Pos.X + 4, Pos.Y + 4), Color.White);
        }
    }
}
