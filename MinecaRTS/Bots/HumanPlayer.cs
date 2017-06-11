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
    /// The class defining all behaviour for a HumanPlayer.
    /// Handles keyboard, mouse and various UI events. 
    /// </summary>
    public class HumanPlayer : Player
    {
        /// <summary>
        /// Whether or not the HumanPlayer is currently creating a selection box.
        /// This happens while left mouse is held down.
        /// </summary>
        private bool _isBoxing = false;

        /// <summary>
        /// Whether or not the HumanPlayer is currently setting a rally point on a building.
        /// This happens when a building is selected and R has been typed.
        /// </summary>
        private bool _isSettingRallyPoint = false;

        /// <summary>
        /// The starting point of the selection box. Used for determining the selection area when mouse is released.
        /// </summary>
        private Point _boxStart = Point.Zero;

        /// <summary>
        /// THe selection box. Used in conjunction with _boxStart to determine the selection area when mouse is released.
        /// </summary>
        private Rectangle _box = Rectangle.Empty;

        /// <summary>
        /// Whether or not the HumanPlayer is placing a building. This will disable other UI interactions while it is active.
        /// </summary>
        private bool _isPlacingBuilding = false;

        /// <summary>
        /// The building currently being placed by the HumanPlayer. Used for visualization and showing valid placement locations.
        /// </summary>
        private Building _buildingToPlace = null;

        /// <summary>
        /// Initializes a new HumanPlayer
        /// </summary>
        /// <param name="data">The PlayerData through which all queries are filtered.</param>
        public HumanPlayer(PlayerData data) : base(data)
        {
        }

        /// <summary>
        /// The method called once per tick defining all logic for the HumanPlayer.
        /// Unlike an AI bot which has decision making logic, this simply handles UI events.
        /// </summary>
        public override void HandleInput()
        {
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

                        // Can create tracks and wallswithout re-selecting building options.

                        if (!(_buildingToPlace is Track || _buildingToPlace is Wall))
                            _isPlacingBuilding = false;
                        else
                        {
                            if (_buildingToPlace is Track)
                                _buildingToPlace = BuildingFactory.CreateTrack(Data, Vector2.Zero);
                            else if (_buildingToPlace is Wall)
                                _buildingToPlace = BuildingFactory.CreateWall(Data, Vector2.Zero);
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

                if (Input.KeyTyped(Keys.F))
                    _buildingToPlace = BuildingFactory.CreateWall(Data, Vector2.Zero);
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

        /// <summary>
        /// Blank. Needs to be overridden to keep with common Bot interface.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void HandleMessage(Message message)
        {
        }

        /// <summary>
        /// The render function for the HumanPlayer. Renders relevant UI aspects such as selection box and
        /// building being placed.
        /// </summary>
        /// <param name="spriteBatch">The spriteBatch to render to.</param>
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
                spriteBatch.DrawString(MinecaRTS.smallFont, "(P) Town Hall", new Vector2(buildingPos.X, buildingPos.Y + 10), Color.White);
                spriteBatch.DrawString(MinecaRTS.smallFont, "(T) Track", new Vector2(buildingPos.X, buildingPos.Y + 20), Color.White);
                spriteBatch.DrawString(MinecaRTS.smallFont, "(D) Deposit Box", new Vector2(buildingPos.X, buildingPos.Y + 30), Color.White);
                spriteBatch.DrawString(MinecaRTS.smallFont, "(F) Wall", new Vector2(buildingPos.X, buildingPos.Y + 40), Color.White);
            }
        }
    }
}
