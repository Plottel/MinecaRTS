using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    class MoveToDepositBox : State<Minecart>
    {
        private static MoveToDepositBox _instance;

        public static MoveToDepositBox Instance
        {
            get { return _instance; }
        }

        public MoveToDepositBox()
        {
            if (_instance == null)
                _instance = this;
            else
                throw new InvalidOperationException("Cannot have more than one instance of MoveToDepositBox Minecart State");
        }

        public override void Enter(Minecart owner)
        {
            if (owner.TargetDepositBox != null)
                owner.pathHandler.GetPathTo(owner.TargetDepositBox.Mid);
            // TODO: else find path to closest deposit box

            //owner.ChangeAnimation(WorkerAnimation.Walk);
        }

        public override void Exit(Minecart owner)
        {
            owner.FollowPath = false;
        }

        public override void Execute(Minecart owner)
        {
            //if (owner.CollisionRect.GetInflated(5, 5).Intersects(owner.targetResourceCell.CollisionRect))
            //owner.FSM.ChangeState(HarvestResource.Instance);

            if (owner.CollisionRect.GetInflated(32, 32).Intersects(owner.TargetDepositBox.CollisionRect))
            {
                MsgBoard.AddMessage(owner, owner.TargetDepositBox.ID, MessageType.GiveMeResources);
                owner.FSM.ChangeState(ReturnToTownHall.Instance);
            }
                
        }

        public override void HandleMessage(Minecart owner, Message message)
        {
        }
    }

    public class ReturnToTownHall : State<Minecart>
    {
        private static ReturnToTownHall _instance;

        public static ReturnToTownHall Instance
        {
            get { return _instance; }
        }

        public ReturnToTownHall()
        {
            if (_instance == null)
                _instance = this;
            else
                throw new InvalidOperationException("Cannot have more than one instance of MoveToDepositBox Minecart State");
        }

        public override void Enter(Minecart owner)
        {
            owner.TargetTownHall = owner.Data.GetClosestActiveBuilding<TownHall>(owner);
            owner.pathHandler.GetPathTo(owner.TargetTownHall.Mid);

            //owner.ChangeAnimation(WorkerAnimation.Walk);
        }

        public override void Exit(Minecart owner)
        {
            owner.FollowPath = false;
        }

        public override void Execute(Minecart owner)
        {
            if (owner.CollisionRect.GetInflated(32, 32).Intersects(owner.TargetTownHall.CollisionRect))
            {
                owner.TargetTownHall.AcceptResources(owner.Wood, owner.Stone);
                owner.EmptySelf();
                owner.FSM.ChangeState(MoveToDepositBox.Instance);
            }                
        }

        public override void HandleMessage(Minecart owner, Message message)
        {
        }
    }
}
