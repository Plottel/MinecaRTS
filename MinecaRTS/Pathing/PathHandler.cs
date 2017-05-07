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
            bool changeCells = !(building is Track);

            // TODO: Possible optimisation by making target a Cell on the edge of building from which unit will approach.
            // Prevents a possibly long List.Contains() for every node.

            var sourceCell = _grid.CellAt(_owner.Mid);
            var targetCell = _grid.CellAt(building.Mid);
            var cellsBuildingIsTouching = _grid.CellsInRect(building.CollisionRect);

            // Temporarily make Building cells passable.
            if (changeCells)
            {
                foreach (Cell c in cellsBuildingIsTouching)
                    c.Passable = true;
            }            

            // Get the path
            _path = Pathfinder.SearchGreedy(_grid, sourceCell, targetCell, _owner, GreedyConsiderationCondition, GreedyTerminationCondition, GreedyScoreMethod, true);
            _pathIndex = 0;

            if (_path.Count > 0)
                _owner.FollowPath = true;

            // Revert Building cells to !Passable
            if (changeCells)
            {
                foreach (Cell c in cellsBuildingIsTouching)
                    c.Passable = false;
            }
        }

        /// <summary>
        /// Used by minecarts to check if there's a track near them. This path is then concatenated with the path to the destination.
        /// </summary>
        /// <returns></returns>
        private List<Cell> GetPathToNearbyTrack()
        {
            var sourceCell = _grid.CellAt(_owner.Mid);

            return Pathfinder.SearchDijkstra(_grid, sourceCell, _owner, GreedyConsiderationCondition, TerminationConditionFindTrack, smoothed:true, depthLimit:100);
        }

        public void GetPathToBuildingFollowingTracks(Building building)
        {
            Cell sourceCell;
            Cell targetCell = _grid.CellAt(building.Mid);

            _path = GetPathToNearbyTrack();

            if (_path.Count > 0)
                sourceCell = _path.Last();
            else
                sourceCell = _grid.CellAt(_owner.Mid);

            bool changeCells = !(building is Track);
            var cellsBuildingIsTouching = _grid.CellsInRect(building.CollisionRect);

            // Temporarily make Building cells passable.
            if (changeCells)
            {
                foreach (Cell c in cellsBuildingIsTouching)
                    c.Passable = true;
            }
            

            // Get the path
            _path.AddRange(Pathfinder.SearchGreedy(_grid, sourceCell, targetCell, _owner, GreedyConsiderationCondition, GreedyTerminationCondition, TrackScoreMethod, false));
            _pathIndex = 0;

            if (_path.Count > 0)
                _owner.FollowPath = true;

            // Revert Building cells to !Passable
            if (changeCells)
            {
                foreach (Cell c in cellsBuildingIsTouching)
                    c.Passable = false;
            }
        }

        public void GetPathToResource(Resource resource)
        {
            var sourceCell = _grid.CellAt(_owner.Mid);
            var targetCell = _grid.CellAt(resource.Mid);

            targetCell.Passable = true;

            _path = Pathfinder.SearchGreedy(_grid, sourceCell, targetCell, _owner, GreedyConsiderationCondition, GreedyTerminationCondition, GreedyScoreMethod, true);
            _pathIndex = 0;

            if (_path.Count > 0)
                _owner.FollowPath = true;

            targetCell.Passable = false;
        }

        public void GetPathToClosestUnsaturatedResource(ResourceType resourceType)
        {
            var sourceCell = _grid.CellAt(_owner.Mid);

            if (resourceType == ResourceType.Wood)
                _path = Pathfinder.SearchDijkstra(_grid, sourceCell, _owner, ConsiderationConditionWood, TerminationConditionWood, true);
            else if (resourceType == ResourceType.Stone)
                _path = Pathfinder.SearchDijkstra(_grid, sourceCell, _owner, ConsiderationConditionStone, TerminationConditionStone, true);
            else
                throw new Exception("Cannot fetch a path for None resource");

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

            _path = Pathfinder.SearchGreedy(_grid, sourceCell, targetCell, _owner, GreedyConsiderationCondition, GreedyTerminationCondition, GreedyScoreMethod, true);
            _pathIndex = 0;

            if (_path.Count > 0)
                _owner.FollowPath = true;           
        }        

        public void GetPathToPosFollowingTracks(Vector2 targetPos)
        {
            Cell sourceCell;
            Cell targetCell = _grid.CellAt(targetPos);

            _path = GetPathToNearbyTrack();

            if (_path.Count > 0)
                sourceCell = _path.Last();
            else
                sourceCell = _grid.CellAt(_owner.Mid);

            _path.AddRange(Pathfinder.SearchGreedy(_grid, sourceCell, targetCell, _owner, GreedyConsiderationCondition, GreedyTerminationCondition, TrackScoreMethod, false));
            _pathIndex = 0;

            if (_path.Count > 0)
                _owner.FollowPath = true;            
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

        #region Search Conditions
        float TrackScoreMethod(Cell cell, Cell Target)
        {
            float score = GreedyScoreMethod(cell, Target);

            Track t = _owner.Data.GetTrackFromCell(cell);

            if (t == null || !t.IsActive)
                score *= 5;

            return score;
        }

        bool ConsiderationConditionWood(Cell cell)
        {
            if (cell.Passable)
                return true;

            Resource resource = _owner.Data.GetResourceFromCell(cell);

            if (resource == null)
                return false;

            // Valid if resource is the correct type and not saturated.
            return resource.Type == ResourceType.Wood && !resource.IsSaturated;
        }

        bool ConsiderationConditionStone(Cell cell)
        {
            if (cell.Passable)
                return true;

            Resource resource = _owner.Data.GetResourceFromCell(cell);

            if (resource == null)
                return false;

            // Valid if resource is the correct type and not saturated.
            return resource.Type == ResourceType.Stone && !resource.IsSaturated;
        }

        bool TerminationConditionWood(Cell current)
        {
            Resource resource = _owner.Data.GetResourceFromCell(current);

            if (resource == null)
                return false;
            else if (resource.Type != ResourceType.Wood)
                return false;

            return !resource.IsSaturated;
        }

        bool TerminationConditionStone(Cell current)
        {
            Resource resource = _owner.Data.GetResourceFromCell(current);

            if (resource == null)
                return false;
            else if (resource.Type != ResourceType.Stone)
                return false;

            return !resource.IsSaturated;
        }

        bool TerminationConditionFindTrack(Cell current)
        {
            return _owner.Data.CellHasTrack(current);
        }

        // Consideration condition for a standard path.
        bool GreedyConsiderationCondition(Cell cell)
        {
            return cell.Passable;
        }

        // Termination condition for a standard path.
        bool GreedyTerminationCondition(Cell current, Cell target)
        {
            return current == target;
        }

        float GreedyScoreMethod(Cell cell, Cell Target)
        {
            return Vector2.Distance(cell.Mid, Target.Mid);
        }
        #endregion Search Conditions
    }
}