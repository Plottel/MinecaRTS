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
        private Cell Source;
        private Cell Target;
        private Cell Current;
        private List<Cell> Open;
        private List<Cell> Closed;
        private Grid Grid;
        private Func<Cell, bool> considerationCondition;
        private Func<Cell, bool> dijkstraTerminationCondition;
        private Func<Cell, Cell, bool> greedyTerminationCondition;
        private Func<Cell, Cell, float> greedyGetScore;
        public bool smoothed;
        private uint depthLimit;
        private uint currentDepth;

        private Dictionary<Cell, Cell> parents;
        private Dictionary<Cell, float> scores;

        public PathHandler handler;
        public Unit owner;

        public Pathfinder(Unit owner, PathHandler handler)
        {
            this.owner = owner;
            this.handler = handler;
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
                if (c == Target)
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
            // Initialize relevant search details and add first node to closed list.
            Setup(grid, source);
            Target = null;

            parents = new Dictionary<Cell, Cell>();
            scores = new Dictionary<Cell, float>();

            scores.Add(Current, 0);

            uint searchDepth = 0;
            bool searchComplete = false;

            // Until we are considering a node with the desired resource that is not overcrowded.
            while (!searchComplete)
            {
                // If we've reached maximum depth, end the search
                if (++searchDepth > depthLimit)
                    break;

                #region /--- RESOURCE PATH CALC DEBUG ---\
                if (Debug.OptionActive(DebugOption.CalcPath))
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
                        if (terminationCondition(cell))
                        {
                            Current = cell;
                            searchComplete = true;
                            break;
                        }
                    }
                }

                // Get cell with lowest F score ready to add neighbours.
                if (!searchComplete)
                {
                    Open.Remove(Current);
                    Closed.Add(Current);
                    GetNextCurrentCell();
                }
            }

            if (searchComplete)
            {
                // Target has been found. Retrace our steps to find path.
                var path = RetracePath();

                // Don't smooth super short paths.
                if (smoothed && path.Count > 2)
                    path = SmoothPath(unit, path);

                return path;
            }
            else
            {
                // We reached depth limit before target was found, return empty list;
                return new List<Cell>();
            }
        }

        public SearchState SingleIterationGreedy()
        {        
            #region /--- GREEDY PATH CALC DEBUG ---\
            if (Debug.OptionActive(DebugOption.CalcPath))
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
                MinecaRTS.Instance.spriteBatch.FillRectangle(Target.RenderRect, Color.LawnGreen);

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
                    scores.Add(cell, greedyGetScore(cell, Target));

                    //cell.Parent = Current;
                    //cell.Score = getScore(cell, Target);

                    Open.Add(cell);
                }
            }

            // Current vs last current
            Open.Remove(Current);
            Closed.Add(Current);

            if (greedyTerminationCondition(Current, Target))
                return SearchState.Complete;

            if (Open.Count == 0)
                return SearchState.Failed;

            // Neighbours have been added, evaluate best node.
            GetNextCurrentCell();

            return SearchState.Searching;
        }

        public List<Cell> SearchGreedy(Grid grid, 
                                              Cell source, 
                                              Cell target, 
                                              Unit unit, 
                                              Func<Cell, bool> considerationCondition, 
                                              Func<Cell, Cell, bool> terminationCondition,
                                              Func<Cell, Cell, float> getScore,
                                              bool smoothed = false)
        {
            parents = new Dictionary<Cell, Cell>();
            scores = new Dictionary<Cell, float>();

            // Don't fetch a path to the same cell.
            if (source == target)
                return new List<Cell>();

            // Initialise relevant search details and add first node to closed list. 
            Setup(grid, source);
            Target = target;

            parents.Add(Current, source);
            scores.Add(Current, 0);

            SearchState searchState = SearchState.Searching;

            // Until we consider the target node.
            while (searchState == SearchState.Searching)
                searchState = SingleIterationGreedy();

            // Search has finished.
            if (searchState == SearchState.Failed)
                return new List<Cell>();

            // Target has been found. Retrace our steps to find path.
            var path = RetracePath();

            // Don't smooth super short paths.
            if (smoothed && path.Count > 2)
                path = SmoothPath(unit, path);

            return path;
        }

        public List<Cell> SmoothPath(Unit unit, List<Cell> path)
        {
            var smoothedPath = new List<Cell>();
            smoothedPath.Add(path[0]);

            int indexFrom = 0;
            int indexTo = 1;

            float prevAngle = (path[indexFrom].Mid - path[indexTo].Mid).ToAngle();
            float currentAngle = (path[indexFrom].Mid - path[indexTo].Mid).ToAngle();

            while (indexTo < path.Count - 1)
            {
                bool pathIsClear;

                prevAngle = currentAngle;
                currentAngle = (path[indexFrom].Mid - path[indexTo].Mid).ToAngle();

                // TODO: Is this worth??
                // If angle is same as last calculation, only check if new section is free.
                //if (prevAngle == currentAngle)
                    //pathIsClear = PathIsClear(path[indexTo - 1], path[indexTo], unit);               
                //else // Different angle.
                    pathIsClear = PathIsClear(path[indexFrom], path[indexTo], unit);               

                if (pathIsClear)
                {
                    ++indexTo;
                }
                else
                {
                    indexFrom = indexTo - 1;
                    smoothedPath.Add(path[indexFrom]);
                    indexTo = indexFrom + 1;
                }
                    
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
            if (Debug.OptionActive(DebugOption.CalcPathSmoothing))
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

        public List<Cell> RetracePath()
        {
            var path = new List<Cell>();
            path.Add(Current);

            // Retrace steps until we get back to source node.
            while (parents[Current] != Source)
            {
                path.Insert(0, parents[Current]);
                Current = parents[Current];
            }

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
        }

        public void SetupGreedy(Grid grid,
                                              Cell source,
                                              Cell target,
                                              Unit unit,
                                              Func<Cell, bool> considerationCondition,
                                              Func<Cell, Cell, bool> terminationCondition,
                                              Func<Cell, Cell, float> getScore,
                                              bool smoothed = false)
        {
            Grid = grid;
            parents = new Dictionary<Cell, Cell>();
            scores = new Dictionary<Cell, float>();

            Source = source;
            Target = target;
            Current = source;

            Open = new List<Cell>();
            Closed = new List<Cell>();

            Open.Add(Current);

            parents.Add(Current, null);
            scores.Add(Current, 0);


            this.considerationCondition = considerationCondition;
            greedyTerminationCondition = terminationCondition;
            greedyGetScore = getScore;

            this.smoothed = smoothed;
            currentDepth = 0;
        }

        private void Setup(Grid grid, Cell source)
        {
            Grid = grid;
            Source = source;
            Current = source;

            Open = new List<Cell>();
            Closed = new List<Cell>();

            Open.Add(Current);
            //Closed.Add(Current);
            Current.Parent = null;
        }
    }
}