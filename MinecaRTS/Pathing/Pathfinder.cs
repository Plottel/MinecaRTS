using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace MinecaRTS
{
    public class Pathfinder
    {
        private SearchState searchState;
        public SearchType searchType;
        private Cell Source;
        private List<Cell> Targets;
        private Cell Current;
        private List<Cell> Open;
        private List<Cell> Closed;
        private Grid Grid;
        private Func<Cell, bool> considerationCondition;
        private Func<Cell, bool> dijkstraTerminationCondition;
        private Func<Cell, List<Cell>, bool> greedyTerminationCondition;
        private Func<Cell, Cell, float> greedyGetScore;
        public bool smoothed;
        private uint depthLimit;
        private uint currentDepth;
        private int smoothingFromIndex;
        private int smoothingToIndex;

        public List<Cell> path;
        private List<Cell> smoothedPath;


        private Dictionary<Cell, Cell> parents;
        private Dictionary<Cell, float> scores;

        public PathHandler handler;
        public Unit owner;

        public Pathfinder(Unit owner, PathHandler handler)
        {
            this.owner = owner;
            this.handler = handler;
            path = new List<Cell>();
        }

        private List<Cell> GetAdjacentCells(Cell cell)
        {
            var result = new List<Cell>();

            Point index = Grid.IndexAt(cell.Pos);

            result.Add(Grid[index.Col(), index.Row() - 1]); // N
            result.Add(Grid[index.Col(), index.Row() + 1]); // S
            result.Add(Grid[index.Col() + 1, index.Row()]); // E
            result.Add(Grid[index.Col() - 1, index.Row()]); // W    

            return result;
        }        

        private void GetNextCurrentCell()
        {
            // Get target if it's in the open.
            foreach (Cell c in Open)
            {
                if (Targets.Contains(c))
                {
                    Current = c;
                    return;
                }
            }            

            // TODO: Implement priority queue.
            //Open.Sort((x, y) => x.Score.CompareTo(y.Score));
            Open.Sort((x, y) => scores[x].CompareTo(scores[y]));

            //if (Open.Contains(Current))
                //Open.Remove(Current);

           // if (!Closed.Contains(Current))
                //Closed.Add(Current);

            // List is sorted therefore lowest score is first index.
            Current = Open[0];
        }

        public SearchState SingleIterationDijkstra()
        {
            // If we've reached maximum depth, end the search
            if (++currentDepth > depthLimit)
                return SearchState.Failed;

            #region /--- RESOURCE PATH CALC DEBUG ---\
            if (Debug.IsOn(DebugOp.CalcPath))
            {
                Input.UpdateStates();
                MinecaRTS.Instance.GraphicsDevice.Clear(Color.Gray);
                Debug.HandleInput();

                MinecaRTS.Instance.spriteBatch.Begin();

                MinecaRTS.Instance.world.Render(MinecaRTS.Instance.spriteBatch);

                // Render closed list pale blue.
                foreach (Cell cell in Closed)
                    MinecaRTS.Instance.spriteBatch.FillRectangle(cell.RenderRect, Color.LightSteelBlue);

                // Render open list cream.
                foreach (Cell cell in Open)
                    MinecaRTS.Instance.spriteBatch.FillRectangle(cell.RenderRect, Color.LightGoldenrodYellow);

                // Render source red
                MinecaRTS.Instance.spriteBatch.FillRectangle(Source.RenderRect, Color.Red);

                // Render current purple
                MinecaRTS.Instance.spriteBatch.FillRectangle(Current.RenderRect, Color.Purple);

                // Render path to current pink
                Cell c = Current;
                while (c.Parent != null)
                {
                    MinecaRTS.Instance.spriteBatch.FillRectangle(c.Parent.RenderRect, Color.Pink);
                    c = c.Parent;
                }

                // Render cell scores in black text.
                foreach (Cell openCell in Open)
                {
                    MinecaRTS.Instance.spriteBatch.DrawString(Debug.debugFont, Math.Floor(scores[openCell]).ToString(), openCell.RenderMid, Color.Black);
                }

                // Render each resource
                foreach (Resource r in MinecaRTS.Instance.world.Resources.Values)
                    r.Render(MinecaRTS.Instance.spriteBatch);

                Debug.RenderDebugOptionStates(MinecaRTS.Instance.spriteBatch);

                MinecaRTS.Instance.spriteBatch.End();

                MinecaRTS.Instance.GraphicsDevice.Present();

                System.Threading.Thread.Sleep(50);
            }

            #endregion /--- RESOURCE PATH CALC DEBUG ---\

            // Get adjacent nodes, calculate score and add to open list.
            foreach (Cell cell in Current.Neighbours)
            {
                if (considerationCondition(cell) && !Closed.Contains(cell) && !Open.Contains(cell))
                {
                    parents.Add(cell, Current);
                    scores.Add(cell, scores[Current] + 1);
                    //cell.Parent = Current;
                    //cell.Score = cell.Parent.Score + 1; // 1 minimum cost.

                    Open.Add(cell);

                    // Check if new node meets termination condition.
                    if (dijkstraTerminationCondition(cell))
                    {
                        Current = cell;
                        return SearchState.Complete;
                    }
                }
            }

            // Get cell with lowest F score ready to add neighbours.
            Open.Remove(Current);
            Closed.Add(Current);
            GetNextCurrentCell();
            return SearchState.Incomplete;
        }

        // TODO: MESSY
        public SearchState SingleIteration()
        {
            // If we still need to search, run an iteration.
            if (searchState == SearchState.Incomplete)
            {
                if (searchType == SearchType.Greedy)
                    searchState = SingleIterationGreedy();
                else if (searchType == SearchType.Dijkstra)
                    searchState = SingleIterationDijkstra();

                // If just completed iteration finished the path, retrace.
                if (searchState == SearchState.Complete)
                {
                    SetupRetracePath();
                    searchState = SearchState.Retracing;
                }
                else if (searchState == SearchState.Failed)
                    return SearchState.Failed;
                    
            }

            if (searchState == SearchState.Retracing)
            {
                if (SingleIterationRetracePath())
                {
                    if (this.smoothed)
                    {
                        SetupSmoothPath();
                        searchState = SearchState.Smoothing;
                    }
                }
            }

            // If we've finished searching but need to smooth, then smooth.
            if (searchState == SearchState.Smoothing)
            {
                bool smoothingFinished = SingleIterationSmoothPath();

                if (smoothingFinished)
                {
                    // Add the destination cell.
                    smoothedPath.Add(path[path.Count - 1]);
                    path = smoothedPath; 

                    return SearchState.Complete;
                }                    

                return SearchState.Incomplete;
            }

            return searchState;
        }

        /// <summary>
        /// Runs Dijkstra's to find the closest cell with a resource of the specified type.
        /// </summary>
        public List<Cell> SearchDijkstra(Grid grid, 
                                                Cell source, 
                                                Unit unit, 
                                                Func<Cell, bool> considerationCondition, 
                                                Func<Cell, bool> terminationCondition, 
                                                bool smoothed = false, 
                                                uint depthLimit = uint.MaxValue)
        {
            this.considerationCondition = considerationCondition;
            dijkstraTerminationCondition = terminationCondition;
            this.depthLimit = depthLimit;

            // Initialize relevant search details and add first node to closed list.
            Setup(grid, source);
            Targets = new List<Cell>();

            parents = new Dictionary<Cell, Cell>();
            scores = new Dictionary<Cell, float>();

            scores.Add(Current, 0);

            currentDepth = 0;

            searchState = SearchState.Incomplete;

            // Until we are considering a node with the desired resource that is not overcrowded.
            while (searchState == SearchState.Incomplete)
                searchState = SingleIterationDijkstra();

            if (searchState == SearchState.Failed)
                return new List<Cell>();

            // Target has been found. Retrace our steps to find path.
            path = RetracePath();

            // Don't smooth super short paths.
            if (smoothed && path.Count > 2)
                path = SmoothPath();

            return path;
        }

        public SearchState SingleIterationGreedy()
        {        
            #region /--- GREEDY PATH CALC DEBUG ---\
            if (Debug.IsOn(DebugOp.CalcPath))
            {
                Input.UpdateStates();
                MinecaRTS.Instance.GraphicsDevice.Clear(Color.Gray);
                Debug.HandleInput();

                MinecaRTS.Instance.spriteBatch.Begin();

                MinecaRTS.Instance.world.Render(MinecaRTS.Instance.spriteBatch);

                // Render closed list pale blue.
                foreach (Cell cell in Closed)
                    MinecaRTS.Instance.spriteBatch.FillRectangle(cell.RenderRect, Color.LightSteelBlue);

                // Render open list cream.
                foreach (Cell cell in Open)
                    MinecaRTS.Instance.spriteBatch.FillRectangle(cell.RenderRect, Color.LightGoldenrodYellow);

                // Render target green
                foreach (Cell target in Targets)
                    MinecaRTS.Instance.spriteBatch.FillRectangle(target.RenderRect, Color.LawnGreen);

                // Render source red
                MinecaRTS.Instance.spriteBatch.FillRectangle(Source.RenderRect, Color.Red);

                // Render current purple
                MinecaRTS.Instance.spriteBatch.FillRectangle(Current.RenderRect, Color.Purple);

                // Render path to current pink
                Cell c = Current;
                while (c.Parent != null)
                {
                    MinecaRTS.Instance.spriteBatch.FillRectangle(c.Parent.RenderRect, Color.Pink);
                    c = c.Parent;
                }

                // Render cell scores in black text.
                foreach (Cell openCell in Open)
                {
                    MinecaRTS.Instance.spriteBatch.DrawString(Debug.debugFont, Math.Floor(scores[openCell]).ToString(), openCell.RenderMid, Color.Black);
                }

                Debug.RenderDebugOptionStates(MinecaRTS.Instance.spriteBatch);

                MinecaRTS.Instance.spriteBatch.End();

                MinecaRTS.Instance.GraphicsDevice.Present();

                System.Threading.Thread.Sleep(50);
            }

            #endregion /--- GREEDY PATH CALC DEBUG ---\

            // Get adjacent nodes, calculate score and add to open list.
            // foreach (Cell cell in GetAdjacentCells(Current))
            foreach (Cell cell in Current.Neighbours)
            {
                if (considerationCondition(cell) && !Closed.Contains(cell) && !Open.Contains(cell))
                {
                    parents.Add(cell, Current);
                    scores.Add(cell, greedyGetScore(cell, Targets[0]));

                    //cell.Parent = Current;
                    //cell.Score = getScore(cell, Target);

                    Open.Add(cell);
                }
            }

            // Current vs last current
            Open.Remove(Current);
            Closed.Add(Current);

            if (greedyTerminationCondition(Current, Targets))
                return SearchState.Complete;

            if (Open.Count == 0)
                return SearchState.Failed;

            // Neighbours have been added, evaluate best node.
            GetNextCurrentCell();

            return SearchState.Incomplete;
        }

        public List<Cell> SearchGreedy(Grid grid, 
                                              Cell source, 
                                              List<Cell> target, 
                                              Unit unit, 
                                              Func<Cell, bool> considerationCondition, 
                                              Func<Cell, List<Cell>, bool> terminationCondition,
                                              Func<Cell, Cell, float> getScore,
                                              bool smoothed = false)
        {
            parents = new Dictionary<Cell, Cell>();
            scores = new Dictionary<Cell, float>();

            this.considerationCondition = considerationCondition;
            greedyTerminationCondition = terminationCondition;
            greedyGetScore = getScore;
            this.smoothed = smoothed;
            this.depthLimit = uint.MaxValue;
            currentDepth = 0;

            Targets = target;

            // Don't fetch a path to the same cell.
            if (Targets.Contains(source))
                return new List<Cell>();

            // Initialise relevant search details and add first node to closed list. 
            Setup(grid, source);

            parents.Add(Current, source);
            scores.Add(Current, 0);

            searchState = SearchState.Incomplete;

            // Until we consider the target node.
            while (searchState == SearchState.Incomplete)
                searchState = SingleIterationGreedy();

            // Search has finished.
            if (searchState == SearchState.Failed)
                return new List<Cell>();

            // Target has been found. Retrace our steps to find path.
            path = RetracePath();

            // Don't smooth super short paths.
            if (smoothed && path.Count > 2)
                path = SmoothPath();

            return path;
        }

        /// <summary>
        /// True indicates smoothing is finished.
        /// False indicates smoothing is still underway.
        /// </summary>
        /// <returns></returns>
        public bool SingleIterationSmoothPath()
        {
            if (smoothingToIndex > path.Count - 1)
                return true;

            if (PathIsClear(path[smoothingFromIndex], path[smoothingToIndex], handler.Owner))
            {
                ++smoothingToIndex;
            }
            else
            {
                smoothingFromIndex = smoothingToIndex - 1;
                smoothedPath.Add(path[smoothingFromIndex]);
                smoothingToIndex = smoothingFromIndex + 1;
            }

            return false;
        }

        private void SetupSmoothPath()
        {
            smoothedPath = new List<Cell>();
            smoothedPath.Add(path[0]);

            smoothingFromIndex = 0;
            smoothingToIndex = 1;
        }
        
        public List<Cell> SmoothPath()
        {
            SetupSmoothPath();

            // TODO: Can optimise if we're traveling on same angle by only calculating NEW portion.
            //float prevAngle = (path[smoothingFromIndex].Mid - path[smoothingToIndex].Mid).ToAngle();
            //float currentAngle = (path[smoothingFromIndex].Mid - path[smoothingToIndex].Mid).ToAngle();

            bool smoothingFinished = false;
            while (!smoothingFinished)
            {
                smoothingFinished = SingleIterationSmoothPath();
                #region Angle Optimisation
                //bool pathIsClear;

                //prevAngle = currentAngle;
                //currentAngle = (path[smoothingFromIndex].Mid - path[smoothingToIndex].Mid).ToAngle();

                // TODO: Is this worth??
                // If angle is same as last calculation, only check if new section is free.
                //if (prevAngle == currentAngle)
                //pathIsClear = PathIsClear(path[indexTo - 1], path[indexTo], unit);               
                //else // Different angle.
                //pathIsClear = PathIsClear(path[smoothingFromIndex], path[smoothingToIndex], unit);     
                #endregion Angle Optimisation
            }

            // Add the destination cell.
            smoothedPath.Add(path[path.Count - 1]);

            return smoothedPath;
        }

        public bool PathIsClear(Cell from, Cell to, Unit unit)
        {
            // TODO: Optimise - list potentially unnecessary.
            // Currently only gets top + bottom lines - need to fill in between.
            // Currently using rectangle bounding box rather than Entity bounding box.

            var cornerDistances = new List<float>();
            List<Point> toCorners = to.CollisionRect.GetInflated(-1, -1).GetCorners().ToList();

            // Loop through each corner.
            foreach (Point corner in toCorners)
                // Calculate distance to this corner.
                cornerDistances.Add(Vector2.Distance(from.Mid, corner.ToVector2()) +
                                    Vector2.Distance(to.Mid, corner.ToVector2()));

            int furthestCnrIdx = cornerDistances.IndexOf(cornerDistances.Max());

            // Get two corners to project.
            var cornerIdxs = new List<int>();

            // Corners are stored in order: TopLeft, TopRight, BottomRight, BottomLeft
            if (furthestCnrIdx == 1 || furthestCnrIdx == 3)
            {
                cornerIdxs.Add(0);
                cornerIdxs.Add(2);
            }
            else
            {
                cornerIdxs.Add(1);
                cornerIdxs.Add(3);
            }

            // Get all cells in the rectangle generated from the two line projections.
            var fromCorners = from.CollisionRect.GetInflated(-1, -1).GetCorners();

            Point from1 = fromCorners[cornerIdxs[0]];
            Point to1 = toCorners[cornerIdxs[0]];
            Point from2 = fromCorners[cornerIdxs[1]];
            Point to2 = toCorners[cornerIdxs[1]];

            HashSet<Cell> cellsInRect = Grid.CellsInRectFromLines(from1, to1, from2, to2);

            // @DEBUG: Show projected rectangle and adjacent nodes for path smoothing.
            #region /--- PATH SMOOTHING DEBUG ---\
            if (Debug.IsOn(DebugOp.CalcPathSmoothing))
            {
                // Finish rendering so we can start fresh render for debug
                MinecaRTS.Instance.spriteBatch.End();
                // TODO: Debug Handle Input in here not registering.
                Input.UpdateStates();
                Debug.HandleInput();

                MinecaRTS.Instance.GraphicsDevice.Clear(Color.Gray);

                MinecaRTS.Instance.spriteBatch.Begin();

                MinecaRTS.Instance.world.Render(MinecaRTS.Instance.spriteBatch);

                // Fill Cells the check rect is touching.
                foreach (Cell cell in cellsInRect)
                    MinecaRTS.Instance.spriteBatch.FillRectangle(cell.RenderRect, Color.DarkCyan);

                // Draw two cells being considered for smoothing.
                MinecaRTS.Instance.spriteBatch.FillRectangle(from.RenderRect, Color.Green);
                MinecaRTS.Instance.spriteBatch.FillRectangle(to.RenderRect, Color.Yellow);

                // Draw line between considered cells.
                MinecaRTS.Instance.spriteBatch.DrawLine(Camera.VecToScreen(from1.ToVector2()), Camera.VecToScreen(to1.ToVector2()), Color.Green, 2);
                MinecaRTS.Instance.spriteBatch.DrawLine(Camera.VecToScreen(from2.ToVector2()), Camera.VecToScreen(to2.ToVector2()), Color.Green, 2);

                Debug.RenderDebugOptionStates(MinecaRTS.Instance.spriteBatch);

                MinecaRTS.Instance.spriteBatch.End();

                MinecaRTS.Instance.GraphicsDevice.Present();

                System.Threading.Thread.Sleep(50);
            }                

            #endregion /--- PATH SMOOTHING DEBUG ---\

            foreach (Cell cell in cellsInRect)
            {
                if (!cell.Passable)
                    return false;
            }

            return true;
        }

        public bool SingleIterationRetracePath()
        {
            path.Insert(0, parents[Current]);
            Current = parents[Current];

            return parents[Current] == Source;
        }

        public void SetupRetracePath()
        {
            path = new List<Cell>();
            path.Add(Current);
        }

        public List<Cell> RetracePath()
        {
            SetupRetracePath();

            // Retrace steps until we get back to source node.
            bool retraceFinished = false;
            while (!retraceFinished)
                retraceFinished = SingleIterationRetracePath();

            return path;
        }

        public void SetupDijkstra(Grid grid,
                                                Cell source,
                                                Unit unit,
                                                Func<Cell, bool> considerationCondition,
                                                Func<Cell, bool> terminationCondition,
                                                bool smoothed = false,
                                                uint depthLimit = uint.MaxValue)
        {
            Source = source;
            this.considerationCondition = considerationCondition;
            dijkstraTerminationCondition = terminationCondition;
            this.smoothed = smoothed;
            this.depthLimit = depthLimit;
            currentDepth = 0;
            searchType = SearchType.Dijkstra;
            searchState = SearchState.Incomplete;

            path = new List<Cell>();

            Grid = grid;
            parents = new Dictionary<Cell, Cell>();
            scores = new Dictionary<Cell, float>();

            Source = source;
            Targets = new List<Cell>();
            Current = source;

            parents.Add(Current, null);
            scores.Add(Current, 0);

            Open = new List<Cell>();
            Closed = new List<Cell>();

            Open.Add(Current);

        }

        public void SetupGreedy(Grid grid,
                                              Cell source,
                                              List<Cell> targets,
                                              Unit unit,
                                              Func<Cell, bool> considerationCondition,
                                              Func<Cell, List<Cell>, bool> terminationCondition,
                                              Func<Cell, Cell, float> getScore,
                                              bool smoothed = false)
        {
            Grid = grid;
            parents = new Dictionary<Cell, Cell>();
            scores = new Dictionary<Cell, float>();

            searchState = SearchState.Incomplete;

            Source = source;
            Targets = targets;
            Current = source;

            Open = new List<Cell>();
            Closed = new List<Cell>();

            Open.Add(Current);

            path = new List<Cell>();

            parents.Add(Current, null);
            scores.Add(Current, 0);

            this.considerationCondition = considerationCondition;
            greedyTerminationCondition = terminationCondition;
            greedyGetScore = getScore;

            this.smoothed = smoothed;
            currentDepth = 0;

            searchType = SearchType.Greedy;
        }

        private void Setup(Grid grid, Cell source)
        {
            Grid = grid;
            Source = source;
            Current = source;
            path = new List<Cell>();

            Open = new List<Cell>();
            Closed = new List<Cell>();

            Open.Add(Current);
            //Closed.Add(Current);
            Current.Parent = null;
        }
    }
}