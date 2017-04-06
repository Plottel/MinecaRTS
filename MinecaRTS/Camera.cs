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

        public static int X
        {
            get { return Pos.X; }

            // Clamp to within screen boundary.
            set
            {
                if (value < 0)
                    Pos.X = 0;
                else if (value > World.WIDTH - WIDTH)
                    Pos.X = World.WIDTH - WIDTH;
                else
                    Pos.X = value;
            }
        }

        public static int Y
        {
            get { return Pos.Y; }
            set
            {
                if (value < 0)
                    Pos.Y = 0;
                else if (value > World.HEIGHT - HEIGHT)
                    Pos.Y = World.HEIGHT - HEIGHT;
                else
                    Pos.Y = value;
            }
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

        // TODO: Implement LookAt() - centers camera on location.
        // TODO: Implement bool PosOnScreen()
    }
}
