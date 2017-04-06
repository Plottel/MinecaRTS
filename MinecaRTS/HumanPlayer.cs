using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MinecaRTS
{
    public class HumanPlayer
    {
        private PlayerData _data;
        private bool _boxing = false;
        private Point _boxStart = Point.Zero;
        private Rectangle _box = Rectangle.Empty;

        public HumanPlayer(PlayerData data)
        {
            _data = data;
        }

        public void HandleInput()
        {
            // Move camera with arrow keys.
            if (Input.KeyDown(Keys.Left))
                Camera.MoveBy(-5, 0);
            if (Input.KeyDown(Keys.Right))
                Camera.MoveBy(5, 0);
            if (Input.KeyDown(Keys.Up))
                Camera.MoveBy(0, -5);
            if (Input.KeyDown(Keys.Down))
                Camera.MoveBy(0, 5);
            
            if (Input.LeftMouseClicked())
            {
                if (_boxing)
                {
                    // TODO: Account for negative box width / height
                    _data.SelectUnitsInRect(Camera.RectToWorld(_box));
                    _boxing = false;
                    _box = Rectangle.Empty;
                }
            }

            if (Input.LeftMouseDown())
            {
                if (!_boxing)
                {
                    _boxing = true;
                    _box = new Rectangle(Input.MousePos.ToPoint(), Point.Zero);
                    _boxStart = Input.MousePos.ToPoint();
                }
                else
                {
                    _box.X = Math.Min(Input.MouseX, _boxStart.X);
                    _box.Y = Math.Min(Input.MouseY, _boxStart.Y);
                    _box.Width = Math.Abs(_boxStart.X - Input.MouseX);
                    _box.Height = Math.Abs(_boxStart.Y - Input.MouseY);
                }
            }

            if (Input.RightMousePressed())
                _data.MoveSelectedUnitsTo(Camera.VecToWorld(Input.MousePos));
        }

        public void Render(SpriteBatch spriteBatch)
        {
            if (_boxing)
                spriteBatch.DrawRectangle(_box, Color.SpringGreen, 2);
        }
    }
}
