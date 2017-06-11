using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    // To simplify reasoning and save the hands!
    using CollisionCellMap = List<List<HashSet<Unit>>>;

    /// <summary>
    /// The spatial partition used to greatly reduce number of collision checks.
    /// Splits the world into a coarse grid and only checks collision of Entities in the same cell.
    /// </summary>
    public class CollisionCellData
    {
        /// <summary>
        /// The grid used for spatial partitioning.
        /// </summary>
        private Grid _grid;

        /// <summary>
        /// Maps the Grid cells to the units within them.
        /// </summary>
        private CollisionCellMap _cellMap;

        /// <summary>
        /// Gets the units in the cell at the specified column and row.
        /// </summary>
        /// <param name="col">The column index.</param>
        /// <param name="row">The row index.</param>
        public HashSet<Unit> this[int col, int row]
        {
            get
            {
                if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                    return new HashSet<Unit>(); // Return empty list if out of range
                return _cellMap[col][row];
            }
        }

        /// <summary>
        /// Gets the units in the cell at the specified index.
        /// </summary>
        /// <param name="cellIndex">The cell index.</param>
        public HashSet<Unit> this[Point cellIndex]
        {
            get { return this[cellIndex.Col(), cellIndex.Row()]; }
        }

        /// <summary>
        /// Initialises a new CollisionCellData.
        /// </summary>
        /// <param name="grid">The grid used for partitioning.</param>
        public CollisionCellData(Grid grid)
        {
            _grid = grid;
            _cellMap = new CollisionCellMap();

            // Initialize each value in the 2D list of CollisionCells.
            for (int col = 0; col < grid.Cols; col++)
            {
                _cellMap.Add(new List<HashSet<Unit>>());
                for (int row = 0; row < grid.Rows; row++)
                {
                    _cellMap[col].Add(new HashSet<Unit>());
                }
            }
        }

        /// <summary>
        /// Gets the grid.
        /// </summary>
        public Grid Grid
        {
            get { return _grid; }
        }

        /// <summary>
        /// Adds the passed in unit to the collision cell it occupies.
        /// </summary>
        /// <param name="u">The unit to add.</param>
        public void AddUnit(Unit u)
        {
            this[_grid.IndexAt(u.Mid)].Add(u);
        }

        /// <summary>
        /// Removes the passed in unit from the collision cell it occupies.
        /// </summary>
        /// <param name="u"></param>
        public void RemoveUnit(Unit u)
        {
            this[_grid.IndexAt(u.Mid)].Remove(u);
        }

        /// <summary>
        /// Removes the passed in unit from the collision cell it occupied last tick and
        /// adds it to the collision cell it now occupies.
        /// </summary>
        /// <param name="u"></param>
        public void UnitMoved(Unit u)
        {
            this[_grid.IndexAt(u.lastMid)].Remove(u);
            this[_grid.IndexAt(u.Mid)].Add(u);
        }

        /// <summary>
        /// Gets the units in the collision cell at the passed in position.
        /// </summary>
        /// <param name="pos">The position to check.</param>
        public HashSet<Unit> GetUnitsInSameCellAsPos(Vector2 pos)
        {
            return this[_grid.IndexAt(pos)];
        }

        /// <summary>
        /// Gets the units in collision cells in a 3x3 grid around the cell at the passed in position.
        /// </summary>
        /// <param name="pos">The pos to serve as the centre cell.</param>
        public List<HashSet<Unit>> GetUnitsInCellsAroundPos(Vector2 pos)
        {
            var result = new List<HashSet<Unit>>();

            foreach (Point cellIndex in _grid.Get33GridIndexesAroundPos(pos))
                result.Add(this[cellIndex]);

            return result;
        }

        /// <summary>
        /// Gets the units in the same collision cell as the passed in unit.
        /// </summary>
        /// <param name="u">The unit.</param>
        public HashSet<Unit> GetUnitsInSameCellAsUnit(Unit u)
        {
            return GetUnitsInSameCellAsPos(u.Mid);
        }

        /// <summary>
        /// Gets the units in the cells in a 3x3 grid area around the same cell as the passed in unit.
        /// </summary>
        /// <param name="u">The unit.</param>
        public List<HashSet<Unit>> GetUnitsInCellsAroundUnit(Unit u)
        {
            return GetUnitsInCellsAroundPos(u.Mid);
        }      

        /// <summary>
        /// Returns the indexes of the cells in a 3x3 grid around the passed in position.
        /// </summary>
        /// <param name="pos">The pos to check.</param>
        public List<Point> GetCellIndexesAroundPos(Vector2 pos)
        {
            var result = new List<Point>();
            Point cellIndex = _grid.IndexAt(pos);

            for (int col = cellIndex.Col() - 1; col <= cellIndex.Col() + 1; col++)
            {
                for (int row = cellIndex.Row() - 1; row <= cellIndex.Row() + 1; row++)
                {
                    if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                        continue;

                    result.Add(new Point(col, row));
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the units in the collision cell at the passed in index.
        /// </summary>
        /// <param name="index">The index.</param>
        public HashSet<Unit> GetUnitsInCellFromIndex(Point index)
        {
            return GetUnitsInCellFromIndex(index.Col(), index.Row());
        }

        /// <summary>
        /// Gets the units in the collision cell at the passed in column and row index.
        /// </summary>
        /// <param name="col">The column index.</param>
        /// <param name="row">The row index.</param>
        /// <returns></returns>
        public HashSet<Unit> GetUnitsInCellFromIndex(int col, int row)
        {
            if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                return new HashSet<Unit>(); // Return blank list if out of range.
            else
                return this[col, row];
        }
    }
}
