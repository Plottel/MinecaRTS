using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    using VisionMap = List<List<bool>>;

    public class FogOfWarData
    {
        private Grid _grid;
        private World _world;
        private CollisionCellData _collisionCells;
        private VisionMap _teamOneCurrentVision;
        private VisionMap _teamTwoCurrentVision;
        private VisionMap _teamOneExploredVision;
        private VisionMap _teamTwoExploredVision;

        private bool this[Team team, int col, int row]
        {
            get
            {
                if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                    return false;

                if (team == Team.One)
                    return _teamOneCurrentVision[col][row];

                return _teamTwoCurrentVision[col][row];
            }
            
            set
            {
                if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                    return;

                if (team == Team.One)
                    _teamOneCurrentVision[col][row] = value;

                _teamTwoCurrentVision[col][row] = value;
            }
        }

        public FogOfWarData(Grid grid, CollisionCellData collisionCells, World world)
        {
            _grid = grid;
            _collisionCells = collisionCells;
            _world = world;

            _teamOneCurrentVision = new VisionMap();
            _teamTwoCurrentVision = new VisionMap();
            _teamOneExploredVision = new VisionMap();
            _teamTwoExploredVision = new VisionMap();

            for (int col = 0; col < grid.Cols; col++)
            {
                _teamOneCurrentVision.Add(new List<bool>());
                _teamTwoCurrentVision.Add(new List<bool>());
                _teamOneExploredVision.Add(new List<bool>());
                _teamTwoExploredVision.Add(new List<bool>());

                for (int row = 0; row < grid.Rows; row++)
                {
                    _teamOneCurrentVision[col].Add(false);
                    _teamTwoCurrentVision[col].Add(false);
                    _teamOneExploredVision[col].Add(false);
                    _teamTwoExploredVision[col].Add(false);
                }
            }
        }

        private void SetExploredCellForTeam(Team team, int col, int row)
        {
            if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                return;

            if (team == Team.One)
                _teamOneExploredVision[col][row] = true;
            else
                _teamTwoExploredVision[col][row] = true;
        }

        public bool TeamCanSeeCell(Team team, int col, int row)
        {
            return this[team, col, row];
        }

        public bool TeamHasExploredCell(Team team, int col, int row)
        {
            if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                return false;

            if (team == Team.One)
                return _teamOneExploredVision[col][row];
            else
                return _teamTwoExploredVision[col][row];
        }

        public void BuildingAdded(Building b)
        {
            Point topLeft = _grid.IndexAt(b.Pos);
            Point bottomRight = _grid.IndexAt(b.CollisionRect.BottomRight());            

            for (int col = topLeft.Col() - 1; col <= bottomRight.Col() + 1; col++)
            {
                for (int row = topLeft.Row() - 1; row <= bottomRight.Row() + 1; row++)
                {
                    this[b.Team, col, row] = true;
                    SetExploredCellForTeam(b.Team, col, row);
                }
            }
        }
        
        public void UnitMoved(Unit u)
        {
            // Identify which cells the unit WAS granting vision to.
            var oldCellIndexes = _collisionCells.GetCellIndexesAroundPos(u.lastMid);
            // Identify which cells the unit IS NOW granting vision to.
            var newCellIndexes = _collisionCells.GetCellIndexesAroundPos(u.Mid);
            // For NEW cells, set visible to true
            foreach (Point p in newCellIndexes)
            {
                this[u.Team, p.Col(), p.Row()] = true;
                SetExploredCellForTeam(u.Team, p.Col(), p.Row());
            }
                
            // For OLD cells that have been DROPPED, check if vision still exists
            var droppedCellIndexes = oldCellIndexes.Where(element => !newCellIndexes.Contains(element)).ToList();

            foreach (Point p in droppedCellIndexes)
                this[u.Team, p.Col(), p.Row()] = UpdateCellVision(u.Team, p);
        }

        private bool UpdateCellVision(Team team, Point cellIndex)
        {
            foreach (Point index in _grid.Get33GridIndexesAroundIndex(cellIndex))
            {
                foreach (Unit u in _collisionCells.GetUnitsInCellFromIndex(index))
                {
                    if (u.Team == team)
                        return true;
                }

                var cell = _grid[index.Col(), index.Row()];

                if (cell != null)
                {
                    foreach (Building b in _world.Buildings)
                    {
                        if (b.CollisionRect.Intersects(cell.CollisionRect))
                            return true;
                    }
                }
            }            

            return false;
        }

        public void UnitAdded(Unit u)
        {
            Point topLeft = _grid.IndexAt(u.Pos);
            Point bottomRight = _grid.IndexAt(u.CollisionRect.BottomRight());

            for (int col = topLeft.Col() - 1; col <= bottomRight.Col() + 1; col++)
            {
                for (int row = topLeft.Row() - 1; row <= bottomRight.Row() + 1; row++)
                {
                    this[u.Team, col, row] = true;
                    SetExploredCellForTeam(u.Team, col, row);
                }
            }
        }
    }
}
