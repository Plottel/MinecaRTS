using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    public class GoalBuildTrackNetwork : CompositeGoal<Worker, MinecartO>
    {
        private DepositBox _depositBox;

        public GoalBuildTrackNetwork(Worker owner, MinecartO bot) : base(owner, bot)
        { }

        public override void Activate()
        {
            Pathfinder pathfinder = new Pathfinder(owner, owner.pathHandler);

            var inflatedBuildingCells = owner.Data.Grid.CellsInRect(bot.townHall.CollisionRect.GetInflated(16, 16));
            var buildingCells = owner.Data.Grid.CellsInRect(bot.townHall.CollisionRect);

            var borderCells = inflatedBuildingCells.Where(element => !buildingCells.Contains(element)).ToList();
            borderCells = borderCells.Where(element => element.Passable).ToList();

            List<Cell> trackPath = pathfinder.SearchGreedy(owner.Data.Grid,
                bot.knownStoneCells.Last(),
                borderCells,
                owner,
                owner.pathHandler.GreedyConsiderationCondition,
                owner.pathHandler.GreedyTerminationCondition,
                owner.pathHandler.GreedyScoreMethod,
                smoothed: false);

            // Chop off a couple of cells, don't build track right up to the resource.
            trackPath.RemoveAt(0);
            trackPath.RemoveAt(0);
            Cell endOfTrack = trackPath[0];
            trackPath.RemoveAt(0);

            _depositBox = BuildingFactory.CreateDepositBox(owner.Data, endOfTrack.Pos);
            _depositBox.Pos = owner.Data.GetValidBuildingPlacementPos(_depositBox);
            AddSubgoal(new GoalConstructBuilding(owner, bot, _depositBox));

            foreach (Cell trackCell in trackPath)
            {
                Track track = BuildingFactory.CreateTrack(owner.Data, trackCell.Pos);
                AddSubgoal(new GoalConstructBuilding(owner, bot, track));
            }

            goals.Peek().Activate();
        }

        public override GoalState Process()
        {
            if (owner.Data.Supply < 15)
            {
                State = GoalState.Active;
                return GoalState.Active;
            }

            if (ProcessSubgoals() == GoalState.Complete)
            {
                if (goals.Count == 0)
                {
                    bot.targetDepositBox = _depositBox;
                    State = GoalState.Complete;
                    return GoalState.Complete;
                }                    
            }

            State = GoalState.Active;
            return GoalState.Active;
        }

        public override void Terminate()
        {
            // Build a Minecart.
            bot.townHall.QueueUpProductionAtIndex(1);
            bot.townHall.rallyPoint = bot.knownStoneCells.Last().Mid;
        }
    }
}
