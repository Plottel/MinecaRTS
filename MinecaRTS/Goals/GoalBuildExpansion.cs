using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    public class GoalBuildExpansion : CompositeGoal<Worker, MinecartO>
    {
        ResourceType Type;
        bool foundResource = false;

        public GoalBuildExpansion(Worker owner, MinecartO bot, ResourceType Type) : base(owner, bot)
        {
            this.Type = Type;
        }

        public override void Activate()
        {
            State = GoalState.Active;
            AddSubgoal(new GoalFindResource(owner, bot, Type));
            goals.Peek().Activate();
        }

        public override GoalState Process()
        {
            GoalState subgoalState = ProcessSubgoals();

            if (subgoalState == GoalState.Complete)
            {
                if (!foundResource)
                {
                    foundResource = true;

                    TownHall townHall = null;

                    if (Type == ResourceType.Wood)
                        townHall = BuildingFactory.CreateTownHall(owner.Data, bot.knownWoodCells.Last().Pos);
                    else if (Type == ResourceType.Stone)
                        townHall = BuildingFactory.CreateTownHall(owner.Data, bot.knownStoneCells.Last().Pos);

                    townHall.Pos = owner.Data.GetValidBuildingPlacementPos(townHall);

                    if (owner.Data.BuyBuilding(townHall, townHall.Pos))
                    {
                        bot.townHall = townHall;
                        AddSubgoal(new GoalConstructBuilding(owner, bot, townHall));
                        goals.Peek().Activate();
                    }                   
                }
                else // We've already found resource, so completed goal is constructing Town Hall. Goal is complete.
                {
                    subgoalState = GoalState.Complete;
                    State = GoalState.Complete;
                    return GoalState.Complete;
                }
                   
            }

            State = GoalState.Active;
            return GoalState.Active;
        }
    }
}
