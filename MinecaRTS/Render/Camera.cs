using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    public static class Camera
    {
        public static Point Pos;
        public static int WIDTH;
        public static int HEIGHT;

        public const int MINIMAP_SIZE = 300;

        public static int MINIMAP_X
        {
            get { return WIDTH - MINIMAP_SIZE - 100; }
        }

        public static int MINIMAP_Y
        {
            get { return HEIGHT - MINIMAP_SIZE - 100; }
        }

        public static int X
        {
            get { return Math.Max(Pos.X, 0); }

            // Clamp to within screen boundary.
            set
            {
                if (value < 0)
                    Pos.X = 0;
                else if (value > World.Width - WIDTH)
                    Pos.X = World.Width - WIDTH;
                else
                    Pos.X = value;
            }
        }

        public static int Y
        {
            get { return Math.Max(Pos.Y, 0); }
            set
            {
                if (value < 0)
                    Pos.Y = 0;
                else if (value > World.Height - HEIGHT)
                    Pos.Y = World.Height - HEIGHT;
                else
                    Pos.Y = value;
            }
        }

        public static Rectangle Rect
        {
            get { return new Rectangle(X, Y, WIDTH, HEIGHT); }
        }

        public static void MoveBy(int x, int y)
        {
            X += x;
            Y += y;
        }

        public static void MoveBy(Point pt)
        {
            X += pt.X;
            Y += pt.Y;
        }

        public static void MoveTo(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static void MoveTo(Point pt)
        {
            X = pt.X;
            Y = pt.Y;
        }

        public static Vector2 VecToWorld(Vector2 v)
        {
            return new Vector2(v.X + X, v.Y + Y);
        }

        public static Point PtToWorld(Point p)
        {
            return new Point(p.X + X, p.Y + Y);
        }

        public static float XToWorld(float x)
        {
            return x + X;
        }

        public static float YToWorld(float y)
        {
            return y + Y;
        }

        public static Vector2 VecToScreen(Vector2 v)
        {
            return new Vector2(v.X - X, v.Y - Y);
        }

        public static Point PtToScreen(Point p)
        {
            return new Point(p.X - X, p.Y - Y);
        }

        public static float XToScreen(float x)
        {
            return x - X;
        }

        public static float YToScreen(float y)
        {
            return y - Y;
        }

        public static Rectangle RectToWorld(Rectangle rect)
        {
            return new Rectangle(rect.X + X, rect.Y + Y, rect.Width, rect.Height);
        }

        public static Rectangle RectToScreen(Rectangle rect)
        {
            return new Rectangle(rect.X - X, rect.Y - Y, rect.Width, rect.Height);
        }

        public static Rectangle WorldRectToMinimapRect(Rectangle rect)
        {
            float xRatio = 1 + (World.Width / MINIMAP_SIZE);
            float yRatio = 1 + (World.Height / MINIMAP_SIZE);

            return new Rectangle(MINIMAP_X + (int)Math.Floor(rect.X / xRatio),
                    MINIMAP_Y + (int)Math.Floor(rect.Y / yRatio),
                    (int)Math.Ceiling(rect.Width / xRatio),
                    (int)Math.Ceiling(rect.Height / yRatio));
        }
    }

        // TODO: Implement LookAt() - centers camera on location.
        // TODO: Implement bool PosOnScreen()
}
