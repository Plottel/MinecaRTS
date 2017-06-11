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
    public class PathHandler : IHandleMessages
    {
        /// <summary>
        /// Which unit the PathHandler belongs to.
        /// </summary>
        protected Unit owner;

        public Unit Owner
        {
            get { return owner; }
        }

        private ulong _id;

        // TODO: JANKY!!
        protected Building buildingPathingTowards;

        public ulong ID
        {
            get { return _id; }
        }

        public Pathfinder pathfinder;

        /// <summary>
        /// The grid used for path handling.
        /// </summary>
        protected Grid grid;

        /// <summary>
        /// The current path.
        /// </summary>
        public List<Cell> path;

        /// <summary>
        /// The index in the path the unit is currently moving towards.
        /// </summary>
        protected int pathIndex;

        /// <summary>
        /// How close a unit has to be to a cell before it has "reached" it.
        /// </summary>
        private float _waypointThreshold;

        protected int estimatedTicksToReachNextCell;
        protected int ticksSpentTravellingToCell;

        public bool HasPath
        {
            get { return path.Count > 0; }
        }

        public Cell TargetCell
        {
            get { return path[pathIndex]; }
        }

        public PathHandler(Unit owner, Grid grid)
        {
            this.owner = owner;
            this.grid = grid;
            path = new List<Cell>();
            pathIndex = 0;
            _waypointThreshold = 5;
            pathfinder = new Pathfinder(owner, this);

            _id = MsgHandlerRegistry.NextID;
            MsgHandlerRegistry.Register(this);
        }

        public virtual void HandleMessage(Message message)
        {
            switch (message.type)
            {
                case MessageType.SearchComplete:
                    var searchState = message.extraInfo;

                    // Search has finished.
                    if (searchState == SearchState.Failed)
                        path = new List<Cell>();
                    else if (searchState == SearchState.Complete)
                    {
                        pathfinder.path.RemoveAt(0);
                        path = pathfinder.path;
                        FinalisePath();
                    }
                    break;
            }
        }

        /// <summary>
        /// Checks if owner has reached current cell index and, if it has, orients towards next cell.
        /// </summary>
        public Vector2 Update()
        {
            Vector2 force = Vector2.Zero;

            if (++ticksSpentTravellingToCell >= estimatedTicksToReachNextCell)
            {
                // Replan path
                GetPathTo(path.Last().Mid);
                return force;
            }

            // Re-orient owner towards target cell to keep on track.
            force = OrientTowardsCell(path[pathIndex]);

            if (ReachedCell(path[pathIndex]))
            {
                if (pathIndex < path.Count - 1)
                {
                    force = OrientTowardsCell(path[++pathIndex]);
                    ticksSpentTravellingToCell = 0;
                    estimatedTicksToReachNextCell = GetEstimatedTicksToReachCell(path[pathIndex]);
                }
                else
                {
                    owner.FollowPath = false;
                    path = new List<Cell>();
                    force = Vector2.Zero;
                }                   
            }

            return force;
        }

        public int GetEstimatedTicksToReachCell(Cell cell)
        {
            float distance = Vector2.Distance(cell.Mid, owner.Mid);

            int estimatedTicks = (int)((distance / owner.Speed) * 1.2);

            if (estimatedTicks < 10)
                estimatedTicks = 10;

            return estimatedTicks;
        }

        /// <summary>
        /// Adjusts owner's velocity to point towards the passed in cell.
        /// </summary>
        /// <param name="cell">The cell to orient towards.</param>
        private Vector2 OrientTowardsCell(Cell cell)
        {
            return Vector2.Normalize(cell.Mid - owner.Mid);
        }

        /// <summary>
        /// Specifies if owner is within the waypoint threshold for the pased in cell.
        /// </summary>
        /// <param name="cell">The cell to check if reached.</param>
        /// <returns></returns>
        private bool ReachedCell(Cell cell)
        {
            // TODO: Since it's all relative, consider LengthSq.
            var distanceFromCell = (cell.Mid - owner.Mid).Length();

            return distanceFromCell <= _waypointThreshold;
        }
        
        public void SetPath(List<Cell> path)
        {
            this.path = path;
            FinalisePath();
        }

        public void FinalisePath()
        {
            pathIndex = 0;

            if (path.Count > 0)
            {
                ticksSpentTravellingToCell = 0;
                estimatedTicksToReachNextCell = GetEstimatedTicksToReachCell(path[0]);
                owner.FollowPath = true;
            }
        }

        /// <summary>
        /// Generates a path to the target position.
        /// If owner is set to follow paths, this will orient owner towards first cell in path.
        /// </summary>
        /// <param name="targetPos"></param>
        public virtual void GetPathTo(Vector2 targetPos, bool smoothed = true)
        {
            var sourceCell = grid.CellAt(owner.Mid);
            var targetCell = new List<Cell> { grid.CellAt(targetPos) };

            
            if (Debug.IsOn(DebugOp.EnableTimeSlicedPathing))
            {
                pathfinder.SetupGreedy(grid, sourceCell, targetCell, owner, GreedyConsiderationCondition, GreedyTerminationCondition, GreedyScoreMethod, smoothed);

                path = new List<Cell> { grid.CellAt(targetPos) };
                FinalisePath();
                TimeSlicedPathManager.AddSearch(pathfinder);

            }
            else
            {
                path = pathfinder.SearchGreedy(grid, sourceCell, targetCell, owner, GreedyConsiderationCondition, GreedyTerminationCondition, GreedyScoreMethod, smoothed);
                FinalisePath();
                owner.ExitState();
            }                 
        }        

        public void RenderPath(SpriteBatch spriteBatch)
        {
            // Render path
            if (path.Count > 0)
            {
                spriteBatch.DrawLine(owner.RenderMid, path[pathIndex].RenderMid, Color.LightGreen);

                for (int i = pathIndex; i < path.Count - 1; i++)
                {
                    spriteBatch.DrawPoint(path[i].RenderMid, Color.Blue, 10);
                    spriteBatch.DrawLine(path[i].RenderMid, path[i + 1].RenderMid, Color.LightGreen);
                }

                spriteBatch.DrawPoint(path[path.Count - 1].RenderMid, Color.Blue, 10);
            }
        }

        #region Search Conditions
        protected float TrackScoreMethod(Cell cell, Cell Target)
        {
            float score = GreedyScoreMethod(cell, Target);

            Track t = owner.Data.GetTrackFromCell(cell);

            if (t == null || !t.IsActive)
                score *= 5;

            return score;
        }

        protected bool ConsiderationConditionWood(Cell cell)
        {
            if (cell.Passable)
                return true;

            Resource resource = owner.Data.GetResourceFromCell(cell);

            if (resource == null)
                return false;

            // Valid if resource is the correct type and not saturated.
            return resource.Type == ResourceType.Wood && !resource.IsSaturated;
        }

        protected bool ConsiderationConditionStone(Cell cell)
        {
            if (cell.Passable)
                return true;

            Resource resource = owner.Data.GetResourceFromCell(cell);

            if (resource == null)
                return false;

            // Valid if resource is the correct type and not saturated.
            return resource.Type == ResourceType.Stone && !resource.IsSaturated;
        }

       protected bool TerminationConditionWood(Cell current)
       {
            Resource resource = owner.Data.GetResourceFromCell(current);

            if (resource == null)
                return false;
            else if (resource.Type != ResourceType.Wood)
                return false;

            return !resource.IsSaturated;
        }

        protected bool TerminationConditionStone(Cell current)
        {
            Resource resource = owner.Data.GetResourceFromCell(current);

            if (resource == null)
                return false;
            else if (resource.Type != ResourceType.Stone)
                return false;

            return !resource.IsSaturated;
        }

        protected bool TerminationConditionFindTrack(Cell current)
        {
            return owner.Data.CellHasTrack(current);
        }

        // Consideration condition for a standard path.
        public bool GreedyConsiderationCondition(Cell cell)
        {
            return cell.Passable;
        }

        // Termination condition for a standard path.
        public  bool GreedyTerminationCondition(Cell current, List<Cell> targets)
        {
            return targets.Contains(current);
        }

        public float GreedyScoreMethod(Cell cell, Cell Target)
        {
            Point idxOne = grid.IndexAt(cell.Pos);
            Point idxTwo = grid.IndexAt(Target.Pos);

            int dx = Math.Abs(idxOne.Col() - idxTwo.Col());
            int dy = Math.Abs(idxOne.Row() - idxTwo.Row());

            return dx + dy;
        }
        #endregion Search Conditions
    }
}