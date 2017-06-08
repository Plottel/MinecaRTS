﻿using System;
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
    public class World : IHandleMessages
    {
        public static Random rand = new Random();
        private static ulong _gameTime = 0;

        public static ulong GameTime
        {
            get { return _gameTime; }
        }

        /// <summary>
        /// World will always be created first and we may want to send messages to the world from anywhere.
        /// Therefore its ID is globally accessible.
        /// </summary>
        public const int MSG_ID = 0;

        private ulong _id;

        public ulong ID
        {
            get { return _id; }
        }

        public static int Width;
        public static int Height;

        public static Dictionary<Type, Cost> entityCosts = new Dictionary<Type, Cost>();

        private List<IRenderable> _renderables = new List<IRenderable>();

        public readonly Grid Grid;
        public readonly Grid CoarseGrid;
        public readonly List<Unit> Units;
        public readonly List<Building> Buildings;
        public List<Unit> SelectedUnits;

        public readonly Dictionary<Cell, Resource> Resources;
        public readonly Dictionary<Cell, Track> Tracks;

        private Player _playerOne;
        private PlayerData _playerOneData;

        public readonly CollisionCellData collisionCells;
        public readonly FogOfWarData fogOfWar;

        public readonly InfluenceMapData playerOneInfluence;
        public readonly InfluenceMapData playerTwoInfluence;


        public World()
        {
            _id = MsgHandlerRegistry.NextID;
            MsgHandlerRegistry.Register(this);

            Grid = new Grid(new Vector2(0, 0), 100, 100, 32);
            CoarseGrid = new Grid(new Vector2(0, 0), 20, 20, 160);

            collisionCells = new CollisionCellData(CoarseGrid);
            fogOfWar = new FogOfWarData(CoarseGrid, collisionCells, this);

            Grid.MakeBorder();

            Width = Grid.Width;
            Height = Grid.Height;

            Units = new List<Unit>();
            Buildings = new List<Building>();
            Resources = new Dictionary<Cell, Resource>();
            Tracks = new Dictionary<Cell, Track>();

            playerOneInfluence = new InfluenceMapData(Grid);
            playerTwoInfluence = new InfluenceMapData(Grid);

            SelectedUnits = new List<Unit>();
            _playerOneData = new PlayerData(this, Team.One);
            //_playerOne = new HumanPlayer(_playerOneData);
            _playerOne = new MinecartO(_playerOneData);            
        }

        public void Setup()
        {
            AddBuilding(BuildingFactory.CreateTownHall(_playerOneData, new Vector2(320, 320)));

            while (!Buildings[0].IsActive)
                Buildings[0].Construct();

            TownHall townHall = Buildings[0] as TownHall;
            townHall.QueueUpProductionAtIndex(0);
        }

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

                default:
                    break;
            }
        }

        public void HandleInput()
        {
            if (Input.KeyTyped(Keys.Space))
                Setup();

            _playerOne.HandleInput();

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

        public bool TeamCanSeeCell(int c, int r, Team team)
        {
            for (int col = c - 1; col <= c + 1; col++)
            {
                for (int row = r - 1; row <= r + 1; row++)
                {
                    foreach (Unit u in collisionCells[col, row])
                    {
                        if (u.Team == team)
                            return true;
                    }
                }
            }

            Cell topLeftCell = CoarseGrid[c - 1, r - 1];

            Point topLeftPt;

            if (topLeftCell != null)
                topLeftPt = topLeftCell.Pos.ToPoint();
            else
                topLeftPt = new Point(0, 0);

            Rectangle boundingRect = new Rectangle(topLeftPt, new Point(CoarseGrid.CellSize * 3, CoarseGrid.CellSize * 3));

            foreach (Building b in Buildings)
            {
                if (b.CollisionRect.Intersects(boundingRect) && b.Team == team)
                    return true;
            }

            return false;
        }

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

        public bool CellHasResource(Cell cell)
        {
            return Resources.ContainsKey(cell);
        }

        public bool CellHasTrack(Cell cell)
        {
            return Tracks.ContainsKey(cell);
        }

        public Resource GetResourceFromCell(Cell cell)
        {
            if (cell == null)
                return null;

            if (Resources.ContainsKey(cell))
                return Resources[cell];
            else
                return null;
        }

        public Track GetTrackFromCell(Cell cell)
        {
            if (cell == null)
                return null;

            if (Tracks.ContainsKey(cell))
                return Tracks[cell];
            else
                return null;
        }

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
