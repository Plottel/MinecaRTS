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
    public class HumanPlayer
    {
        private PlayerData _data;

        private bool _boxing = false;
        private Point _boxStart = Point.Zero;
        private Rectangle _box = Rectangle.Empty;

        // TODO: This gives too much power to the bot.
        // Should be something like "select a building (enum)" and "select a position",
        // then send a request to PlayerData to actually handle it.
        private bool _placingBuilding = false;
        private Building _buildingToPlace = null;

        public HumanPlayer(PlayerData data)
        {
            _data = data;
        }

        public void HandleInput()
        {
            // Move camera with arrow keys.
            if (Input.KeyDown(Keys.Left))
                Camera.MoveBy(-5, 0);
            if (Input.KeyDown(Keys.Right))
                Camera.MoveBy(5, 0);
            if (Input.KeyDown(Keys.Up))
                Camera.MoveBy(0, -5);
            if (Input.KeyDown(Keys.Down))
                Camera.MoveBy(0, 5);
            
            if (Input.LeftMouseClicked())
            {
                if (_boxing)
                {
                    _data.SelectFirstBuildingInRect(Camera.RectToWorld(_box));
                    _data.SelectUnitsInRect(Camera.RectToWorld(_box));
                    _boxing = false;
                    _box = Rectangle.Empty;
                }
            }

            // TODO: This event will get messy with priorities. (Boxing, placing building, issuing attack etc. etc.)
            // most likely need a State Machine here.
            if (Input.LeftMouseDown())
            {
                // Handle building placement
                if (_placingBuilding)
                {
                    // Don't take away building blueprint unless a valid selection was made.
                    if (_data.BuyBuilding(_buildingToPlace))
                    {
                        _data.OrderSelectedWorkerToConstructBuilding(_buildingToPlace);
                        _placingBuilding = false;
                        _buildingToPlace.isActive = false;
                    }                        
                }
                else // Handle unit selection
                {
                    if (!_boxing)
                    {
                        _boxing = true;
                        _box = new Rectangle(Input.MousePos.ToPoint(), Point.Zero);
                        _boxStart = Input.MousePos.ToPoint();
                    }
                    else
                    {
                        _box.X = Math.Min(Input.MouseX, _boxStart.X);
                        _box.Y = Math.Min(Input.MouseY, _boxStart.Y);
                        _box.Width = Math.Abs(_boxStart.X - Input.MouseX);
                        _box.Height = Math.Abs(_boxStart.Y - Input.MouseY);
                    }
                }                
            }

            if (Input.RightMousePressed())
                _data.MoveSelectedUnitsTo(Camera.VecToWorld(Input.MousePos));

            if (Input.KeyTyped(Keys.B))
            {
                _placingBuilding = true;
                _buildingToPlace = BuildingFactory.CreateTownHall(_data, Vector2.Zero);
                _buildingToPlace.isActive = true;
            }

            if (_placingBuilding)
            {
                if (Input.KeyTyped(Keys.H))
                {
                    _buildingToPlace = BuildingFactory.CreateHouse(_data, Vector2.Zero);
                    _buildingToPlace.isActive = true;
                }                    

                if (Input.KeyTyped(Keys.P))
                {
                    _buildingToPlace = BuildingFactory.CreateTownHall(_data, Vector2.Zero);
                    _buildingToPlace.isActive = true;
                }                    
            }

            // C for cancel - stop placing building mode
            if (Input.KeyTyped(Keys.C))
            {
                _placingBuilding = false;
                _buildingToPlace = null;
            }

            if (Input.KeyTyped(Keys.W))
                _data.OrderSelectedWorkersToGatherClosestResource(ResourceType.Wood);

            if (Input.KeyTyped(Keys.S))
                _data.OrderSelectedWorkersToGatherClosestResource(ResourceType.Stone);

            if (Input.KeyTyped(Keys.H))
                _data.OrderSelectedUnitsToStop();

            if (Input.KeyTyped(Keys.Q))
                _data.HandleSelectedBuildingInputAtIndex(0);
        }

        public void Render(SpriteBatch spriteBatch)
        {
            if (_boxing)
                spriteBatch.DrawRectangle(_box, Color.SpringGreen, 2);

            if (_placingBuilding)
            {
                // Treat mouse pos as building center, poffset by building scale                   
                _buildingToPlace.Pos = _data.world.Grid.CellAt(Camera.VecToWorld(Input.MousePos - _buildingToPlace.Scale / 2)).Pos;
                _buildingToPlace.Render(spriteBatch);

                foreach (Cell c in _data.world.Grid.CellsInRect(_buildingToPlace.CollisionRect))
                {
                    if (!c.Passable)
                        spriteBatch.FillRectangle(c.RenderRect, new Color(Color.Red, 100));
                    else
                        spriteBatch.FillRectangle(c.RenderRect, new Color(Color.Green, 100));

                }

                // Render building selection information
                Vector2 buildingPos = Camera.VecToScreen(_buildingToPlace.Pos);
                spriteBatch.DrawString(MinecaRTS.smallFont, "(H) House", buildingPos, Color.White);
                spriteBatch.DrawString(MinecaRTS.smallFont, "(P) Production", new Vector2(buildingPos.X, buildingPos.Y + 10), Color.White);
            }
        }
    }
}
