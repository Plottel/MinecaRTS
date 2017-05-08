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
    using CollisionCellMap = List<List<HashSet<Unit>>>;

    public class World : IHandleMessages
    {
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

        private HumanPlayer _playerOne;
        private PlayerData _playerOneData;

        public readonly CollisionCellMap collisionCells;

        public HashSet<Unit> GetUnitsInCollisionCellFromIndex(Point index)
        {
            if (index.Col() < 0 || index.Col() >= CoarseGrid.Cols || index.Row() < 0 || index.Row() >= CoarseGrid.Rows)
                return new HashSet<Unit>(); // Return blank list if out of range.
            else
                return collisionCells[index.Col()][index.Row()];
        }

        public HashSet<Unit> GetUnitsInCollisionCellFromIndex(int col, int row)
        {
            if (col < 0 || col >= CoarseGrid.Cols || row < 0 || row >= CoarseGrid.Rows)
                return new HashSet<Unit>(); // Return blank list if out of range.
            else
                return collisionCells[col][row];
        }

        public World()
        {
            _id = MsgHandlerRegistry.NextID;
            MsgHandlerRegistry.Register(this);

            Grid = new Grid(new Vector2(0, 0), 100, 100, 32);
            CoarseGrid = new Grid(new Vector2(0, 0), 20, 20, 160);

            collisionCells = new CollisionCellMap();

            for (int col = 0; col < 20; col++)
            {
                collisionCells.Add(new List<HashSet<Unit>>());
                for (int row = 0; row < 20; row++)
                {
                    collisionCells[col].Add(new HashSet<Unit>());
                }
            }                

            Grid.MakeBorder();

            Width = Grid.Width;
            Height = Grid.Height;

            Units = new List<Unit>();
            Buildings = new List<Building>();
            Resources = new Dictionary<Cell, Resource>();
            Tracks = new Dictionary<Cell, Track>();

            SelectedUnits = new List<Unit>();
            _playerOneData = new PlayerData(this, Team.One);
            _playerOne = new HumanPlayer(_playerOneData);
        }

        public void HandleMessage(Message message)
        {
            switch (message.type)
            {
                case MessageType.ResourceDepleted:
                        Resource resource = message.sender as Resource;
                        RemoveResourceFromCell(Grid.CellAt(resource.Mid));
                        break;

                default:
                    break;
            }
        }

        public void HandleInput()
        {
            _playerOne.HandleInput();

            if (Input.KeyDown(Keys.F))
            {
                foreach (Building b in Buildings)
                    b.Construct();
            }
        }

        public void Update()
        {
            foreach (Unit u in Units)
                u.Update();

            foreach (Building b in Buildings)
                b.Update();
        }

        public void Render(SpriteBatch spriteBatch)
        {
            Debug.HookText("Number of Units In Game: " + Units.Count.ToString());
            // TODO: Only render what's on the screen.
            Grid.Render(spriteBatch);

            CoarseGrid.Render(spriteBatch);

            foreach (Track t in Tracks.Values)
                t.Render(spriteBatch);

            // Filter by Y to create Z-index
            _renderables = _renderables.OrderBy(renderable => renderable.RenderRect.Y).ToList();

            // Render
            foreach (IRenderable r in _renderables)
                r.Render(spriteBatch);

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

            u.MoveTowards(rallyPoint);

             Units.Add(u);
            _renderables.Add(u);
        }

        public void AddBuilding(Building building)
        {
            Buildings.Add(building);

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
            Grid.ShowGrid = Debug.OptionActive(DebugOption.ShowGrid) || MinecaRTS.Instance.editMode;
            CoarseGrid.ShowGrid = Debug.OptionActive(DebugOption.ShowCoarseGrid);

            if (Debug.OptionActive(DebugOption.ShowPaths))
            {
                foreach (Unit u in Units)
                    u.pathHandler.RenderPath(spriteBatch);
            }

            if (Debug.OptionActive(DebugOption.ShowUnitFeelers))
            {
                foreach (Unit u in Units)
                {
                    SteeringBehaviours s = u.Steering;
                    spriteBatch.DrawLine(u.RenderMid, Camera.VecToScreen(s.centreFeelerEnd), Color.GreenYellow, 1);
                    spriteBatch.DrawLine(u.RenderMid, Camera.VecToScreen(s.leftFeelerEnd), Color.GreenYellow, 1);
                    spriteBatch.DrawLine(u.RenderMid, Camera.VecToScreen(s.rightFeelerEnd), Color.GreenYellow, 1);
                }
            }

            if (Debug.OptionActive(DebugOption.ShowWallPushForce))
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

            if (Debug.OptionActive(DebugOption.ShowStates))
            {
                foreach (Unit u in Units)
                {
                    u.RenderDebug(spriteBatch);
                }
            }

            if (Debug.OptionActive(DebugOption.ShowCoarseGrid))
            {
                //List<List<HashSet<Unit>>> collisionCells;
                for (int col = 0; col < collisionCells.Count; col++)
                {
                    for (int row = 0; row < collisionCells[col].Count; row++)
                    {
                        HashSet<Unit> units = collisionCells[col][row];

                        spriteBatch.DrawString(MinecaRTS.largeFont, units.Count.ToString(), CoarseGrid[col, row].RenderMid, Color.White);
                    }
                }
            }
        }
    }
}
