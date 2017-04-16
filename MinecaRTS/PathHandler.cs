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
    /// <summary>
    /// Responsible for getting a unit to follow a path.
    /// Each unit has their own instance of PathHandler.
    /// </summary>
    public class PathHandler
    {
        /// <summary>
        /// Which unit the PathHandler belongs to.
        /// </summary>
        private Unit _owner;

        /// <summary>
        /// The grid used for path handling.
        /// </summary>
        private Grid _grid;

        /// <summary>
        /// The current path.
        /// </summary>
        public List<Cell> _path;

        /// <summary>
        /// The index in the path the unit is currently moving towards.
        /// </summary>
        private int _pathIndex;

        /// <summary>
        /// How close a unit has to be to a cell before it has "reached" it.
        /// </summary>
        private float _waypointThreshold;

        public bool HasPath
        {
            get { return _path.Count > 0; }
        }

        public Cell TargetCell
        {
            get { return _path[_pathIndex]; }
        }

        public PathHandler(Unit owner, Grid grid)
        {
            _owner = owner;
            _grid = grid;
            _path = new List<Cell>();
            _pathIndex = 0;
            _waypointThreshold = 20;
        }

        /// <summary>
        /// Checks if owner has reached current cell index and, if it has, orients towards next cell.
        /// </summary>
        public Vector2 Update()
        {
            Vector2 force = Vector2.Zero;

            // Re-orient owner towards target cell to keep on track.
            force = OrientTowardsCell(_path[_pathIndex]);

            if (ReachedCell(_path[_pathIndex]))
            {
                if (_pathIndex < _path.Count - 1)
                    force = OrientTowardsCell(_path[++_pathIndex]);
                else
                {
                    _owner.FollowPath = false;
                    force = Vector2.Zero;
                }                   
            }

            return force;
        }

        /// <summary>
        /// Adjusts owner's velocity to point towards the passed in cell.
        /// </summary>
        /// <param name="cell">The cell to orient towards.</param>
        private Vector2 OrientTowardsCell(Cell cell)
        {
            return Vector2.Normalize(cell.Mid - _owner.Mid);
        }

        /// <summary>
        /// Specifies if owner is within the waypoint threshold for the pased in cell.
        /// </summary>
        /// <param name="cell">The cell to check if reached.</param>
        /// <returns></returns>
        private bool ReachedCell(Cell cell)
        {
            // TODO: Since it's all relative, consider LengthSq.
            var distanceFromCell = (cell.Mid - _owner.Mid).Length();

            return distanceFromCell <= _waypointThreshold;
        }

        /// <summary>
        /// Gets a Greedy path to the passed in building.
        /// Cheats by making all cells the building touches temporarily passable for searching.
        /// </summary>
        /// <param name="building"></param>
        public void GetPathToBuilding(Building building)
        {
            // TODO: Possible optimisation by making target a Cell on the edge of building from which unit will approach.
            // Prevents a possibly long List.Contains() for every node.

            var sourceCell = _grid.CellAt(_owner.Mid);
            var targetCell = _grid.CellAt(building.Mid);

            // Temporarily make Building cells passable.
            var cellsBuildingIsTouching = _grid.CellsInRect(building.CollisionRect);

            foreach (Cell c in cellsBuildingIsTouching)
                c.Passable = true;

            // Get the path
            _path = Pathfinder.SearchGreedy(_grid, sourceCell, targetCell, _owner, ConsiderationCondition, TerminationCondition, true);
            _pathIndex = 0;

            if (_path.Count > 0)
                _owner.FollowPath = true;

            // Revert Building cells to !Passable
            foreach (Cell c in cellsBuildingIsTouching)
                c.Passable = false;

            // Consideration condition for getting a path to a building.
            bool ConsiderationCondition(Cell cell)
            {
                return cell.Passable;
            }

            // Termination condition for getting a path to a building
            bool TerminationCondition(Cell current, Cell target)
            {
                return current == target;
            }
        }

        public void GetPathToClosestUnsaturatedResource(ResourceType resource)
        {
            var sourceCell = _grid.CellAt(_owner.Mid);

            if (resource == ResourceType.Wood)
                _path = Pathfinder.SearchDijkstra(_grid, sourceCell, _owner, ConsiderationConditionWood, TerminationConditionWood, true);
            else if (resource == ResourceType.Stone)
                _path = Pathfinder.SearchDijkstra(_grid, sourceCell, _owner, ConsiderationConditionStone, TerminationConditionStone, true);
            else
                throw new Exception("Cannot fetch a path for None resource");

            _pathIndex = 0;

            if (_path.Count > 0)
                _owner.FollowPath = true;

            bool ConsiderationConditionWood(Cell cell)
            {
                if (cell.Passable)
                    return true;

                // If not passable, cell can be considered if it has the right resource.
                return cell.ResourceType == ResourceType.Wood;
            }

            bool ConsiderationConditionStone(Cell cell)
            {
                if (cell.Passable)
                    return true;

                // If not passable, cell can be considered if it has the right resource.
                return cell.ResourceType == ResourceType.Stone;
            }

            bool TerminationConditionWood(Cell current)
            {
                if (current.ResourceType != ResourceType.Wood)
                    return false;
                else if (current.Resource == null)
                    return false;

                return !current.Resource.IsSaturated;
            }

            bool TerminationConditionStone(Cell current)
            {
                if (current.ResourceType != ResourceType.Stone)
                    return false;
                else if (current.Resource == null)
                    return false;

                return !current.Resource.IsSaturated;
            }
        }

        /// <summary>
        /// Generates a path to the target position.
        /// If owner is set to follow paths, this will orient owner towards first cell in path.
        /// </summary>
        /// <param name="targetPos"></param>
        public void GetPathTo(Vector2 targetPos)
        {
            var sourceCell = _grid.CellAt(_owner.Mid);
            var targetCell = _grid.CellAt(targetPos);

            _path = Pathfinder.SearchGreedy(_grid, sourceCell, targetCell, _owner, ConsiderationCondition, TerminationCondition, true);
            _pathIndex = 0;

            if (_path.Count > 0)
                _owner.FollowPath = true;

            // Consideration condition for a standard path.
            bool ConsiderationCondition(Cell cell)
            {
                return cell.Passable;
            }

            // Termination condition for a standard path.
            bool TerminationCondition(Cell current, Cell target)
            {
                return current == target;
            }
        }

        public void RenderPath(SpriteBatch spriteBatch)
        {
            // Render path
            if (_path.Count > 0)
            {
                spriteBatch.DrawLine(_owner.RenderMid, _path[_pathIndex].RenderMid, Color.LightGreen);

                for (int i = _pathIndex; i < _path.Count - 1; i++)
                {
                    spriteBatch.DrawPoint(_path[i].RenderMid, Color.Blue, 10);
                    spriteBatch.DrawLine(_path[i].RenderMid, _path[i + 1].RenderMid, Color.LightGreen);
                }

                spriteBatch.DrawPoint(_path[_path.Count - 1].RenderMid, Color.Blue, 10);
            }
        }
    }
}