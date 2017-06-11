using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    public class GoalConstructBuilding : Goal<Worker, MinecartO>
    {
        private Building building;
        private bool constructionRequestSuccessful = false; 

        public GoalConstructBuilding(Worker owner, MinecartO bot, Building building) : base(owner, bot)
        {
            this.building = building;
        }

        public override void Activate()
        {
            State = GoalState.Active;
            building.Pos = owner.Data.GetValidBuildingPlacementPos(building);

            if (owner.Data.BuyBuilding(building, building.Pos))
            {
                constructionRequestSuccessful = true;
                owner.GoConstructBuilding(building);
            }           
        }

        public override GoalState Process()
        {
            if (constructionRequestSuccessful)
            {
                if (building.IsActive)
                    State = GoalState.Complete;
            }
            else
            {
                Activate();
                State = GoalState.Active;
            }
            

            return State;
        }
    }
}
