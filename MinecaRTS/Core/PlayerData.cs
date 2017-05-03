using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public Building selectedBuilding;

        private uint _wood = 1000;
        private uint _stone = 0;
        private uint _currentSupply = 0;

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
        }

        #region World Wrapper Methods
        public Grid Grid
        {
            get { return _world.Grid; }
        }

        public Resource GetResourceFromCell(Cell cell)
        {
            return _world.GetResourceFromCell(cell);
        }

        public Track GetTrackFromCell(Cell cell)
        {
            return _world.GetTrackFromCell(cell);
        }

        public bool CellHasTrack(Cell cell)
        {
            return _world.CellHasTrack(cell);
        }

        public void AddUnit(Type unitType, Vector2 pos, Team team)
        {
            _world.AddUnit(unitType, pos, Team);
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
            foreach (Unit u in _world.SelectedUnits)
                u.MoveTowards(pos);          
        }

        public void OrderSelectedWorkersToGatherClosestResource(ResourceType desiredResource)
        {
            foreach (Worker w in _world.SelectedUnits)
            {
                w.resrcLookingFor = desiredResource;
                w.returningResourcesTo = null;
                w.targetResourceCell = null;
                w.FSM.ChangeState(MoveToResource.Instance);         
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

        public List<Unit> GetCollidingUnits(Unit unit)
        {
            var result = new List<Unit>();

            foreach (Unit u in _world.Units)
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

            foreach (Unit u in _world.Units)
            {
                if (Vector2.DistanceSquared(unit.Mid, u.Mid) < taggableDistance)
                    result.Add(u);
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
            selectedBuilding = null;

            foreach (Building b in _world.Buildings)
            {
                if (rect.Intersects(b.CollisionRect))
                {
                    selectedBuilding = b;
                    return;
                }
            }
        }

        public bool BuyBuilding(Building building)
        {
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
            }
        }

        public bool CanAffordEntityType(Type entityType)
        {
            Cost cost = World.entityCosts[entityType];
            return Wood >= cost.woodCost && Stone >= cost.stoneCost && SpareSupply >= cost.supplyCost;
        }

        public void HandleSelectedBuildingInputAtIndex(int index)
        {
            if (selectedBuilding != null)
            {
                selectedBuilding.HandleInput(index);
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

        public Building GetClosestResourceReturnBuilding(Unit u)
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
                spriteBatch.DrawRectangle(selectedBuilding.RenderRect.GetInflated(3, 3), Color.SpringGreen);

            spriteBatch.DrawString(MinecaRTS.largeFont, "WOOD: " + Wood.ToString(), new Vector2(500, 10), Color.White);
            spriteBatch.DrawString(MinecaRTS.largeFont, "STONE: " + Stone.ToString(), new Vector2(700, 10), Color.White);
            spriteBatch.DrawString(MinecaRTS.largeFont, "SUPPLY: " + _currentSupply.ToString() + "/" + MaxSupply.ToString(), new Vector2(900, 10), Color.White);
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
                if (r.Type == ResourceType.Wood)
                    spriteBatch.FillRectangle(Camera.WorldRectToMinimapRect(r.CollisionRect), Color.Chocolate);
                else if (r.Type == ResourceType.Stone)
                    spriteBatch.FillRectangle(Camera.WorldRectToMinimapRect(r.CollisionRect), Color.DimGray);
            }

            // Draw box representing current view
            spriteBatch.DrawRectangle(Camera.WorldRectToMinimapRect(Camera.Rect), Color.White);

        }
    }
}
