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
        // This represents the facade. All queries are done through this and the real world figures it out.
        public World world;

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

                foreach (Building b in world.Buildings)
                {
                    if (b.isActive)
                    {
                        IBoostsSupply supplyBooster = b as IBoostsSupply;

                        if (supplyBooster != null)
                        {
                            result += supplyBooster.BoostAmount;
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
                foreach (Unit u in world.SelectedUnits)
                {
                    Worker w = u as Worker;

                    if (w != null)
                        return w;
                }

                return null;
            }
        }

        public PlayerData(World w, Team team)
        {
            world = w;
            _team = team;
        }

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

            world.SelectedUnits = new List<Unit>();

            foreach (Unit u in world.Units)
            {
                if (selectAt.Intersects(u.CollisionRect))
                    world.SelectedUnits.Add(u);
            }
        }

        public void MoveSelectedUnitsTo(Vector2 pos)
        {
            foreach (Unit u in world.SelectedUnits)
                u.MoveTowards(pos);          
        }

        public void OrderSelectedWorkersToGatherClosestResource(ResourceType desiredResource)
        {
            foreach (Worker w in world.SelectedUnits)
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
            foreach (Unit u in world.SelectedUnits)
                u.Stop();
        }

        public List<Unit> GetCollidingUnits(Unit unit)
        {
            var result = new List<Unit>();

            foreach (Unit u in world.Units)
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

            foreach (Unit u in world.Units)
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
            foreach (Building b in world.Buildings)
            {
                if (b.CollisionRect.Contains(pos))
                    return b;
            }

            return null;
        }

        public void SelectFirstBuildingInRect(Rectangle rect)
        {
            selectedBuilding = null;

            foreach (Building b in world.Buildings)
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
            if (world.Grid.RectIsClear(building.CollisionRect))
            {
                Type buildingType = building.GetType();

                if (CanAffordEntityType(buildingType))
                {
                    Cost cost = World.entityCosts[buildingType];

                    SpendResources(cost.woodCost, cost.stoneCost);

                    world.AddBuilding(building);
                    return true;
                }              
            }

            return false;
        }

        public void BuyUnit(Type unitType)
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

            foreach (Building b in world.Buildings)
            {
                if (b as ICanAcceptResources != null)
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
            foreach (Unit u in world.SelectedUnits)
                spriteBatch.DrawRectangle(u.RenderRect.GetInflated(3, 3), Color.SpringGreen);

            if (selectedBuilding != null)
                spriteBatch.DrawRectangle(selectedBuilding.RenderRect.GetInflated(3, 3), Color.SpringGreen);

            spriteBatch.DrawString(MinecaRTS.largeFont, "WOOD: " + Wood.ToString(), new Vector2(500, 10), Color.White);
            spriteBatch.DrawString(MinecaRTS.largeFont, "STONE: " + Stone.ToString(), new Vector2(700, 10), Color.White);
            spriteBatch.DrawString(MinecaRTS.largeFont, "SUPPLY: " + _currentSupply.ToString() + "/" + MaxSupply.ToString(), new Vector2(900, 10), Color.White);
        }
    }
}
