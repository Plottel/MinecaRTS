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
    public class World
    {
        public static int WIDTH;
        public static int HEIGHT;

        public static Dictionary<Type, Cost> entityCosts = new Dictionary<Type, Cost>();

        public readonly Grid Grid;
        public readonly List<Unit> Units;
        public readonly List<Building> Buildings;
        public List<Unit> SelectedUnits;

        public readonly Dictionary<Cell, Resource> Resources;


        public HumanPlayer _playerOne;
        public PlayerData _playerOneData;

        public World()
        {
            Grid = new Grid(new Vector2(0, 0), 100, 100);
            Grid.MakeBorder();

            WIDTH = Grid.Width;
            HEIGHT = Grid.Height;

            Units = new List<Unit>();
            Buildings = new List<Building>();
            Resources = new Dictionary<Cell, Resource>();

            SelectedUnits = new List<Unit>();
            _playerOneData = new PlayerData(this, Team.One);
            _playerOne = new HumanPlayer(_playerOneData);
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
            // TODO: Only render what's on the screen.
            Grid.Render(spriteBatch);

            foreach (Building b in Buildings)
                b.Render(spriteBatch);

            foreach (Unit u in Units)
                u.Render(spriteBatch);

            foreach (Resource r in Resources.Values)
                r.Render(spriteBatch);

            _playerOneData.Render(spriteBatch);
            _playerOne.Render(spriteBatch);
        }

        public void AddUnit(Vector2 pos, Vector2 scale, Team team)
        {
            Units.Add(new Unit(_playerOneData, team, pos, scale));
        }

        public void AddWorker(Vector2 pos, Team team)
        {
            Units.Add(new Worker(_playerOneData, team, pos, new Vector2(26, 35)));
        }

        public void AddBuilding(Building building)
        {
            foreach (Cell cell in Grid.CellsInRect(building.CollisionRect))
                cell.Passable = false;

            Buildings.Add(building);
        }

        public void AddResourceToCell(Resource resource, Cell cell)
        {
            // If cell already has a resource, overwrite to new resource
            if (Resources.ContainsKey(cell))
                Resources[cell] = resource;
            else
                Resources.Add(cell, resource);
        }

        public void RemoveResourceFromCell(Cell cell)
        {
            Resources.Remove(cell);
        }

        public bool CellHasResource(Cell cell)
        {
            return Resources.ContainsKey(cell);
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

        public void HarvestResource(Worker harvester, Resource resource)
        {
            resource.GiveResources(harvester);

            // If resource needs to be removed, tell all relevant Workers to find a new target resource.
            if (resource.IsDepleted)
            {
                Cell resourceCell = Grid.CellAt(resource.Mid);

                Resources.Remove(resourceCell);
                resourceCell.Passable = true;

                foreach (Worker w in resource.Harvesters)
                    w.FSM.ChangeState(MoveToResource.Instance);
            }                
        }

        public void RenderDebug(SpriteBatch spriteBatch)
        {
            // TODO: A lot of this can be put inside Unit Classes which would remove
            // a bunch of the "fuck it everything's public".

            if (Debug.OptionActive(DebugOption.ShowPaths))
            {
                foreach (Unit u in Units)
                    u.pathHandler.RenderPath(spriteBatch);
            }

            if (Debug.OptionActive(DebugOption.ShowUnitFeelers))
            {
                foreach (Unit u in Units)
                {
                    SteeringBehaviours s = u._steering;
                    spriteBatch.DrawLine(u.RenderMid, Camera.VecToScreen(s.centreFeelerEnd), Color.GreenYellow, 1);
                    spriteBatch.DrawLine(u.RenderMid, Camera.VecToScreen(s.leftFeelerEnd), Color.GreenYellow, 1);
                    spriteBatch.DrawLine(u.RenderMid, Camera.VecToScreen(s.rightFeelerEnd), Color.GreenYellow, 1);
                }
            }

            if (Debug.OptionActive(DebugOption.ShowWallPushForce))
            {
                foreach (Unit u in Units)
                {
                    if (u._steering.closestUnpassableCellMid != Vector2.Zero)
                    {
                        Vector2 wallPushForce = u._steering.wallPushForce;
                        spriteBatch.DrawLine(Camera.VecToScreen(u._steering.closestUnpassableCellMid), 
                                                Camera.VecToScreen(u._steering.closestUnpassableCellMid + (wallPushForce * 100)), 
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
        }
    }
}
