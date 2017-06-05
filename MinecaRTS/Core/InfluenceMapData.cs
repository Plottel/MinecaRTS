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
    using InfluenceMap = List<List<float>>;

    public class InfluenceMapData
    {
        private Grid _grid;
        private InfluenceMap _influence;

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

        public float this[Point cellIndex]
        {
            get { return this[cellIndex.Col(), cellIndex.Row()]; }
            private set { this[cellIndex.Col(), cellIndex.Row()] = value; }
        }

        public InfluenceMapData(Grid grid)
        {
            _grid = grid;
            _influence = new List<List<float>>();

            for (int col = 0; col < grid.Cols; col++)
            {
                _influence.Add(new List<float>());
                for (int row = 0; row < grid.Rows; row++)
                {
                    _influence[col].Add(0);
                }
            }
        }

        // TODO: Calculate 4 edges instead of getting bigger rectangle and excluding center.
        public List<Point> GetBorderIndexes(Rectangle rect)
        {
            var inflatedCells = _grid.IndexesInRect(rect.GetInflated(20, 20));
            var currentCells = _grid.IndexesInRect(rect);

            return inflatedCells.Where(element => !currentCells.Contains(element)).ToList();
        }

        public void InfluencerAdded(Entity influencer)
        {
            float strength = 0;

            if (influencer is Building)
                strength = 100;
            else if (influencer is Unit)
                strength = 30;

            foreach (Point index in _grid.IndexesInRect(influencer.CollisionRect))
                this[index] += strength;

            strength -= 10;

            int numExpansions = 0; 

            while (strength > 1)
            {
                foreach (Point index in GetBorderIndexes(influencer.CollisionRect.GetInflated(20 * numExpansions, 20 * numExpansions)))
                    this[index] += strength;

                ++numExpansions;
                strength -= 10;
            }
        }

        public void InfluencerMoved(Entity influencer)
        {
            // Decrement influence in old area
            // Increment influence in new area
        }

        public void InfluencerRemoved(Entity influencer)
        {
            // Decrement influence in area
        }

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
                        //spriteBatch.DrawString(MinecaRTS.smallFont, ((int)(this[col, row])).ToString(), _grid[col, row].RenderMid, Color.White);
                    }
                }
            }
        }
    }
}
