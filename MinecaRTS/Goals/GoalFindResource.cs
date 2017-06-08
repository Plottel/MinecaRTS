using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    public class GoalFindResource : Goal<Worker>
    {
        public ResourceType Type;
        public Cell prevCoarseCell;
        public Cell curCoarseCell;
        public Cell resourceCell;

        public GoalFindResource(Worker owner, ResourceType resourceType) : base(owner)
        {
            Type = resourceType;
        }

        public override void Activate()
        {
            State = GoalState.Active;

            // Find a coarse grid cell that can't be seen
            var open = new List<Cell>();
            var closed = new List<Cell>();
            Cell current = owner.Data.CoarseGrid.CellAt(owner.Mid);

            open.Add(current);

            while (open.Count > 0)
            {
                current = open[0];

                foreach (Cell cell in current.Neighbours.OrderBy(a => World.rand.NextDouble()).ToList())
                {
                    if (!open.Contains(cell) && !closed.Contains(cell))
                    {
                        if (!owner.Data.HasExploredCoarseCell(cell))
                        {
                            owner.MoveTowards(cell.Mid);
                            return;                         
                        }

                        open.Add(cell);
                    }
                }

                open.Remove(current);
                closed.Add(current);
            }            
        }

        public override GoalState Process()
        {
            if (!owner.pathHandler.HasPath)
                Activate();

            prevCoarseCell = owner.Data.CoarseGrid.CellAt(owner.lastMid);
            curCoarseCell = owner.Data.CoarseGrid.CellAt(owner.Mid);

            if (prevCoarseCell != curCoarseCell)
            {
                int cellScale = owner.Data.CoarseGrid.CellSize / owner.Data.Grid.CellSize;

                // Check to see if we've found the resource.
                foreach (Point coarseIdx in owner.Data.CoarseGrid.Get33GridIndexesAroundPos(curCoarseCell.Mid))
                {
                    Cell coarseCell = owner.Data.CoarseGrid[coarseIdx];
                    Point coarseIdxInFineGrid = owner.Data.Grid.IndexAt(coarseCell.Pos);

                    for (int col = coarseIdxInFineGrid.Col(); col < coarseIdxInFineGrid.Col() + cellScale; col++)
                    {
                        for (int row = coarseIdxInFineGrid.Row(); row < coarseIdxInFineGrid.Row() + cellScale; row++)
                        {
                            Cell cell = owner.Data.Grid[col, row];
                            if (owner.Data.CellHasResource(cell))
                            {
                                if (owner.Data.GetResourceFromCell(cell).Type == Type)
                                {
                                    State = GoalState.Complete;
                                    resourceCell = cell;
                                    return State;
                                }
                                    
                            }
                        }
                    }
                }
            }

            return State;
        }

        public override void Terminate()
        {
        }

        public override void AddSubgoal(Goal<Worker> goal)
        {
        }
    }
}
