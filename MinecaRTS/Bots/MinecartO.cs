using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MinecaRTS
{
    public class MinecartO : Bot
    {
        private Stack<Goal<Worker>> goals;

        public MinecartO(PlayerData data) : base(data)
        {
            goals = new Stack<Goal<Worker>>();
        }

        public override void HandleInput()
        {
            Debug.HookText("Number of Goals: " + goals.Count.ToString());
            if (Input.KeyTyped(Keys.R))
            {
                goals.Push(new GoalFindResource(Data.GetClosestFreeWorkerToPos(Vector2.Zero), ResourceType.Stone));
                goals.Peek().Activate();
            }
                

            if (goals.Count > 0)
            {
                goals.Peek().Process();

                if (goals.Peek().State == GoalState.Complete)
                {
                    goals.Pop();
                }
            }
                
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
                Vector2 housePos = Data.GetHousePlacementPos();

                if (housePos != Vector2.Zero)
                {
                    House house = BuildingFactory.CreateHouse(Data, housePos);

                    if (Data.BuyBuilding(house, housePos))
                    {
                        Worker worker = Data.GetClosestFreeWorkerToPos(housePos);

                        if (worker != null)
                            worker.GoConstructBuilding(house);
                    }
                }                      
            }                
        }

        public override void OnBuildingComplete(Building building)
        {
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
