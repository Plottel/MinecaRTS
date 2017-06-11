using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    // To simplify reasoning and save the hands!
    using VisionMap = List<List<bool>>;

    /// <summary>
    /// The class determining vision in the program. Keeps track of which cells players can currently see, 
    /// as well as which areas they have explored. Utilises the coarse grid.
    /// </summary>
    public class FogOfWarData
    {
        private Grid _grid;
        private World _world;
        private CollisionCellData _collisionCells;
        private VisionMap _teamOneCurrentVision;
        private VisionMap _teamTwoCurrentVision;
        private VisionMap _teamOneExploredVision;
        private VisionMap _teamTwoExploredVision;

        /// <summary>
        /// Gets or sets the current vision at a given cell index for a given team.
        /// </summary>
        /// <param name="team">The team to set vision for.</param>
        /// <param name="col">The column index.</param>
        /// <param name="row">The row index.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Initializes a new FogOfWarData.
        /// </summary>
        /// <param name="grid">Coarse grid for the cells.</param>
        /// <param name="collisionCells">Collision cells to fetch units for updating.</param>
        /// <param name="world">World for data requests.</param>
        public FogOfWarData(Grid grid, CollisionCellData collisionCells, World world)
        {
            _grid = grid;
            _collisionCells = collisionCells;
            _world = world;
            
            // Initialize vision cell data.
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

        /// <summary>
        /// Sets the current vision at the given cell for the given team.
        /// </summary>
        /// <param name="team">Team to set vision for.</param>
        /// <param name="col">Column index</param>
        /// <param name="row">Row index</param>
        /// <param name="value">Set vision to true or false</param>
        private void SetCellVisibilityForTeam(Team team, int col, int row, bool value)
        {
            if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                return;

            if (team == Team.One)
                _teamOneCurrentVision[col][row] = value;

            _teamTwoCurrentVision[col][row] = value;
        }

        /// <summary>
        /// Overload to set cell vision with Point instead of col / row
        /// </summary>
        private void SetCellVisibilityForTeam(Team team, Point cellIndex, bool value)
        {
            SetCellVisibilityForTeam(team, cellIndex.Col(), cellIndex.Row(), value);
        }

        /// <summary>
        /// Sets cell for team as explored. When vision leaves this cell, neutral objects will still be visible.
        /// </summary>
        /// <param name="team">Team to set visino for.</param>
        /// <param name="col">Column index</param>
        /// <param name="row">Row index</param>
        private void SetCellAsExploredForTeam(Team team, int col, int row)
        {
            if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                return;

            if (team == Team.One)
                _teamOneExploredVision[col][row] = true;
            else
                _teamTwoExploredVision[col][row] = true;
        }

        /// <summary>
        /// Overload for Point instead of col / row.
        /// </summary>
        private void SetCellAsExploredForTeam(Team team, Point cellIndex)
        {
            SetCellAsExploredForTeam(team, cellIndex.Col(), cellIndex.Row());
        }

        /// <summary>
        /// Returns whether or not the passed in team can see the cell at the given index.
        /// </summary>
        /// <param name="team">Team to check vision for.</param>
        /// <param name="col">Column index</param>
        /// <param name="row">Row index</param>
        public bool TeamCanSeeCell(Team team, int col, int row)
        {
            if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                return false;

            if (team == Team.One)
                return _teamOneCurrentVision[col][row];

            return _teamTwoCurrentVision[col][row];
        }

        /// <summary>
        /// Overload for Point instead of col / row
        /// </summary>
        public bool TeamCanSeeCell(Team team, Point cellIndex)
        {
            return TeamCanSeeCell(team, cellIndex.Col(), cellIndex.Row());
        }

        /// <summary>
        /// Returns whether or not the passed in team can see the current position.
        /// </summary>
        /// <param name="team">Team to check vision for.</param>
        /// <param name="pos">Position to check vision at.</param>
        /// <returns></returns>
        public bool TeamCanSeePos(Team team, Vector2 pos)
        {
            return TeamCanSeeCell(team, _grid.IndexAt(pos));
        }

        /// <summary>
        /// Returns whether or not the team has explored the cell.
        /// Not necessariyl currently visible, but has been explored at one point.
        /// </summary>
        /// <param name="team">Team to check exploration status for.</param>
        /// <param name="col">Column index</param>
        /// <param name="row">Row index</param>
        public bool TeamHasExploredCell(Team team, int col, int row)
        {
            if (col < 0 || col >= _grid.Cols || row < 0 || row >= _grid.Rows)
                return false;

            if (team == Team.One)
                return _teamOneExploredVision[col][row];
            else
                return _teamTwoExploredVision[col][row];
        }

        /// <summary>
        /// Overload for Point instead of col / row
        /// </summary>
        public bool TeamHasExploredCell(Team team, Point cellIndex)
        {
            return TeamHasExploredCell(team, cellIndex.Col(), cellIndex.Row());
        }

        /// <summary>
        /// Returns whether or not the passed in team has explored the passed in position.
        /// Not necessarily currently visible, but has been explored at least once.
        /// </summary>
        /// <param name="team">Team to check exploration status for.</param>
        /// <param name="pos">Position to check.</param>
        /// <returns></returns>
        public bool TeamHasExploredPos(Team team, Vector2 pos)
        {
            return TeamHasExploredCell(team, _grid.IndexAt(pos));
        }

        /// <summary>
        /// Called whenever a new building is created.
        /// Adjusts fog of war values for cells affected by the building.
        /// </summary>
        /// <param name="b">The building.</param>
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
        
        /// <summary>
        /// Called whenever a unit moves.
        /// Adjusts fog of war values for cells affected by the unit.
        /// Removes values from old position of the unit, adds new values for current position.
        /// </summary>
        /// <param name="u">The unit to update values based on.</param>
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

        /// <summary>
        /// Specifies whether or not the pased in cell index should be removed from current vision.
        /// Checks all entities for the team to see if any of them affect the cell.
        /// </summary>
        /// <param name="team">Team to check vision for.</param>
        /// <param name="cellIndex">Cell index to check vision for.</param>
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

        /// <summary>
        /// Called whenever a Unit is added.
        /// Updates vision for cells affected by the unit.
        /// </summary>
        /// <param name="u">The unit.</param>
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
