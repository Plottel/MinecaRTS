using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    using CollisionCellMap = List<List<HashSet<Unit>>>;

    public class CollisionCellData
    {
        private Grid _grid;
        private CollisionCellMap _cellMap;

        public HashSet<Unit> this[int col, int row]
        {
            get
            {
                if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                    return new HashSet<Unit>(); // Return empty list if out of range
                return _cellMap[col][row];
            }
        }

        public HashSet<Unit> this[Point cellIndex]
        {
            get { return this[cellIndex.Col(), cellIndex.Row()]; }
        }

        public CollisionCellData(Grid grid)
        {
            _grid = grid;
            _cellMap = new CollisionCellMap();

            for (int col = 0; col < grid.Cols; col++)
            {
                _cellMap.Add(new List<HashSet<Unit>>());
                for (int row = 0; row < grid.Rows; row++)
                {
                    _cellMap[col].Add(new HashSet<Unit>());
                }
            }
        }

        public Grid Grid
        {
            get { return _grid; }
        }

        public void AddUnit(Unit u)
        {
            this[_grid.IndexAt(u.Mid)].Add(u);
        }

        public void RemoveUnit(Unit u)
        {
            this[_grid.IndexAt(u.Mid)].Remove(u);
        }

        public HashSet<Unit> GetUnitsInSameCellAsUnit(Unit u)
        {
            return this[_grid.IndexAt(u.Mid)];
        }

        public List<HashSet<Unit>> GetUnitsInCellsAroundUnit(Unit u)
        {
            var result = new List<HashSet<Unit>>();

            foreach (Point cellIndex in _grid.Get33GridIndexesAroundPos(u.Mid))
                result.Add(this[cellIndex]);

            return result;
        }

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

        public HashSet<Unit> GetUnitsInCellFromIndex(Point index)
        {
            return GetUnitsInCellFromIndex(index.Col(), index.Row());
        }

        public HashSet<Unit> GetUnitsInCellFromIndex(int col, int row)
        {
            if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                return new HashSet<Unit>(); // Return blank list if out of range.
            else
                return this[col, row];
        }
    }
}
