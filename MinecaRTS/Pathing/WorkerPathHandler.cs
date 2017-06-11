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

        public override void GetPathTo(Vector2 targetPos, bool smoothed = true)
        {
            Building buildingAtPos = _owner.Data.BuildingAtPos(targetPos);

            if (buildingAtPos != null)
                GetPathToBuilding(buildingAtPos);
            else
            {
                Cell cell = grid.CellAt(targetPos);
                Resource resource = _owner.Data.GetResourceFromCell(cell);

                if (resource != null)
                    GetPathToResource(resource);
                else
                    base.GetPathTo(targetPos, smoothed);
            }

            FinalisePath();         
        }

        public void GetPathToClosestUnsaturatedResource(ResourceType resourceType)
        {
            var sourceCell = grid.CellAt(owner.Mid);

            if (Debug.IsOn(DebugOp.EnableTimeSlicedPathing))
            {
                if (resourceType == ResourceType.Wood)
                    pathfinder.SetupDijkstra(grid, sourceCell, owner, ConsiderationConditionWood, TerminationConditionWood, true);
                else
                    pathfinder.SetupDijkstra(grid, sourceCell, owner, ConsiderationConditionStone, TerminationConditionStone, true);

                TimeSlicedPathManager.AddSearch(pathfinder);
            }
            else
            {
                if (resourceType == ResourceType.Wood)
                    path = pathfinder.SearchDijkstra(grid, sourceCell, owner, ConsiderationConditionWood, TerminationConditionWood, true);
                else if (resourceType == ResourceType.Stone)
                    path = pathfinder.SearchDijkstra(grid, sourceCell, owner, ConsiderationConditionStone, TerminationConditionStone, true);
                else
                    throw new Exception("Cannot fetch a path for None resource");

                _owner.targetResourceCell = grid.CellAt(path.Last().Mid);

                // TODO: This is a special case where pathIndex / FollowPath is necessary since currently it can't be implemented in terms of GetPathTo.
                // Consider reworking.
                FinalisePath();
            }            
        }

        public void GetPathToResource(Resource resource)
        {
            var sourceCell = grid.CellAt(owner.Mid);

            var targetCells = grid.CellsInRect(resource.CollisionRect.GetInflated(16, 16));
            targetCells = targetCells.Where(element => element.Passable).ToList();


            _owner.resrcLookingFor = resource.Type;
            _owner.targetResourceCell = grid.CellAt(resource.Mid);

            if (Debug.IsOn(DebugOp.EnableTimeSlicedPathing))
            {
                pathfinder.SetupGreedy(grid, sourceCell, targetCells, owner, GreedyConsiderationCondition, GreedyTerminationCondition, GreedyScoreMethod, true);
                TimeSlicedPathManager.AddSearch(pathfinder);
            }
            else
            {
                // TODO: This won't be able to find path if resource is saturated.
                path = pathfinder.SearchGreedy(grid, sourceCell, targetCells, owner, GreedyConsiderationCondition, GreedyTerminationCondition, GreedyScoreMethod, true);
            }            
        }

        /// <summary>
        /// Gets a Greedy path to the passed in building.
        /// Cheats by making all cells the building touches temporarily passable for searching.
        /// </summary>
        /// <param name="building"></param>
        public void GetPathToBuilding(Building building)
        {
            var sourceCell = grid.CellAt(owner.Mid);
            //var targetCell = new List<Cell> { grid.CellAt(building.Mid) };

            var inflatedBuildingCells = grid.CellsInRect(building.CollisionRect.GetInflated(16, 16));
            var buildingCells = grid.CellsInRect(building.CollisionRect);

            var borderCells = inflatedBuildingCells.Where(element => !buildingCells.Contains(element)).ToList();
            borderCells = borderCells.Where(element => element.Passable).ToList();

            if (Debug.IsOn(DebugOp.EnableTimeSlicedPathing))
            {
                pathfinder.SetupGreedy(grid, sourceCell, borderCells, owner, GreedyConsiderationCondition, GreedyTerminationCondition, GreedyScoreMethod, true);
                TimeSlicedPathManager.AddSearch(pathfinder);
            }
            else
            {
                // Get the path
                path = pathfinder.SearchGreedy(grid, sourceCell, borderCells, owner, GreedyConsiderationCondition, GreedyTerminationCondition, GreedyScoreMethod, true);
            }            
        }
    }
}
