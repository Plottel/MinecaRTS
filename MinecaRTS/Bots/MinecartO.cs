using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    public class MinecartO : Bot
    {
        private Vector2 _buildHousesAt = new Vector2(320, 156);

        public MinecartO(PlayerData data) : base(data)
        {
        }

        public override void HandleInput()
        {
            // Logic loop
        }

        public override void OnUnitSpawn(Unit newUnit)
        {
            Worker w = newUnit as Worker;

            if (w != null)
            {
                w.resrcLookingFor = ResourceType.Wood;
                w.FSM.ChangeState(MoveToResource.Instance);
            }                
        }

        public override void OnProductionBuildingTaskComplete(ProductionBuilding prodBuild)
        {
            prodBuild.QueueUpProductionAtIndex(0);
        }

        public override void OnSupplyChange()
        {
            if (Data.SpareSupply <= 4)
            { 
                House house = BuildingFactory.CreateHouse(Data, _buildHousesAt);

                if (Data.BuyBuilding(house, _buildHousesAt))
                {
                    Worker worker = Data.GetClosestWorkerToPos(_buildHousesAt);

                    if (worker != null)
                        worker.GoConstructBuilding(house);
                }                
            }                
        }

        public override void OnBuildingComplete(Building building)
        {
            if (building is House)
                _buildHousesAt.X += 64;

            foreach (HashSet<Unit> unitsInCell in Data.GetUnitsInCollisionCellsAroundPos(building.Mid))
            {
                foreach (Unit unit in unitsInCell)
                {
                    Worker w = unit as Worker;

                    if (w.FSM.CurrentState == null)
                        w.FSM.ChangeState(MoveToResource.Instance);
                }
            }
        }
    }
}
