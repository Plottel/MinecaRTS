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
    public class HumanPlayer : Player
    {
        private bool _isBoxing = false;
        private bool _isSettingRallyPoint = false;
        private Point _boxStart = Point.Zero;
        private Rectangle _box = Rectangle.Empty;

        // TODO: This gives too much power to the bot.
        // Should be something like "select a building (enum)" and "select a position",
        // then send a request to PlayerData to actually handle it.
        private bool _isPlacingBuilding = false;
        private Building _buildingToPlace = null;

        public HumanPlayer(PlayerData data) : base(data)
        {
        }

        public override void HandleInput()
        {
            //if (Input.KeyTyped(Keys.T))
              //  _data.TestPathfindingCalculationTime();

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
            
            if (Input.LeftMouseClicked())
            {
                Button clickedOn = Data.ButtonAtPos(Input.MousePos);

                if (clickedOn != null)
                {
                    switch (clickedOn.Name)
                    {
                        case "Town Hall":
                            _isPlacingBuilding = true;
                            _buildingToPlace = BuildingFactory.CreateTownHall(Data, Vector2.Zero);
                            break;

                        case "House":
                            _isPlacingBuilding = true;
                            _buildingToPlace = BuildingFactory.CreateHouse(Data, Vector2.Zero);
                            break;

                        case "Deposit Box":
                            _isPlacingBuilding = true;
                            _buildingToPlace = BuildingFactory.CreateDepositBox(Data, Vector2.Zero);
                            break;

                        case "Track":
                            _isPlacingBuilding = true;
                            _buildingToPlace = BuildingFactory.CreateTrack(Data, Vector2.Zero);
                            break;

                        case "Stop":
                            Data.OrderSelectedUnitsToStop();
                            break;

                        case "Gather Wood":
                            Data.OrderSelectedWorkersToGatherClosestResource(ResourceType.Wood);
                            break;

                        case "Gather Stone":
                            Data.OrderSelectedWorkersToGatherClosestResource(ResourceType.Stone);
                            break;

                        case "Build 0":
                            Data.QueueUpProductionOnSelectedBuildingAtIndex(0);
                            break;

                        case "Build 1":
                            Data.QueueUpProductionOnSelectedBuildingAtIndex(1);
                            break;

                        case "Rally Point":
                            _isSettingRallyPoint = true;
                            break;
                    }
                }                

                if (_isBoxing && !Data.ClickedOnUI())
                {
                    Data.SelectFirstBuildingInRect(Camera.RectToWorld(_box));
                    Data.SelectUnitsInRect(Camera.RectToWorld(_box));                    
                }

                _isBoxing = false;
                _box = Rectangle.Empty;
            }

            // TODO: This event will get messy with priorities. (Boxing, placing building, issuing attack etc. etc.)
            // most likely need a State Machine here.
            if (Input.LeftMouseDown())
            {
                if (_isSettingRallyPoint && !Data.ClickedOnUI())
                {
                    Data.SetSelectedBuildingRallyPointTo(Camera.VecToWorld(Input.MousePos));
                    _isSettingRallyPoint = false;
                }

                // Handle building placement
                if (_isPlacingBuilding && !Data.ClickedOnUI())
                {
                    // Don't take away building blueprint unless a valid selection was made.
                    if (Data.BuyBuilding(_buildingToPlace, Camera.VecToWorld(Input.MousePos)))
                    {
                        Data.OrderSelectedWorkerToConstructBuilding(_buildingToPlace);

                        // Can create tracks without re-selecting building options.
                        if (!(_buildingToPlace is Track))
                            _isPlacingBuilding = false;
                        else
                        {
                            _buildingToPlace = BuildingFactory.CreateTrack(Data, Vector2.Zero);
                        }
                    }                        
                }
                else // Handle unit selection
                {
                    if (!_isBoxing)
                    {
                        _isBoxing = true;
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
                Data.MoveSelectedUnitsTo(Camera.VecToWorld(Input.MousePos));

            if (Input.KeyTyped(Keys.B))
            {
                _isPlacingBuilding = true;
                _buildingToPlace = BuildingFactory.CreateTownHall(Data, Vector2.Zero);
            }

            if (Input.KeyTyped(Keys.R))
            {
                if (Data.selectedBuilding as ProductionBuilding != null)
                    _isSettingRallyPoint = true;
            }

            if (_isPlacingBuilding)
            {
                if (Input.KeyTyped(Keys.H))
                    _buildingToPlace = BuildingFactory.CreateHouse(Data, Vector2.Zero);            

                if (Input.KeyTyped(Keys.P))
                    _buildingToPlace = BuildingFactory.CreateTownHall(Data, Vector2.Zero);

                if (Input.KeyTyped(Keys.T))
                    _buildingToPlace = BuildingFactory.CreateTrack(Data, Vector2.Zero);

                if (Input.KeyTyped(Keys.D))
                    _buildingToPlace = BuildingFactory.CreateDepositBox(Data, Vector2.Zero);
            }

            // C for cancel - stop placing building mode
            if (Input.KeyTyped(Keys.C))
            {
                _isPlacingBuilding = false;
                _buildingToPlace = null;
            }

            if (Input.KeyTyped(Keys.O))
                Data.OrderSelectedWorkersToGatherClosestResource(ResourceType.Wood);

            if (Input.KeyTyped(Keys.S))
                Data.OrderSelectedWorkersToGatherClosestResource(ResourceType.Stone);

            if (Input.KeyTyped(Keys.H))
                Data.OrderSelectedUnitsToStop();

            if (Input.KeyTyped(Keys.Q))
                Data.QueueUpProductionOnSelectedBuildingAtIndex(0);

            if (Input.KeyTyped(Keys.W))
                Data.QueueUpProductionOnSelectedBuildingAtIndex(1);

        }

        public override void HandleMessage(Message message)
        {
        }

        public override void Render(SpriteBatch spriteBatch)
        {
            if (_isBoxing)
                spriteBatch.DrawRectangle(_box, Color.SpringGreen, 2);

            if (_isPlacingBuilding)
            {
                // Treat mouse pos as building center, poffset by building scale                   
                _buildingToPlace.Pos = Data.Grid.CellAt(Camera.VecToWorld(Input.MousePos)).Pos;
                spriteBatch.Draw(_buildingToPlace.ActiveTexture, _buildingToPlace.RenderPos, Color.White);

                foreach (Cell c in Data.Grid.CellsInRect(_buildingToPlace.CollisionRect))
                {
                    if (!c.Passable)
                        spriteBatch.FillRectangle(c.RenderRect, new Color(Color.Red, 100));
                    else
                        spriteBatch.FillRectangle(c.RenderRect, new Color(Color.Green, 100));

                }

                // Render building selection information
                Vector2 buildingPos = _buildingToPlace.RenderPos;
                spriteBatch.DrawString(MinecaRTS.smallFont, "(H) House", buildingPos, Color.White);
                spriteBatch.DrawString(MinecaRTS.smallFont, "(P) Production", new Vector2(buildingPos.X, buildingPos.Y + 10), Color.White);
                spriteBatch.DrawString(MinecaRTS.smallFont, "(T) Track", new Vector2(buildingPos.X, buildingPos.Y + 20), Color.White);
                spriteBatch.DrawString(MinecaRTS.smallFont, "(D) Deposit Box", new Vector2(buildingPos.X, buildingPos.Y + 30), Color.White);
            }
        }
    }
}
