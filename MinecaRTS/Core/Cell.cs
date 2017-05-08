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
    public class Cell
    {
        private int _size;

        public int Size
        {
            get { return _size; }
        }

        /// <summary>
        /// The position of the cell.
        /// </summary>
        public Vector2 Pos { get; set; }       

        /// <summary>
        /// The color.
        /// </summary>
        public Color Color { get; set; }

        // TODO: This will need to be taken out of the data structure to allow facade to operate properly.
        // For path planning, each team will have a different view of what is passable.
        #region     /------ Pathfinding Details ------\
        public List<Cell> Neighbours;


        /// <summary>
        /// Whether or not the cell can be walked on.
        /// </summary>
        public bool Passable { get; set; }

        /// <summary>
        /// The parent of the cell in graph traversal.
        /// </summary>
        public Cell Parent;

        /// <summary>
        /// The score associated with this cell in the path being calculated.
        /// </summary>
        public float Score;
        #endregion  /------ Pathfinding Details ------\

        /// <summary>
        /// The world coordinate rectangle used for collisions.
        /// </summary>
        public Rectangle CollisionRect
        {
            get { return new Rectangle((int)Pos.X, (int)Pos.Y, _size, _size); }
        }

        /// <summary>
        /// The mid point of the collision rectangle.
        /// </summary>
        public Vector2 Mid
        {
            get {return new Vector2((int)Pos.X + _size / 2, (int)Pos.Y + _size / 2);}
        }

        /// <summary>
        /// The screen coordinate rectangle used for rendering.
        /// </summary>
        public Rectangle RenderRect
        {
            get { return new Rectangle((int)Camera.XToScreen(Pos.X), (int)Camera.YToScreen(Pos.Y), _size, _size); }
        }
        
        /// <summary>
        /// The mid point of the render rectangle.
        /// </summary>
        public Vector2 RenderMid
        {
            get { return RenderRect.Center.ToVector2(); }
        }

        /// <summary>
        /// Initialises cell values.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        public Cell(Vector2 pos, int size)
        {
            _size = size;
            Pos = pos;
            Color = Color.ForestGreen;
            Passable = true;
        }

        /// <summary>
        /// Renders a black rectangle around the cell border.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to render to.</param>
        public void Render(SpriteBatch spriteBatch)
        {
            //spriteBatch.FillRectangle(RenderRect, Color);
        }
    }
}
