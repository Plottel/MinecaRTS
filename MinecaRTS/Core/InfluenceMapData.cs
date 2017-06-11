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
    // To simplify reasoning and save the hands!
    using InfluenceMap = List<List<float>>;

    /// <summary>
    /// The class used to store and calculate influence.
    /// Currently just keeps track of buildings and influence refers purely to existence of buildings.
    /// </summary>
    public class InfluenceMapData
    {
        /// <summary>
        /// The grid used to map values to an area.
        /// </summary>
        private Grid _grid;

        /// <summary>
        /// The influence values.
        /// </summary>
        private InfluenceMap _influence;

        /// <summary>
        /// The border formed from all non-zero influence values.
        /// </summary>
        private List<Cell> _influenceBorder;

        /// <summary>
        /// Gets or sets the influence value at the given column and row index.
        /// </summary>
        /// <param name="col">Column index</param>
        /// <param name="row">Row index</param>
        public float this[int col, int row]
        {
            get
            {
                if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                    return 0; // Return empty list if out of range
                return _influence[col][row];
            }

            private set
            {
                if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                    return;
                _influence[col][row] = value;
            }
        }

        /// <summary>
        /// Overload for Point instead of col / row
        /// </summary>
        public float this[Point cellIndex]
        {
            get { return this[cellIndex.Col(), cellIndex.Row()]; }
            private set { this[cellIndex.Col(), cellIndex.Row()] = value; }
        }

        /// <summary>
        /// Gets the list of cells forming the border from all non-zero influence values.
        /// </summary>
        public List<Cell> InfluenceBorder
        {
            get { return _influenceBorder; }
        }

        /// <summary>
        /// Initializes a new InfluenceMapData
        /// </summary>
        /// <param name="grid">The grid to convert values to space.</param>
        public InfluenceMapData(Grid grid)
        {
            _grid = grid;
            _influence = new List<List<float>>();
            _influenceBorder = new List<Cell>();

            // Initialise influence values to zero.
            for (int col = 0; col < grid.Cols; col++)
            {
                _influence.Add(new List<float>());
                for (int row = 0; row < grid.Rows; row++)
                {
                    _influence[col].Add(0);
                }
            }
        }

        /// <summary>
        /// Returns the indexes at the border of the passed in rectangle
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        public List<Point> GetBorderIndexes(Rectangle rect)
        {
            var inflatedCells = _grid.IndexesInRect(rect.GetInflated(20, 20));
            var currentCells = _grid.IndexesInRect(rect);

            return inflatedCells.Where(element => !currentCells.Contains(element)).ToList();
        }

        /// <summary>
        /// Called whenever a new influencer (building) is added.
        /// Updates influence values around the influencer by linear propogation.
        /// </summary>
        /// <param name="influencer">The new influencer.</param>
        public void InfluencerAdded(Entity influencer)
        {
            float strength = 0;

            // Assign influence value based on type.
            // TODO: This should be a dictionary
            if (influencer is Building)
            {
                if (influencer is TownHall)
                    strength = 100;
                else if (influencer is House)
                    strength = 15;
                else if (influencer is Track)
                    strength = 30;
                else if (influencer is DepositBox)
                    strength = 50;
            }
            else if (influencer is Unit)
                strength = 0;

            // Increment influence at cells the entity touches.
            foreach (Point index in _grid.IndexesInRect(influencer.CollisionRect))
                this[index] += strength;

            // First propogation drop-off
            strength -= 10;

            // How many times influence has propogated to a new ring of cells.
            int numExpansions = 0; 

            // While there is still influence to propogate.
            while (strength > 1)
            {
                // Increment influence in next ring of cells.
                foreach (Point index in GetBorderIndexes(influencer.CollisionRect.GetInflated(20 * numExpansions, 20 * numExpansions)))
                    this[index] += strength;

                // Drop off the propogation value.
                ++numExpansions;
                strength -= 10;
            }
        }

        /// <summary>
        /// Runs a floodfill to get the border of influence from the passed in starting cell.
        /// The border is decided by the layer of cells before influence reaches zero.
        /// </summary>
        /// <param name="startingCell">The starting cell for the floodfill</param>
        public void CalculateInfluenceBorderAroundCell(Cell startingCell)
        {
            _influenceBorder = new List<Cell>();
            var open = new List<Cell>();
            var closed = new List<Cell>();
            var current = startingCell;

            open.Add(startingCell);

            while (open.Count > 0)
            {
                current = open[0];

                bool isBorderCell = false;

                foreach (Cell cell in current.Neighbours)
                {
                    if (!open.Contains(cell) && !closed.Contains(cell))
                    {
                        if (this[_grid.IndexAt(cell.Mid)] > 0)
                            open.Add(cell);
                        else
                            isBorderCell = true;
                    }
                }

                if (isBorderCell)
                    _influenceBorder.Add(current);

                open.Remove(current);
                closed.Add(current);
            }
        }

        /// <summary>
        /// Renders the InfluenceMap to the screen.
        /// Higher blue value for higher influence, displays influence value at center of cell.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to render to.</param>
        public void Render(SpriteBatch spriteBatch)
        {
            // Find maximum influence value for color scaling.
            float maxInfluence = 0;

            for (int col = 0; col < _grid.Cols; col++)
            {
                for (int row = 0; row < _grid.Rows; row++)
                {
                    float influence = this[col, row];

                    if (influence > maxInfluence)
                        maxInfluence = influence;
                }
            }

            float colorScale = 255 / maxInfluence;

            // ((oldval - Min)*(255/(Max-Min)))
            // (oldVal - 0) * (255 / maxInfluence)
            // oldVal * (255 / maxInfluence)

            // Render cells based on their influence value.
            for (int col = 0; col < _grid.Cols; col++)
            {
                for (int row = 0; row < _grid.Rows; row++)
                {
                    if (this[col, row] > 0)
                    {
                        int color = (int)(this[col, row] * colorScale);
                        spriteBatch.FillRectangle(_grid[col, row].RenderRect, new Color(0, 0, color + 50, 150));
                        spriteBatch.DrawString(MinecaRTS.smallFont, ((int)(this[col, row])).ToString(), _grid[col, row].RenderMid, Color.White);
                    }
                }
            }

            foreach (Cell cell in _influenceBorder)
                spriteBatch.FillRectangle(cell.RenderRect, Color.White);
            
        }
    }
}
