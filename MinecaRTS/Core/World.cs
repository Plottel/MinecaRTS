using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MinecaRTS
{
    /// <summary>
    /// The main Game Controller. Co-ordinates the game.
    /// </summary>
    public class World : IHandleMessages
    {
        /// <summary>
        /// Centralized RNG for the game.
        /// </summary>
        public static Random rand = new Random();

        /// <summary>
        /// Current game time - measured in ticks (NOT DELTA TIME).
        /// </summary>
        private static ulong _gameTime = 0;

        /// <summary>
        /// Gets the current game time - measured in ticks (NOT DELTA TIME).
        /// </summary>
        public static ulong GameTime
        {
            get { return _gameTime; }
        }

        /// <summary>
        /// World will always be created first and we may want to send messages to the world from anywhere.
        /// Therefore its ID is globally accessible.
        /// </summary>
        public const int MSG_ID = 0;

        /// <summary>
        /// The Messaging ID.
        /// </summary>
        private ulong _id;

        /// <summary>
        /// Gets the Messaging ID.
        /// </summary>
        public ulong ID
        {
            get { return _id; }
        }

        /// <summary>
        /// Width of the world.
        /// </summary>
        public static int Width;

        /// <summary>
        /// Height of the world.
        /// </summary>
        public static int Height;

        /// <summary>
        /// Stores cost values for all entity types.
        /// </summary>
        public static Dictionary<Type, Cost> entityCosts = new Dictionary<Type, Cost>();

        /// <summary>
        /// All entities which need to be rendered with respect to a Z-index.
        /// Sorted each frame by Y-position to create Z-index.
        /// </summary>
        private List<IRenderable> _renderables = new List<IRenderable>();

        /// <summary>
        /// The fine grid.
        /// </summary>
        public readonly Grid Grid;

        /// <summary>
        /// The coarse grid.
        /// </summary>
        public readonly Grid CoarseGrid;

        /// <summary>
        /// All units in the game.
        /// </summary>
        public readonly List<Unit> Units;

        /// <summary>
        /// All buildings in the game.
        /// </summary>
        public readonly List<Building> Buildings;

        /// <summary>
        /// Only the units which are currently selected.
        /// </summary>
        public List<Unit> SelectedUnits;

        /// <summary>
        /// Maps all resources in the game to the cell they reside in.
        /// </summary>
        public readonly Dictionary<Cell, Resource> Resources;

        /// <summary>
        /// Maps all tracks in the game to the cell they reside in.
        /// </summary>
        public readonly Dictionary<Cell, Track> Tracks;

        /// <summary>
        /// The Player class for player one.
        /// </summary>
        private Player _playerOne;

        /// <summary>
        /// The PlayerData for player one.
        /// </summary>
        private PlayerData _playerOneData;

        /// <summary>
        /// The collision cells for spatial partitioning.
        /// </summary>
        public readonly CollisionCellData collisionCells;

        /// <summary>
        /// The fog of war data. Uses coarse grid.
        /// </summary>
        public readonly FogOfWarData fogOfWar;

        /// <summary>
        /// Influence data for player one. Uses coarse grid.
        /// </summary>
        public readonly InfluenceMapData playerOneInfluence;

        /// <summary>
        /// Influence data for player two. Uses coarse grid.
        /// </summary>
        public readonly InfluenceMapData playerTwoInfluence;

        /// <summary>
        /// Initializes a new World.
        /// </summary>
        public World()
        {
            // Setup messaging.
            _id = MsgHandlerRegistry.NextID;
            MsgHandlerRegistry.Register(this);

            // Initialize grids.
            Grid = new Grid(new Vector2(0, 0), 100, 100, 32);
            CoarseGrid = new Grid(new Vector2(0, 0), 20, 20, 160);
            Grid.MakeBorder();

            // Setup spatial partitioning.
            collisionCells = new CollisionCellData(CoarseGrid);

            // Setup Fog Of War.
            fogOfWar = new FogOfWarData(CoarseGrid, collisionCells, this);

            // Set dimensions.
            Width = Grid.Width;
            Height = Grid.Height;

            // Initialize master lists.
            Units = new List<Unit>();
            Buildings = new List<Building>();
            Resources = new Dictionary<Cell, Resource>();
            Tracks = new Dictionary<Cell, Track>();
            SelectedUnits = new List<Unit>();

            // Setup influence maps.
            playerOneInfluence = new InfluenceMapData(Grid);
            playerTwoInfluence = new InfluenceMapData(Grid);

            // Setup players.
            _playerOneData = new PlayerData(this, Team.One);
            _playerOne = new HumanPlayer(_playerOneData);
            //_playerOne = new MinecartO(_playerOneData);            
        }

        /// <summary>
        /// Sets up the simulation to run the bot for demonstration purposes.
        /// </summary>
        public void SetupSimulation()
        {
            Unit u = new Worker(_playerOneData, Team.One, new Vector2(1000, 800), new Vector2(26, 35));
            fogOfWar.UnitAdded(u);
            collisionCells.AddUnit(u);
            playerOneInfluence.InfluencerAdded(u);
            Units.Add(u);
            _renderables.Add(u);
        }

        /// <summary>
        /// Defines how the World should handle messages.
        /// The world can also forward messages through to Players where necessary.
        /// </summary>
        /// <param name="message">The message</param>
        public void HandleMessage(Message message)
        {
            switch (message.type)
            {
                case MessageType.ResourceDepleted:
                    Resource resource = message.sender as Resource;
                    RemoveResourceFromCell(Grid.CellAt(resource.Mid));
                    break;

                case MessageType.UnitSpawned:
                    ProductionBuilding pb = message.sender as ProductionBuilding;

                    if (pb.Team == Team.One)
                        MsgBoard.AddMessage(MsgBoard.SENDER_IRRELEVANT, _playerOne.ID, MessageType.ProductionBuildingTaskComplete, info: pb);


                    AddUnit(message.extraInfo, new Vector2(pb.Mid.X, pb.CollisionRect.Bottom), pb.Team, pb.rallyPoint);
                    break;

                case MessageType.UnitMoved:
                    Unit u = message.sender as Unit;

                    if (u.lastMid != u.Mid)
                    {
                        collisionCells.UnitMoved(u);
                        fogOfWar.UnitMoved(u);
                    }
                    break;

                case MessageType.SupplyChanged:
                    PlayerData playerData = message.sender as PlayerData;

                    if (playerData.Team == Team.One)
                        MsgBoard.AddMessage(MsgBoard.SENDER_IRRELEVANT, _playerOne.ID, MessageType.SupplyChanged);
                    break;

                case MessageType.BuildingComplete:
                    Building b = message.sender as Building;

                    if (b.Team == Team.One)
                        MsgBoard.AddMessage(MsgBoard.SENDER_IRRELEVANT, _playerOne.ID, MessageType.BuildingComplete, info: b);
                    break;

                case MessageType.ResourcesReceived:
                    PlayerData data = message.sender as PlayerData;

                    if (data.Team == Team.One)
                        MsgBoard.AddMessage(MsgBoard.SENDER_IRRELEVANT, _playerOne.ID, MessageType.ResourcesReceived);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// User input method for the world.
        /// </summary>
        public void HandleInput()
        {
            if (Input.KeyTyped(Keys.Space))
                SetupSimulation();

            _playerOne.HandleInput();

            // Cheat for demonstration purposes to setup buildings quickly.
            if (Input.KeyDown(Keys.F))
            {
                foreach (Building b in Buildings)
                    b.Construct();
            }

            if (Input.KeyDown(Keys.N))
                playerOneInfluence.CalculateInfluenceBorderAroundCell(Grid.CellAt(Buildings[0].Mid));

            // Move camera with arrow keys.
            if (Input.KeyDown(Keys.Left))
                Camera.MoveBy(-20, 0);
            if (Input.KeyDown(Keys.Right))
                Camera.MoveBy(20, 0);
            if (Input.KeyDown(Keys.Up))
                Camera.MoveBy(0, -20);
            if (Input.KeyDown(Keys.Down))
                Camera.MoveBy(0, 20);

            // Move camera if mouse is at edge of screen
            if (Input.MouseX <= 5)
                Camera.MoveBy(-20, 0);
            if (Input.MouseX >= Camera.WIDTH - 50)
                Camera.MoveBy(20, 0);
            if (Input.MouseY <= 5)
                Camera.MoveBy(0, -20);
            if (Input.MouseY >= Camera.HEIGHT - 50)
                Camera.MoveBy(0, 20);
        }

        /// <summary>
        /// Main update loop method for the world.
        /// </summary>
        public void Update()
        {
            ++_gameTime;

            foreach (Unit u in Units)
                u.Update();

            foreach (Building b in Buildings)
                b.Update();

            TimeSlicedPathManager.Update();
            MsgBoard.SendMessages();
        }        

        /// <summary>
        /// Adds a new unit of the passed in type to the game.
        /// Updates fog of war, collision cells and influence accordingly.
        /// Also issues a move command based on rally point.
        /// </summary>
        /// <param name="unitType">The unit type to create.</param>
        /// <param name="pos">The position to create the unit at.</param>
        /// <param name="team">The team the unit belongs to.</param>
        /// <param name="rallyPoint">Where the unit should move to upon creation.</param>
        public void AddUnit(Type unitType, Vector2 pos, Team team, Vector2 rallyPoint)
        {
            Unit u = null;

            if (unitType == typeof(Worker))
                u = new Worker(_playerOneData, team, pos, new Vector2(26, 35));
            else if (unitType == typeof(Minecart))
                u = new Minecart(_playerOneData, team, pos, new Vector2(40, 40));

            fogOfWar.UnitAdded(u);
            collisionCells.AddUnit(u);

            if (team == Team.One)
                playerOneInfluence.InfluencerAdded(u);
            else if (team == Team.Two)
                playerTwoInfluence.InfluencerAdded(u);

            u.MoveTowards(rallyPoint);

            Units.Add(u);
            _renderables.Add(u);

            // Send message to player to be interpreted by bot.
            MsgBoard.AddMessage(MsgBoard.SENDER_IRRELEVANT, _playerOne.ID, MessageType.UnitSpawned, info: u);
        }

        /// <summary>
        /// Adds the passed in building to the master list.
        /// Also updates collision cells, fog of war and influence accordingly.
        /// </summary>
        /// <param name="building">The building.</param>
        public void AddBuilding(Building building)
        {
            Buildings.Add(building);

            fogOfWar.BuildingAdded(building);

            Track t = building as Track;

            // Minecarts don't get added to the Renderable list since they're always on the bottom layer
            // and dont need to be filtered with Z-Index.
            if (t != null)
                Tracks.Add(Grid.CellAt(building.Mid), t);
            else
            {
                _renderables.Add(building);

                foreach (Cell cell in Grid.CellsInRect(building.CollisionRect))
                    cell.Passable = false;
            }

            if (building.Team == Team.One)
                playerOneInfluence.InfluencerAdded(building);
            else if (building.Team == Team.Two)
                playerTwoInfluence.InfluencerAdded(building);
        }

        /// <summary>
        /// Adds the passed in resource to the passed in cell.
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <param name="cell">The cell</param>
        public void AddResourceToCell(Resource resource, Cell cell)
        {
            if (cell.Passable)
            {
                cell.Passable = false;
                cell.Color = Color.Gray;
                // If cell already has a resource, overwrite to new resource
                if (CellHasResource(cell))
                    Resources[cell] = resource;
                else
                    Resources.Add(cell, resource);

                _renderables.Add(resource);
            }            
        }

        /// <summary>
        /// Removes the resource residing at the passed in cell from the game.
        /// </summary>
        /// <param name="cell">The cell</param>
        public void RemoveResourceFromCell(Cell cell)
        {
            if (CellHasResource(cell))
            {
                Resource resource = Resources[cell];

                _renderables.Remove(Resources[cell]);
                Resources.Remove(cell);

                cell.Passable = true;

                foreach (Worker w in resource.Harvesters)
                    w.FSM.ChangeState(MoveToResource.Instance);
            }            
        }

        /// <summary>
        /// Returns whether or not a resource resides at the passed in cell.
        /// </summary>
        /// <param name="cell">The cell</param>
        public bool CellHasResource(Cell cell)
        {
            return Resources.ContainsKey(cell);
        }

        /// <summary>
        /// Returns whether or not a track resides at the passed in cell.
        /// </summary>
        /// <param name="cell">The cell</param>
        public bool CellHasTrack(Cell cell)
        {
            return Tracks.ContainsKey(cell);
        }

        /// <summary>
        /// Returns the resource at the passed in cell.
        /// Returns null if no resource exists there.
        /// </summary>
        /// <param name="cell">The cell.</param>
        public Resource GetResourceFromCell(Cell cell)
        {
            if (cell == null)
                return null;

            if (Resources.ContainsKey(cell))
                return Resources[cell];
            else
                return null;
        }

        /// <summary>
        /// Returns the track at the passed in cell.
        /// Returns null if no track exists there.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public Track GetTrackFromCell(Cell cell)
        {
            if (cell == null)
                return null;

            if (Tracks.ContainsKey(cell))
                return Tracks[cell];
            else
                return null;
        }

        /// <summary>
        /// Render method for the World. Tells everything else in the game to render.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to render to.</param>
        public void Render(SpriteBatch spriteBatch)
        {
            // TODO: Only render what's on the screen.
            Grid.Render(spriteBatch);

            CoarseGrid.Render(spriteBatch);

            foreach (Track t in Tracks.Values)
                t.Render(spriteBatch);

            // Filter by Y to create Z-index
            _renderables = _renderables.OrderBy(renderable => renderable.RenderRect.Y).ToList();

            if (Debug.IsOn(DebugOp.ShowFogOfWar))
            {
                var neutralItemsToRenderRegardlessOfFog = new List<IRenderable>();

                foreach (IRenderable r in _renderables)
                {
                    Point cellIndex = CoarseGrid.IndexAt(r.Mid);

                    Resource resource = r as Resource;

                    if (resource != null)
                    {
                        if (fogOfWar.TeamHasExploredCell(Team.One, cellIndex))
                            neutralItemsToRenderRegardlessOfFog.Add(r);
                    }
                    else
                    {
                        if (fogOfWar.TeamCanSeeCell(Team.One, cellIndex))
                            r.Render(spriteBatch);
                    }
                }

                for (int col = 0; col < CoarseGrid.Cols; col++)
                {
                    for (int row = 0; row < CoarseGrid.Rows; row++)
                    {
                        if (!fogOfWar.TeamCanSeeCell(Team.One, col, row))
                            spriteBatch.FillRectangle(CoarseGrid[col, row].RenderRect, Color.Black);
                    }
                }

                // Render explored neutral items after fog
                foreach (IRenderable r in neutralItemsToRenderRegardlessOfFog)
                    r.Render(spriteBatch);
            }
            else
            {
                // Just render renderables regardless of vision.
                foreach (IRenderable r in _renderables)
                    r.Render(spriteBatch);
            }

            _playerOneData.Render(spriteBatch);
            _playerOneData.RenderUI(spriteBatch);
            _playerOne.Render(spriteBatch);
        }

        /// <summary>
        /// Renders debug information for the game.
        /// Filters what is rendered based on currently active debug options.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to render to.</param>
        public void RenderDebug(SpriteBatch spriteBatch)
        {
            // TODO: A lot of this can be put inside Unit Classes which would remove
            // a bunch of the "lol everything's public".
            Grid.ShowGrid = Debug.IsOn(DebugOp.ShowGrid) || MinecaRTS.Instance.editMode;
            CoarseGrid.ShowGrid = Debug.IsOn(DebugOp.ShowCoarseGrid);

            if (Debug.IsOn(DebugOp.ShowPaths))
            {
                foreach (Unit u in Units)
                    u.pathHandler.RenderPath(spriteBatch);
            }

            if (Debug.IsOn(DebugOp.ShowUnitFeelers))
            {
                foreach (Unit u in Units)
                {
                    SteeringBehaviours s = u.Steering;
                    spriteBatch.DrawLine(u.RenderMid, Camera.VecToScreen(s.centreFeelerEnd), Color.GreenYellow, 1);
                    spriteBatch.DrawLine(u.RenderMid, Camera.VecToScreen(s.leftFeelerEnd), Color.GreenYellow, 1);
                    spriteBatch.DrawLine(u.RenderMid, Camera.VecToScreen(s.rightFeelerEnd), Color.GreenYellow, 1);
                }
            }

            if (Debug.IsOn(DebugOp.ShowWallPushForce))
            {
                foreach (Unit u in Units)
                {
                    if (u.Steering.closestUnpassableCellMid != Vector2.Zero)
                    {
                        Vector2 wallPushForce = u.Steering.wallPushForce;
                        spriteBatch.DrawLine(Camera.VecToScreen(u.Steering.closestUnpassableCellMid), 
                                                Camera.VecToScreen(u.Steering.closestUnpassableCellMid + (wallPushForce * 100)), 
                                                Color.Red, 
                                                3);
                    }                    
                }
            }

            if (Debug.IsOn(DebugOp.ShowStates))
            {
                foreach (Unit u in Units)
                {
                    u.RenderDebug(spriteBatch);
                }
            }

            if (Debug.IsOn(DebugOp.ShowCoarseGrid))
            {
                //List<List<HashSet<Unit>>> collisionCells;
                for (int col = 0; col < collisionCells.Grid.Cols; col++)
                {
                    for (int row = 0; row < collisionCells.Grid.Rows; row++)
                    {
                        HashSet<Unit> units = collisionCells[col, row];

                        spriteBatch.DrawString(MinecaRTS.largeFont, units.Count.ToString(), CoarseGrid[col, row].RenderMid, Color.White);
                    }
                }
            }

            if (Debug.IsOn(DebugOp.ShowInfluence))
            {
                playerOneInfluence.Render(spriteBatch);
            }
        }
    }
}
