using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    public class MinecartPathHandler : PathHandler
    {
        private List<Cell> _tempPathToTrack = new List<Cell>();

        public MinecartPathHandler(Unit owner, Grid grid) : base (owner, grid)
        {
        }        

        public override void HandleMessage(Message message)
        {
            
            switch (message.type)
            {
                case MessageType.SearchComplete:
                    var searchState = message.extraInfo;

                    if (searchState == SearchState.Failed)
                        path = new List<Cell>();
                    else
                    {
                        path = _tempPathToTrack;
                        path.AddRange(pathfinder.RetracePath());

                        _tempPathToTrack = new List<Cell>();
                        FinalisePath();
                    }
                    break;
            }
        }

        public override void GetPathTo(Vector2 targetPos)
        {
            Building buildingAtPos = owner.Data.BuildingAtPos(targetPos);

            if (buildingAtPos != null)
                GetPathToBuildingFollowingTracks(buildingAtPos);
            else if (owner.Data.Grid.CellAt(targetPos).Passable)
                GetPathToPosFollowingTracks(targetPos);

            FinalisePath();           
        }

        public void GetPathToPosFollowingTracks(Vector2 targetPos)
        {
            Cell sourceCell;
            var targetCell = new List<Cell> { grid.CellAt(targetPos) };

            _tempPathToTrack = GetPathToNearbyTrack();

            if (_tempPathToTrack.Count > 0)
                sourceCell = path.Last();
            else
                sourceCell = grid.CellAt(owner.Mid);

            if (Debug.IsOn(DebugOp.EnableTimeSlicedPathing))
            {
                pathfinder.SetupGreedy(grid, sourceCell, targetCell, owner, GreedyConsiderationCondition, GreedyTerminationCondition, TrackScoreMethod, false);
                TimeSlicedPathManager.AddSearch(pathfinder);
            }
            else
            {
                path = _tempPathToTrack;
                path.AddRange(pathfinder.SearchGreedy(grid, sourceCell, targetCell, owner, GreedyConsiderationCondition, GreedyTerminationCondition, TrackScoreMethod, false));
                _tempPathToTrack = new List<Cell>();
            }

        }

        public void GetPathToBuildingFollowingTracks(Building building)
        {
            Cell sourceCell;

            var inflatedBuildingCells = grid.CellsInRect(building.CollisionRect.GetInflated(16, 16));
            var buildingCells = grid.CellsInRect(building.CollisionRect);

            var borderCells = inflatedBuildingCells.Where(element => !buildingCells.Contains(element)).ToList();
            borderCells = borderCells.Where(element => element.Passable).ToList();

            _tempPathToTrack = GetPathToNearbyTrack();

            if (_tempPathToTrack.Count > 0)
                sourceCell = path.Last();
            else
                sourceCell = grid.CellAt(owner.Mid);

            if (Debug.IsOn(DebugOp.EnableTimeSlicedPathing))
            {
                pathfinder.SetupGreedy(grid, sourceCell, borderCells, owner, GreedyConsiderationCondition, GreedyTerminationCondition, TrackScoreMethod, false);
                TimeSlicedPathManager.AddSearch(pathfinder);
            }
            else
            {
                path = _tempPathToTrack;
                path.AddRange(pathfinder.SearchGreedy(grid, sourceCell, borderCells, owner, GreedyConsiderationCondition, GreedyTerminationCondition, TrackScoreMethod, false));
                _tempPathToTrack = new List<Cell>();
            }
        }

        /// <summary>
        /// Used by minecarts to check if there's a track near them. This path is then concatenated with the path to the destination.
        /// </summary>
        /// <returns></returns>
        private List<Cell> GetPathToNearbyTrack()
        {
            var sourceCell = grid.CellAt(owner.Mid);

            return pathfinder.SearchDijkstra(grid, sourceCell, owner, GreedyConsiderationCondition, TerminationConditionFindTrack, smoothed: true, depthLimit: 100);
        }
    }
}