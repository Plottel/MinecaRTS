using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    public class GoalBuildPerimeter : CompositeGoal<Worker, MinecartO>
    {
        public GoalBuildPerimeter(Worker owner, MinecartO bot) : base(owner, bot)
        { }

        public override void Activate()
        {
            var influenceBorder = owner.Data.GetInfluenceBorderAroundPos(bot.townHall.Mid);

            foreach (Cell cell in influenceBorder)
            {
                if (cell.Passable)
                {
                    Wall wall = BuildingFactory.CreateWall(owner.Data, cell.Pos);
                    AddSubgoal(new GoalConstructBuilding(owner, bot, wall));
                }
            }

            goals.Peek().Activate();
        }

        public override GoalState Process()
        {
            if (ProcessSubgoals() == GoalState.Complete)
            {
                if (goals.Count == 0)
                {
                    State = GoalState.Complete;
                    return GoalState.Complete;
                }
            }

            State = GoalState.Active;
            return GoalState.Active;
        }
    }
}
