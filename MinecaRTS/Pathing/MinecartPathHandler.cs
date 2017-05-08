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
        public MinecartPathHandler(Unit owner, Grid grid) : base (owner, grid)
        {
        }

        public override void GetPathTo(Vector2 targetPos)
        {
            Building buildingAtPos = owner.Data.BuildingAtPos(targetPos);

            if (buildingAtPos != null)
                GetPathToBuildingFollowingTracks(buildingAtPos);
            else if (owner.Data.Grid.CellAt(targetPos).Passable)
                GetPathToPosFollowingTracks(targetPos);

            pathIndex = 0;

            if (path.Count > 0)
            {
                owner.FollowPath = true;

                ticksSpentTravellingToCell = 0;
                estimatedTicksToReachNextCell = GetEstimatedTicksToReachCell(path[0]);
            }                
        }

        public void GetPathToPosFollowingTracks(Vector2 targetPos)
        {
            Cell sourceCell;
            Cell targetCell = grid.CellAt(targetPos);

            path = GetPathToNearbyTrack();

            if (path.Count > 0)
                sourceCell = path.Last();
            else
                sourceCell = grid.CellAt(owner.Mid);

            path.AddRange(Pathfinder.SearchGreedy(grid, sourceCell, targetCell, owner, GreedyConsiderationCondition, GreedyTerminationCondition, TrackScoreMethod, false));
        }

        public void GetPathToBuildingFollowingTracks(Building building)
        {
            Cell sourceCell;
            Cell targetCell = grid.CellAt(building.Mid);

            path = GetPathToNearbyTrack();

            if (path.Count > 0)
                sourceCell = path.Last();
            else
                sourceCell = grid.CellAt(owner.Mid);

            bool changeCells = !(building is Track);
            var cellsBuildingIsTouching = grid.CellsInRect(building.CollisionRect);

            // Temporarily make Building cells passable.
            if (changeCells)
            {
                foreach (Cell c in cellsBuildingIsTouching)
                    c.Passable = true;
            }

            // Get the path
            path.AddRange(Pathfinder.SearchGreedy(grid, sourceCell, targetCell, owner, GreedyConsiderationCondition, GreedyTerminationCondition, TrackScoreMethod, false));
            
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
            var sourceCell = grid.CellAt(owner.Mid);

            return Pathfinder.SearchDijkstra(grid, sourceCell, owner, GreedyConsiderationCondition, TerminationConditionFindTrack, smoothed: true, depthLimit: 100);
        }
    }
}