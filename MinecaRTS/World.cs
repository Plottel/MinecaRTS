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

        public readonly Grid Grid;
        public readonly List<Unit> Units;
        public readonly List<Building> Buildings;
        public List<Unit> SelectedUnits;
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
            SelectedUnits = new List<Unit>();
            _playerOneData = new PlayerData(this);
            _playerOne = new HumanPlayer(_playerOneData);
        }

        public void HandleInput()
        {
            _playerOne.HandleInput();

            if (Input.KeyTyped(Keys.W))
            {
                foreach (Unit u in Units)
                {
                    u.pathHandler.GetPathToClosestResource(ResourceType.Wood);
                }
            }

            if (Input.KeyTyped(Keys.S))
            {
                foreach (Unit u in Units)
                {
                    u.pathHandler.GetPathToClosestResource(ResourceType.Stone);
                }
            }
        }

        public void Update()
        {
            foreach (Unit u in Units)
                u.Update();
        }

        public void Render(SpriteBatch spriteBatch)
        {
            // TODO: Only render what's on the screen.
            Grid.Render(spriteBatch);

            foreach (Building b in Buildings)
                b.Render(spriteBatch);

            foreach (Unit u in Units)
                u.Render(spriteBatch);

            foreach (Unit u in SelectedUnits)
                spriteBatch.DrawRectangle(u.RenderRect.GetInflated(3, 3), Color.SpringGreen);

            _playerOne.Render(spriteBatch);
        }

        public void AddUnit(Vector2 pos, Vector2 scale)
        {
            Units.Add(new Unit(_playerOneData, pos, scale));
        }

        public void AddWorker(Vector2 pos, Vector2 scale)
        {
            Units.Add(new Worker(_playerOneData, pos, scale));
        }

        public void AddBuilding(Building building)
        {
            Buildings.Add(building);
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

            if (Debug.OptionActive(DebugOption.ShowUnitVisionRect))
            {
                foreach (Unit u in Units)
                {
                    SteeringBehaviours s = u._steering;

                    spriteBatch.DrawLine(Camera.VecToScreen(s.from1), Camera.VecToScreen(s.to1), Color.GreenYellow, 1);
                    spriteBatch.DrawLine(Camera.VecToScreen(s.from2), Camera.VecToScreen(s.to2), Color.GreenYellow, 1);
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
