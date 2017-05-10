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
    // TODO: Add overloads for X/Y rather than just Point / Vector.
    // creating a bunch of unnecessary Vector / Point objects.
    public class Grid
    {
        private int _cellSize;

        public int CellSize
        {
            get { return _cellSize; }
        }
        /// <summary>
        /// The cells in the grid.
        /// Outer list represents columns.
        /// Inner list represents rows.
        /// </summary>
        private List<List<Cell>> _cells = new List<List<Cell>>();

        /// <summary>
        /// The number of columns in the grid.
        /// </summary>
        private int _cols = 0;

        /// <summary>
        /// The number of rows in the grid.
        /// </summary>
        private int _rows = 0;

        /// <summary>
        /// The position of the grid in world coordinates.
        /// </summary>
        public Vector2 Pos { get; set; }

        /// <summary>
        /// Gets or sets the Cell at the given index.
        /// </summary>
        /// <param name="col">The column index.</param>
        /// <param name="row">The row index.</param>
        /// <returns></returns>
        public Cell this[int col, int row]
        {
            get
            {
                if (col < 0 || col > Cols - 1 || row < 0 || row > Rows - 1)
                    return null;
                else
                    return _cells[col][row];
            }

            set { _cells[col][row] = value; }
        }

        public Cell this[Point cellIndex]
        {
            get { return this[cellIndex.Col(), cellIndex.Row()]; }
            set { this[cellIndex.Col(), cellIndex.Row()] = value; }
        }

        /// <summary>
        /// Gets the number of columns in the grid.
        /// </summary>
        public int Cols
        {
            get {return _cols;}
        }

        /// <summary>
        /// Gets the number of rows in the grid.
        /// </summary>
        public int Rows
        {
            get {return _rows;}
        }

        /// <summary>
        /// The width of the grid.
        /// -1 so the absolute edge doesn't register next cell and out of bounds.
        /// </summary>
        public int Width
        {
            get { return Cols * _cellSize - 1;}
        }

        /// <summary>
        /// The height of the grid.
        /// -1 so the absolute edge doesn't register next cell and out of bounds.
        /// </summary>
        public int Height
        {
            get { return Rows * _cellSize - 1; }
        }

        /// <summary>
        /// The bounding rectangle of the grid in world coordinates.
        /// </summary>
        public Rectangle CollisionRect
        {
            get { return new Rectangle((int)Pos.X, (int)Pos.Y, Width, Height); }
        }

        /// <summary>
        /// The bounding rectangle of the grid in screen coordinates.
        /// </summary>
        public Rectangle RenderRect
        {
            get { return new Rectangle((int)Pos.X - Camera.X, (int)Pos.Y - Camera.Y, Width, Height); }
        }

        /// <summary>
        /// Specifies if a rectangle should be drawn around each cell.
        /// </summary>
        public bool ShowGrid { get; set; }

        /// <summary>
        /// Creates a new grid.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="cols"></param>
        /// <param name="rows"></param>
        public Grid(Vector2 pos, int cols, int rows, int cellSize)
        {
            Pos = pos;

            _cellSize = cellSize;

            AddColumns(cols);
            AddRows(rows);
            ShowGrid = false;

            SetupCellNeighbours();
        }

        private void SetupCellNeighbours()
        {
            for (int col = 0; col < Cols; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    var neighbours = new List<Cell>();

                    // Diagonal
                    //foreach (Point cellIndex in Get33GridIndexesAroundIndex(new Point(col, row)))
                    //{
                      //  AddCell(this[cellIndex], neighbours);
                    //}

                    // Orthogonal
                    AddCell(this[col - 1, row], neighbours); // West
                    AddCell(this[col, row + 1], neighbours); // South
                    AddCell(this[col + 1, row], neighbours); // East
                    AddCell(this[col, row - 1], neighbours); // North

                    this[col, row].Neighbours = neighbours;
                }
            }
        }

        /// <summary>
        /// Null checks the passed in cell and adds it to the list if it is not null.
        /// </summary>
        public void AddCell(Cell cell, ICollection<Cell> list)
        {
            if (cell != null)
                list.Add(cell);
        }

        /// <summary>
        /// Returns the cell at the given position.
        /// </summary>
        /// <param name="pos">The position to check.</param>
        public Cell CellAt(Vector2 pos)
        {
            int col = (int)Math.Floor((pos.X - Pos.X) / _cellSize);
            int row = (int)Math.Floor((pos.Y - Pos.Y) / _cellSize);

            return this[col, row];
        }

        /// <summary>
        /// Returns the cell at the mouse position.
        /// </summary>
        public Cell CellAtMousePos()
        {
            return CellAt(Camera.VecToWorld(Input.MousePos));
        }

        /// <summary>
        /// Returns the index of the cell at the given position.
        /// Returned as a Point where X = Col and Y = Row.
        /// </summary>
        /// <param name="pos">The position to check</param>
        public Point IndexAt(Vector2 pos)
        {
            var col = (int)Math.Floor((pos.X - Pos.X) / _cellSize);
            var row = (int)Math.Floor((pos.Y - Pos.Y) / _cellSize);

            return new Point(col, row);
        }

        public List<Point> Get33GridIndexesAroundIndex(Point index)
        {
            var result = new List<Point>();

            for (int col = index.Col() - 1; col <= index.Col() + 1; col++)
            {
                for (int row = index.Row() - 1; row <= index.Row() + 1; row++)
                {
                    if (col < 0 || col >= _cols || row < 0 || row >= _rows)
                        continue;

                    result.Add(new Point(col, row));
                }
            }

            return result;
        }

        public List<Point> Get33GridIndexesAroundPos(Vector2 pos)
        {
            return Get33GridIndexesAroundIndex(IndexAt(pos));
        }

        /// <summary>
        /// Specifies if every cell the passed in rectangle touches is traversable.
        /// If the rect touches 1 or more unpassable cells, this returns false.
        /// </summary>
        public bool RectIsClear(Rectangle rect)
        {
            foreach (Cell c in CellsInRect(rect))
            {
                if (!c.Passable)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a list of cells the passed in Rectangle touches.
        /// </summary>
        public List<Cell> CellsInRect(Rectangle rect)
        {
            List<Cell> result = new List<Cell>();

            Point min = IndexAt(new Vector2(rect.Left, rect.Top));
            Point max = IndexAt(new Vector2(rect.Right, rect.Bottom));

            for (int col = min.Col(); col <= max.Col(); col++)
            {
                for (int row = min.Row(); row <= max.Row(); row++)
                {
                    AddCell(this[col, row], result);
                }
            }

            return result;
        }

        /// <summary>
        /// Projects a rectangle from two lines and returns all cells the rectangle is touching.
        /// </summary>
        public HashSet<Cell> CellsInRectFromLines(Point from1, Point to1, Point from2, Point to2)
        {
            // TODO: Currently just gets cells touching both lines - need to generate rectangle to join areas between lines.
            var result = CellsOnLine(from1.ToVector2(), to1.ToVector2());
            result.UnionWith(CellsOnLine(from2.ToVector2(), to2.ToVector2()));

            return result;
        }

        public HashSet<Cell> CellsOnLine(Vector2 v1, Vector2 v2)
        {
            var result = new HashSet<Cell>();

            bool isSteep = Math.Abs(v2.Y - v1.Y) > Math.Abs(v2.X - v1.X);

            if (isSteep)
            {
                Utils.Swap(ref v1.X, ref v1.Y);
                Utils.Swap(ref v2.X, ref v2.Y);
            }

            if (v1.X > v2.X)
            {
                Utils.Swap(ref v1.X, ref v2.X);
                Utils.Swap(ref v1.Y, ref v2.Y);
            }

            float dx = v2.X - v1.X;
            float dy = Math.Abs(v2.Y - v1.Y);

            float error = dx / 2.0f;
            int yStep = (v1.Y < v2.Y) ? 1 : -1;
            int y = (int)v1.Y;

            int maxX = (int)v2.X;

            for (int x = (int)v1.X; x < maxX; x++)
            {
                if (isSteep)
                    AddCell(CellAt(new Vector2(y, x)), result);
                else
                    AddCell(CellAt(new Vector2(x, y)), result);

                error -= dy;

                if (error < 0)
                {
                    y += yStep;
                    error += dx;
                }
            }

            return result;
        }

        /// <summary>
        /// Adds columns to the grid.
        /// Columns are the outer list, so adding a column creates a new list and populates it.
        /// </summary>
        /// <param name="amountToAdd">The number of columns to add.</param>
        public void AddColumns(int amountToAdd)
        {
            // For each column to be added.
            for (int col = 0; col < amountToAdd; col++)
            {
                var newCol = new List<Cell>();

                // For each row in the newly created column.
                for (int row = 0; row < Rows; row++)
                {
                    // Add the new cell.
                    newCol.Add(new Cell(new Vector2(Pos.X + Cols * _cellSize,
                                                    Pos.Y + row * _cellSize), 
                                                    _cellSize));
                }

                // Add the column to cells, increment number of columns in the grid.
                _cells.Add(newCol);
                _cols++;
            }
        }

        /// <summary>
        /// Adds rows to the grid.
        /// Rows are the inner list, so adding a row appends a new cell to the end of each list.
        /// </summary>
        /// <param name="amountToAdd">The number of rows to add.</param>
        public void AddRows(int amountToAdd)
        {
            // For each row to be added.
            for (int row = 0; row < amountToAdd; row++)
            {
                // For each column to have a new row added to it.
                for (int col = 0; col < Cols; col++)
                {
                    // Add the new cell to the column.
                    _cells[col].Add(new Cell(new Vector2(Pos.X + col * _cellSize, 
                                                         Pos.Y + Rows * _cellSize), 
                                                         _cellSize));
                }

                // Increment the number of rows in the grid.
                _rows++;
            }
        }

        /// <summary>
        /// Makes walls around the edge of the grid - for easy initialisation.
        /// </summary>
        public void MakeBorder()
        {
            for (int col = 0; col < Cols; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    if (col == 0 || col == Cols - 1 || row == 0 || Rows == Rows - 1)
                    {
                        _cells[col][row].Passable = false;
                        _cells[col][row].Color = Color.Black;
                    }
                }
            }
        }

        /// <summary>
        /// Renders the grid.
        /// Calls render on each Cell and draws a rectangle around it if ShowGrid is turned on.
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Render(SpriteBatch spriteBatch)
        {
            foreach (List<Cell> col in _cells)
            {
                foreach (Cell cell in col)
                {
                    cell.Render(spriteBatch);

                    // TODO: Separate loop, should only need to check ShowGrid once.
                    if (ShowGrid)
                        spriteBatch.DrawRectangle(cell.RenderRect, Color.Black, 1);
                }
            }          
        }
    }

}
