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

        private void SetCellVisibilityForTeam(Team team, int col, int row, bool value)
        {
            if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                return;

            if (team == Team.One)
                _teamOneCurrentVision[col][row] = value;

            _teamTwoCurrentVision[col][row] = value;
        }

        private void SetCellVisibilityForTeam(Team team, Point cellIndex, bool value)
        {
            SetCellVisibilityForTeam(team, cellIndex.Col(), cellIndex.Row(), value);
        }

        private void SetCellAsExploredForTeam(Team team, int col, int row)
        {
            if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                return;

            if (team == Team.One)
                _teamOneExploredVision[col][row] = true;
            else
                _teamTwoExploredVision[col][row] = true;
        }

        private void SetCellAsExploredForTeam(Team team, Point cellIndex)
        {
            SetCellAsExploredForTeam(team, cellIndex.Col(), cellIndex.Row());
        }

        public bool TeamCanSeeCell(Team team, int col, int row)
        {
            if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                return false;

            if (team == Team.One)
                return _teamOneCurrentVision[col][row];

            return _teamTwoCurrentVision[col][row];
        }

        public bool TeamCanSeeCell(Team team, Point cellIndex)
        {
            return TeamCanSeeCell(team, cellIndex.Col(), cellIndex.Row());
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

        public bool TeamHasExploredCell(Team team, Point cellIndex)
        {
            return TeamHasExploredCell(team, cellIndex.Col(), cellIndex.Row());
        }

        // Buildings will grant vision based on their size.
        // Units grant vision in 3x3 grid around their center, regardless of size.
        public void BuildingAdded(Building b)
        {
            Point topLeft = _grid.IndexAt(b.Pos);
            Point bottomRight = _grid.IndexAt(b.CollisionRect.BottomRight());            

            for (int col = topLeft.Col() - 1; col <= bottomRight.Col() + 1; col++)
            {
                for (int row = topLeft.Row() - 1; row <= bottomRight.Row() + 1; row++)
                {
                    SetCellVisibilityForTeam(b.Team, col, row, true);
                    SetCellAsExploredForTeam(b.Team, col, row);
                }
            }
        }
        
        public void UnitMoved(Unit u)
        {
            // Identify which cells the unit WAS granting vision to.
            var oldCellIndexes = _grid.Get33GridIndexesAroundPos(u.lastMid);
            // Identify which cells the unit IS NOW granting vision to.
            var newCellIndexes = _grid.Get33GridIndexesAroundPos(u.Mid);
            // For NEW cells, set visible to true
            foreach (Point p in newCellIndexes)
            {
                SetCellVisibilityForTeam(u.Team, p, true);
                SetCellAsExploredForTeam(u.Team, p);
            }
                
            // For OLD cells that have been DROPPED, check if vision still exists
            var droppedCellIndexes = oldCellIndexes.Where(element => !newCellIndexes.Contains(element)).ToList();

            foreach (Point p in droppedCellIndexes)
                SetCellVisibilityForTeam(u.Team, p, UpdateCellVision(u.Team, p));
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

                var cell = _grid[index];

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
            foreach (Point cellIndex in _grid.Get33GridIndexesAroundPos(u.Mid))
            {
                SetCellVisibilityForTeam(u.Team, cellIndex, true);
                SetCellAsExploredForTeam(u.Team, cellIndex);
            }
        }
    }
}
