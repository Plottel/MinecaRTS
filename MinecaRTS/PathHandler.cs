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

        public void GetPathToClosestResource(ResourceType resource)
        {
            var sourceCell = _grid.CellAt(_owner.Mid);

            _path = Pathfinder.SearchClosestResource(_grid, sourceCell, resource, _owner, true);
            _pathIndex = 0;

            if (_path.Count > 0)
                _owner.FollowPath = true;
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

            if (targetCell.Passable)
            {
                _path = Pathfinder.SearchGreedy(_grid, sourceCell, targetCell, _owner, true);
                _pathIndex = 0;

                if (_path.Count > 0)
                    _owner.FollowPath = true;
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