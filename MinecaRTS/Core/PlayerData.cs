using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MinecaRTS
{
    /// <summary>
    /// The interface through which the player interacts with the world.
    /// Bots can be plugged in to a PlayerData and can access it to issue commands.
    /// </summary>
    public class PlayerData
    {
        // Bots can't access the world directly. PlayerData defines various wrapper methods for a limited interface with the world.
        // Therefore bots can use SOME methods of world, but only those which PlayerData defines a wrapper for.
        private World _world;

        private Type _buildingFactoryClass;

        public Building selectedBuilding;

        private uint _wood = 10000;
        private uint _stone = 0;
        private uint _currentSupply = 0;

        private Panel _buildingSelectionPanel;
        private Panel _unitCommandPanel;
        private Panel _buildingCommandPanel;

        private Team _team;

        public Team Team
        {
            get { return _team; }
        }

        public uint Wood
        {
            get { return _wood; }
        }

        public uint Stone
        {
            get { return _stone; }
        }

        // 10 + (houses * 10)
        public uint MaxSupply
        {
            get
            {
                uint result = 10;

                foreach (Building b in _world.Buildings)
                {
                    if (b.IsActive)
                    {
                        IBoostsSupply supplyBooster = b as IBoostsSupply;

                        if (supplyBooster != null)
                        {
                            result += supplyBooster.SupplyBoostAmount;
                        }
                    }                    
                }

                return result;
            }
        }

        // How much can build supply capped
        public uint SpareSupply
        {
            get { return MaxSupply - _currentSupply; }
        }

        public Worker SelectedWorker
        {
            get
            {
                foreach (Unit u in _world.SelectedUnits)
                {
                    Worker w = u as Worker;

                    if (w != null && w.FSM.CurrentState != MoveToConstructBuilding.Instance && w.FSM.CurrentState != ConstructBuilding.Instance)
                        return w;
                }

                return null;
            }
        }

        public PlayerData(World w, Team team)
        {
            _world = w;
            _team = team;

            _buildingFactoryClass = Type.GetType("MinecaRTS.BuildingFactory");

            _buildingSelectionPanel = new Panel("Construct Buildings", new Vector2(250, Camera.HEIGHT - 175), new Vector2(300, 175));
            _buildingSelectionPanel.AddButton(new Button("Town Hall", new Vector2(5, 35), new Vector2(70, 30), _buildingSelectionPanel));
            _buildingSelectionPanel.AddButton(new Button("House", new Vector2(5, 68), new Vector2(70, 30), _buildingSelectionPanel));
            _buildingSelectionPanel.AddButton(new Button("Deposit Box", new Vector2(5, 101), new Vector2(70, 30), _buildingSelectionPanel));
            _buildingSelectionPanel.AddButton(new Button("Track", new Vector2(80, 35), new Vector2(70, 30), _buildingSelectionPanel));

            _unitCommandPanel = new Panel("Selected Units", new Vector2(Camera.WIDTH - 250, Camera.HEIGHT - 250), new Vector2(250, 250));
            _unitCommandPanel.AddButton(new Button("Stop", new Vector2(5, 35), new Vector2(70, 30), _unitCommandPanel));
            _unitCommandPanel.AddButton(new Button("Gather Wood", new Vector2(5, 68), new Vector2(70, 30), _unitCommandPanel));
            _unitCommandPanel.AddButton(new Button("Gather Stone", new Vector2(5, 101), new Vector2(70, 30), _unitCommandPanel));

            _buildingCommandPanel = new Panel("Selected Building", new Vector2(Camera.WIDTH - 500, Camera.HEIGHT - 250), new Vector2(250, 250));
            _buildingCommandPanel.AddButton(new Button("Build 0", new Vector2(5, 101), new Vector2(70, 30), _buildingCommandPanel));
            _buildingCommandPanel.AddButton(new Button("Build 1", new Vector2(5, 134), new Vector2(70, 30), _buildingCommandPanel));
            _buildingCommandPanel.AddButton(new Button("Rally Point", new Vector2(5, 167), new Vector2(70, 30), _buildingCommandPanel));
        }

        #region World Wrapper Methods
        public Grid Grid
        {
            get { return _world.Grid; }
        }

        public void RemoveUnitFromCollisionCells(Unit u)
        {
            _world.collisionCells.RemoveUnit(u);
        }

        public void AddUnitToCollisionCells(Unit u)
        {
            _world.collisionCells.AddUnit(u);
        }

        public void UpdateFogOfWarForUnit(Unit u)
        {
            _world.fogOfWar.UnitMoved(u);
        }

        public List<HashSet<Unit>> GetUnitsInCollisionCellsAroundPos(Vector2 pos)
        {
            return _world.collisionCells.GetUnitsInCellsAroundPos(pos);
        }

        public List<HashSet<Unit>> GetUnitsInCollisionCellsAroundUnit(Unit u)
        {
            return _world.collisionCells.GetUnitsInCellsAroundUnit(u);
        }

        public HashSet<Unit> GetUnitsInSameCollisionCellsAsUnit(Unit u)
        {
            return _world.collisionCells.GetUnitsInSameCellAsUnit(u);
        }

        public Resource GetResourceFromCell(Cell cell)
        {
            if (cell != null)
            {
                if (_world.fogOfWar.TeamHasExploredPos(Team, cell.Mid))
                    return _world.GetResourceFromCell(cell);
            }
            
            return null;
        }

        public Track GetTrackFromCell(Cell cell)
        {
            if (_world.fogOfWar.TeamCanSeePos(Team, cell.Mid))
                return _world.GetTrackFromCell(cell);
            return null;
        }

        public bool CellHasResource(Cell cell)
        {
            if (_world.fogOfWar.TeamHasExploredPos(Team, cell.Mid))
                return _world.CellHasResource(cell);
            return false;
        }

        public bool CellHasTrack(Cell cell)
        {
            if (_world.fogOfWar.TeamCanSeePos(Team, cell.Mid))
                return _world.CellHasTrack(cell);
            return false;
        }

        public void AddUnit(Type unitType, Vector2 pos, Team team, Vector2 rallyPoint)
        {
            _world.AddUnit(unitType, pos, Team, rallyPoint);
        }

        #endregion World Wrapper Methods

        public void GiveResources(uint amount, ResourceType type)
        {
            if (type == ResourceType.Wood)
                _wood += amount;
            else if (type == ResourceType.Stone)
                _stone += amount;
            else
                throw new Exception("Invalid resource type in PlayerData.GiveResources");
        }

        public void SelectUnitsInRect(Rectangle selectAt)
        {
            // TODO: Figure out how to do this with LINQ.

            _world.SelectedUnits = new List<Unit>();

            foreach (Unit u in _world.Units)
            {
                if (selectAt.Intersects(u.CollisionRect))
                    _world.SelectedUnits.Add(u);
            }
        }

        public void MoveSelectedUnitsTo(Vector2 pos)
        {
            //float groupRadius = _world.SelectedUnits.Count;

            //Random rand = new Random();

            foreach (Unit u in _world.SelectedUnits)
            {
                //u.MoveTowards(new Vector2(pos.X + (rand.NextSingle(-1, 1) * groupRadius), pos.Y + (rand.NextSingle(-1, 1) * groupRadius)));
                u.MoveTowards(pos);
            }                          
        }

        public void OrderSelectedWorkersToGatherClosestResource(ResourceType desiredResource)
        {
            foreach (Unit u in _world.SelectedUnits)
            {
                Worker w = u as Worker;
                
                // If this is actually a worker and not some other unit type
                if (w != null)
                {
                    w.resrcLookingFor = desiredResource;
                    w.returningResourcesTo = null;
                    w.targetResourceCell = null;
                    w.FSM.ChangeState(MoveToResource.Instance);
                }               
            }
        }

        public void OrderSelectedWorkerToConstructBuilding(Building building)
        {
            if (SelectedWorker != null)
            {
                SelectedWorker.GoConstructBuilding(building);
            }
        }

        public void OrderSelectedUnitsToStop()
        {
            foreach (Unit u in _world.SelectedUnits)
                u.Stop();
        }

        // TODO: SLOW - can be optimised with a floodfill using spatial partion cells.
        public Worker GetClosestWorkerToPos(Vector2 pos)
        {
            float closestDistance = float.MaxValue;
            Worker closestWorker = null;

            foreach (Unit u in _world.Units)
            {
                if (u is Worker)
                {
                    var distance = Vector2.Distance(pos, u.Mid);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestWorker = u as Worker;
                    }
                }
            }
            return closestWorker;
        }

        public List<Unit> GetCollidingUnits(Unit unit)
        {
            var result = new List<Unit>();

            foreach (Unit u in GetUnitsInSameCollisionCellsAsUnit(unit))
            {
                if (unit.CollisionRect.Intersects(u.CollisionRect))
                    result.Add(u);
            }

            // Remove the unit being checked from list to prevent checks when this list is used.
            result.Remove(unit);

            return result;
        }

        public List<Unit> GetUnitsInRadius(Unit unit, float radius)
        {
            var result = new List<Unit>();

            float taggableDistance = (float)Math.Pow(radius + unit.Scale.X / 2, 2);

            foreach (HashSet<Unit> units in GetUnitsInCollisionCellsAroundUnit(unit))
            {
                foreach (Unit u in units)
                {
                    if (Vector2.DistanceSquared(unit.Mid, u.Mid) < taggableDistance)
                        result.Add(u);
                }
            }            

            // Remove the unit being checked from list to prevent checks when this list is used.
            result.Remove(unit);

            return result;
        }

        public Building BuildingAtPos(Vector2 pos)
        {
            foreach (Building b in _world.Buildings)
            {
                if (b.CollisionRect.Contains(pos))
                    return b;
            }

            return null;
        }

        public void SelectFirstBuildingInRect(Rectangle rect)
        {
            foreach (Building b in _world.Buildings)
            {
                if (rect.Intersects(b.CollisionRect))
                {
                    selectedBuilding = b;
                    return;
                }
            }
        }

        // Really wanted to use Generics here, but since the Type argument is a variable in HumanPlayer, it's illegal.
        // Won't let us call a Generic method with a Type variable instead of a literal type name without a bunch 
        // of extra mumbo jumbo MethodInfo.MakeGenericMethod() and such.
        public bool BuyBuilding(Building building, Vector2 pos)
        {
            //if (!T.IsSubclassOf(typeof(Building)))
            //throw new InvalidOperationException(T.Name + " does not inherit from Building");

            //MethodInfo createBuilding = Type.GetType("MinecaRTS.BuildingFactory").GetMethod("Create" + T.Name);

            //Building b = (Building)createBuilding.Invoke(this, new object[] { this, pos});

            ProductionBuilding prodBuild = building as ProductionBuilding;

            if (prodBuild != null)
                prodBuild.ResetRallyPoint();

            // Can only buy building if area is clear and there are no minecart tracks around
            foreach (Cell c in _world.Grid.CellsInRect(building.CollisionRect))
            {
                if (_world.CellHasTrack(c))
                    return false;
            }

            if (_world.Grid.RectIsClear(building.CollisionRect))
            {                
                Type buildingType = building.GetType();

                if (CanAffordEntityType(buildingType))
                {
                    Cost cost = World.entityCosts[buildingType];

                    SpendResources(cost.woodCost, cost.stoneCost);

                    _world.AddBuilding(building);

                    // If you bought a building and you have a selected worker, it will go construct it.
                    OrderSelectedWorkerToConstructBuilding(building);

                    return true;
                }              
            }

            return false;
        }

        public void SpendResourcesForUnitType(Type unitType)
        {
            if (CanAffordEntityType(unitType))
            {
                Cost cost = World.entityCosts[unitType];

                SpendResources(cost.woodCost, cost.stoneCost);
                _currentSupply += cost.supplyCost;

                MsgBoard.AddMessage(this, World.MSG_ID, MessageType.SupplyChanged, info: unitType);
            }
        }

        public bool CanAffordEntityType(Type entityType)
        {
            Cost cost = World.entityCosts[entityType];
            return Wood >= cost.woodCost && Stone >= cost.stoneCost && SpareSupply >= cost.supplyCost;
        }

        public void QueueUpProductionOnSelectedBuildingAtIndex(int index)
        {
            if (selectedBuilding != null)
            {
                selectedBuilding.QueueUpProductionAtIndex(index);
            }
        }

        public void SpendResources(uint wood, uint stone)
        {
            _wood -= wood;
            _stone -= stone;

            if (_wood < 0)
                _wood = 0;

            if (_stone < 0)
                _stone = 0;
        }

        public T GetClosestActiveBuilding<T>(Unit u) where T : Building
        {
            float closestDistance = float.MaxValue;
            Building closestBuilding = null;

            foreach (Building b in _world.Buildings)
            {
                if (b.IsActive && b is T)
                {
                    float distance = Vector2.Distance(u.Mid, b.Mid);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestBuilding = b;
                    }
                }
            }

            return closestBuilding as T;
        }

        public void SetSelectedBuildingRallyPointTo(Vector2 pos)
        {
            ProductionBuilding prodBuild = selectedBuilding as ProductionBuilding;

            if (prodBuild != null)
                prodBuild.rallyPoint = pos;
        }

        public Building GetClosestResourceReturnPoint(Unit u)
        {
            float closestDistance = float.MaxValue;
            Building closestBuilding = null;

            foreach (Building b in _world.Buildings)
            {
                if (b.IsActive && b as ICanAcceptResources != null)
                {
                    float distance = Vector2.Distance(u.Mid, b.Mid);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestBuilding = b;
                    }
                }                
            }

            return closestBuilding;
        }

        public void Render(SpriteBatch spriteBatch)
        {
            foreach (Unit u in _world.SelectedUnits)
                spriteBatch.DrawRectangle(u.RenderRect.GetInflated(3, 3), Color.SpringGreen);

            if (selectedBuilding != null)
            {
                spriteBatch.DrawRectangle(selectedBuilding.RenderRect.GetInflated(3, 3), Color.SpringGreen);

                ProductionBuilding prodBuild = selectedBuilding as ProductionBuilding;

                if (prodBuild != null)
                    spriteBatch.DrawLine(prodBuild.RenderMid, Camera.VecToScreen(prodBuild.rallyPoint), Color.SpringGreen, 1);
            }            
        }

        public void RenderUI(SpriteBatch spriteBatch)
        {
            RenderMinimap(spriteBatch);

            _buildingSelectionPanel.Render(spriteBatch);
            _unitCommandPanel.Render(spriteBatch);

            _buildingCommandPanel.Render(spriteBatch);

            if (selectedBuilding != null)
            {
                spriteBatch.DrawString(MinecaRTS.smallFont, selectedBuilding.GetType().Name, new Vector2(_buildingCommandPanel.Pos.X + 5, _buildingCommandPanel.Pos.Y + 25), Color.White);

                ProductionBuilding prodBuild = selectedBuilding as ProductionBuilding;

                if (prodBuild != null)
                {
                    for (int i = 0; i < prodBuild.ProductionTypes.Count; i++)
                    {
                        spriteBatch.DrawString(MinecaRTS.smallFont, "(" + i + ") " + prodBuild.ProductionTypes[i].Name, new Vector2(_buildingCommandPanel.Pos.X + 10, _buildingCommandPanel.Pos.Y + 35 + (i * 10)), Color.White);
                    }
                }
            }

            spriteBatch.DrawString(MinecaRTS.largeFont, "WOOD: " + Wood.ToString(), new Vector2(500, 10), Color.White);
            spriteBatch.DrawString(MinecaRTS.largeFont, "STONE: " + Stone.ToString(), new Vector2(700, 10), Color.White);
            spriteBatch.DrawString(MinecaRTS.largeFont, "SUPPLY: " + _currentSupply.ToString() + "/" + MaxSupply.ToString(), new Vector2(900, 10), Color.White);
        }

        public bool PanelAtPos(string panelName, Vector2 pos)
        {
            if (panelName == "Construct Buildings")
            {
                return _buildingSelectionPanel.RenderRect.Contains(pos);
            }

            return false;
        }

        public Button ButtonAtPos(Vector2 pos)
        {
            if (_buildingSelectionPanel.RenderRect.Contains(pos))
                return _buildingSelectionPanel.ButtonAtPos(pos);

            if (_unitCommandPanel.RenderRect.Contains(pos))
                return _unitCommandPanel.ButtonAtPos(pos);

            if (_buildingCommandPanel.RenderRect.Contains(pos))
                return _buildingCommandPanel.ButtonAtPos(pos);

            return null;
        }

        public bool ClickedOnUI()
        {
            Vector2 mousePos = Input.MousePos;

            if (_buildingSelectionPanel.RenderRect.Contains(mousePos))
                return true;

            if (_unitCommandPanel.RenderRect.Contains(mousePos))
                return true;

            if (_buildingCommandPanel.RenderRect.Contains(mousePos))
                return true;

            if (Camera.MinimapRect.Contains(mousePos))
                return true;

            return false;
        }

        public void RenderMinimap(SpriteBatch spriteBatch)
        {
            float xRatio = 1 + (World.Width / Camera.MINIMAP_SIZE);
            float yRatio = 1 + (World.Height / Camera.MINIMAP_SIZE);

            spriteBatch.FillRectangle(new Rectangle(Camera.MINIMAP_X, Camera.MINIMAP_Y, Camera.MINIMAP_SIZE, Camera.MINIMAP_SIZE), Color.DarkSlateGray);

            foreach (Building b in _world.Buildings)
                spriteBatch.FillRectangle(Camera.WorldRectToMinimapRect(b.CollisionRect), Color.SpringGreen);

            foreach (Unit u in _world.Units)
                spriteBatch.FillRectangle(Camera.WorldRectToMinimapRect(u.CollisionRect), Color.LawnGreen);

            foreach (Resource r in _world.Resources.Values)
            {
                if (!Debug.IsOn(DebugOp.ShowFogOfWar) || _world.fogOfWar.TeamHasExploredPos(_team, r.Mid))
                {
                    if (r.Type == ResourceType.Wood)
                        spriteBatch.FillRectangle(Camera.WorldRectToMinimapRect(r.CollisionRect), Color.Chocolate);
                    else if (r.Type == ResourceType.Stone)
                        spriteBatch.FillRectangle(Camera.WorldRectToMinimapRect(r.CollisionRect), Color.DimGray);
                }                
            }

            // Draw box representing current view
            spriteBatch.DrawRectangle(Camera.WorldRectToMinimapRect(Camera.Rect), Color.White);

            spriteBatch.DrawRectangle(Camera.MinimapRect, Color.White, 3);

        }

        public void TestPathfindingCalculationTime()
        {
            Unit u = new Unit(this, Team, new Vector2(50, 50), new Vector2(20, 20));

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            double total = 0;

            for (int loop = 0; loop < 30; loop++)
            {
                sw.Start();

                int numTests = 1000000;

                for (int i = 0; i < numTests; i++)
                    u.MoveTowards(Grid[Grid.Cols - 1, Grid.Rows - 1].Mid);
                sw.Stop();

                total += sw.Elapsed.TotalMilliseconds / numTests;

                Console.WriteLine(sw.Elapsed.TotalMilliseconds / numTests);

                sw.Reset();
            }

            Console.WriteLine("Global Average: " + total / 30);
        }
    }
}
