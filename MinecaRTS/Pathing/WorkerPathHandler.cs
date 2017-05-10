using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    class WorkerPathHandler : PathHandler
    {
        private Worker _owner;

        public WorkerPathHandler(Worker owner, Grid grid) : base(owner, grid)
        {
            _owner = owner;
        }

        public override void GetPathTo(Vector2 targetPos)
        {
            Building buildingAtPos = _owner.Data.BuildingAtPos(targetPos);

            if (buildingAtPos != null)
                GetPathToBuilding(buildingAtPos);
            else
            {
                Cell cell = _owner.Data.Grid.CellAt(targetPos);
                Resource resource = _owner.Data.GetResourceFromCell(cell);

                if (resource != null)
                    GetPathToResource(resource);
                else
                    base.GetPathTo(targetPos);
            }

            pathIndex = 0;

            if (path.Count > 0)
            {
                _owner.FollowPath = true;
                ticksSpentTravellingToCell = 0;
                estimatedTicksToReachNextCell = GetEstimatedTicksToReachCell(path[0]);
            }               
        }

        public void GetPathToClosestUnsaturatedResource(ResourceType resourceType)
        {
            var sourceCell = grid.CellAt(owner.Mid);

            if (resourceType == ResourceType.Wood)
                path = pathfinder.SearchDijkstra(grid, sourceCell, owner, ConsiderationConditionWood, TerminationConditionWood, true);
            else if (resourceType == ResourceType.Stone)
                path = pathfinder.SearchDijkstra(grid, sourceCell, owner, ConsiderationConditionStone, TerminationConditionStone, true);
            else
                throw new Exception("Cannot fetch a path for None resource");

            // TODO: This is a special case where pathIndex / FollowPath is necessary since currently it can't be implemented in terms of GetPathTo.
            // Consider reworking.
            pathIndex = 0;

            if (path.Count > 0)
            {
                owner.FollowPath = true;
                ticksSpentTravellingToCell = 0;
                estimatedTicksToReachNextCell = GetEstimatedTicksToReachCell(path[0]);
            }
                
        }

        public void GetPathToResource(Resource resource)
        {
            var sourceCell = grid.CellAt(owner.Mid);
            var targetCell = grid.CellAt(resource.Mid);

            targetCell.Passable = true;

            path = pathfinder.SearchGreedy(grid, sourceCell, targetCell, owner, GreedyConsiderationCondition, GreedyTerminationCondition, GreedyScoreMethod, true);

            targetCell.Passable = false;
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

            var sourceCell = grid.CellAt(owner.Mid);
            var targetCell = grid.CellAt(building.Mid);
            var cellsBuildingIsTouching = grid.CellsInRect(building.CollisionRect);

            // Temporarily make Building cells passable.
            if (changeCells)
            {
                foreach (Cell c in cellsBuildingIsTouching)
                    c.Passable = true;
            }

            // Get the path
            path = pathfinder.SearchGreedy(grid, sourceCell, targetCell, owner, GreedyConsiderationCondition, GreedyTerminationCondition, GreedyScoreMethod, true);

            // Revert Building cells to !Passable
            if (changeCells)
            {
                foreach (Cell c in cellsBuildingIsTouching) 
                    c.Passable = false;
            }
        }
    }
}
