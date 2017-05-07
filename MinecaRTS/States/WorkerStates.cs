using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    // -----------------------------------------------------------
    //
    //   Move to Resource Worker State
    //
    // -----------------------------------------------------------
    class MoveToResource : State<Worker>
    {
        private static MoveToResource _instance;

        public static MoveToResource Instance
        {
            get { return _instance; }
        }

        public MoveToResource()
        {
            if (_instance == null)
                _instance = this;
            else
                throw new InvalidOperationException("Cannot have more than one instance of MoveToResource Worker State");
        }

        public override void Enter(Worker owner)
        {
            owner.ChangeAnimation(WorkerAnimation.Walk);
            owner.Steering.separationOn = false;

            // TODO: This check will be more robust to check if the resource has expired???
            // should just Greedy if already has target resource.
            if (owner.TargetResource == null)
                owner.pathHandler.GetPathToClosestUnsaturatedResource(owner.resrcLookingFor);
            else
                owner.pathHandler.GetPathToResource(owner.TargetResource);

            owner.targetResourceCell = owner.pathHandler._path.Last();
        }

        public override void Exit(Worker owner)
        {
            owner.Steering.separationOn = true;
        }

        public override void Execute(Worker owner)
        {
            if (owner.CollisionRect.GetInflated(5, 5).Intersects(owner.targetResourceCell.CollisionRect))
                owner.FSM.ChangeState(HarvestResource.Instance);
        }

        public override void HandleMessage(Worker owner, Message message)
        {
        }
    }

    // -----------------------------------------------------------
    //
    //   Return Resource Worker State
    //
    // -----------------------------------------------------------
    class ReturnResource : State<Worker>
    {
        private static ReturnResource _instance;

        public static ReturnResource Instance
        {
            get { return _instance; }
        }

        public ReturnResource()
        {
            if (_instance == null)
                _instance = this;
            else
                throw new InvalidOperationException("Cannot have more than one instance of ReturnResource Worker State");
        }

        public override void Enter(Worker owner)
        {
            if (owner.resrcHolding == ResourceType.Wood)
                owner.ChangeAnimation(WorkerAnimation.Logs);
            else if (owner.resrcHolding == ResourceType.Stone)
                owner.ChangeAnimation(WorkerAnimation.Bag);

            owner.Steering.separationOn = false;

            // Get path to base
            //TODO: Need to fetch path to location if returning to a minecart
            if (owner.returningResourcesTo != null)
                owner.pathHandler.GetPathToBuilding(owner.returningResourcesTo as Building);
            else
            {
                Building closestBuilding = owner.Data.GetClosestResourceReturnPoint(owner);
                owner.pathHandler.GetPathToBuilding(closestBuilding);
                owner.returningResourcesTo = closestBuilding as ICanAcceptResources;
            }
        }

        public override void Exit(Worker owner)
        {
            owner.Steering.separationOn = true;
            owner.ChangeAnimation(WorkerAnimation.Walk);
        }

        public override void Execute(Worker owner)
        {
            if (owner.CollisionRect.GetInflated(5, 5).Intersects(owner.returningResourcesTo.CollisionRect))
            {
                owner.DepositResources();
                owner.FSM.ChangeState(MoveToResource.Instance);
            }

        }

        public override void HandleMessage(Worker owner, Message message)
        {
        }
    }

    // -----------------------------------------------------------
    //
    //   Harvest Resource Worker State
    //
    // -----------------------------------------------------------
    class HarvestResource : State<Worker>
    {
        private static HarvestResource _instance;

        public static HarvestResource Instance
        {
            get { return _instance; }
        }

        public HarvestResource()
        {
            if (_instance == null)
                _instance = this;
            else
                throw new InvalidOperationException("Cannot have more than one instance of Worker Harvest Resource State");
        }

        public override void Enter(Worker owner)
        {
            // If resource was depleted while moving towards it, find another one.
            if (owner.TargetResource == null)
            {
                owner.FSM.ChangeState(MoveToResource.Instance);
                return;
            }

            // If arrived at a saturated resource, re-evaluate.
            if (owner.TargetResource.IsSaturated)
            {
                owner.FSM.ChangeState(MoveToResource.Instance);
                return;
            }

            if (owner.resrcLookingFor == ResourceType.Wood)
                owner.ChangeAnimation(WorkerAnimation.Chop);
            else if (owner.resrcLookingFor == ResourceType.Stone)
                owner.ChangeAnimation(WorkerAnimation.Mine);

            owner.TargetResource.AddHarvester(owner);
            owner.Steering.separationOn = false;
            owner.timeSpentHarvesting = 0;
            owner.FollowPath = false;
            owner.Vel = Vector2.Zero;
        }

        public override void Exit(Worker owner)
        {
            owner.ChangeAnimation(WorkerAnimation.Walk);
            owner.Steering.separationOn = true;
            owner.timeSpentHarvesting = 0;

            if (owner.TargetResource != null)
                owner.TargetResource.RemoveHarvester(owner);
        }

        public override void Execute(Worker owner)
        {
            if (++owner.timeSpentHarvesting >= Resource.HARVEST_DURATION)
            {
                owner.TargetResource.GiveResources(owner);
                owner.FSM.ChangeState(ReturnResource.Instance);
            }
        }

        public override void HandleMessage(Worker owner, Message message)
        {
        }
    }

    // -----------------------------------------------------------
    //
    //   Move To Construct Building Worker State
    //
    // -----------------------------------------------------------
    class MoveToConstructBuilding : State<Worker>
    {
        private static MoveToConstructBuilding _instance;

        public static MoveToConstructBuilding Instance
        {
            get { return _instance; }
        }

        public MoveToConstructBuilding()
        {
            if (_instance == null)
                _instance = this;
            else
                throw new InvalidOperationException("Cannot have more than one instance of Worker Harvest Resource State");
        }

        public override void Enter(Worker owner)
        {
            owner.pathHandler.GetPathToBuilding(owner.constructing);
        }

        public override void Exit(Worker owner)
        {
        }

        public override void Execute(Worker owner)
        {
            if (owner.CollisionRect.GetInflated(10, 10).Intersects(owner.constructing.CollisionRect))
                owner.FSM.ChangeState(ConstructBuilding.Instance);
        }

        public override void HandleMessage(Worker owner, Message message)
        {
        }
    }

    // -----------------------------------------------------------
    //
    //   Construct Building Worker State
    //
    // -----------------------------------------------------------
    class ConstructBuilding : State<Worker>
    {
        private static ConstructBuilding _instance;

        public static ConstructBuilding Instance
        {
            get { return _instance; }
        }

        public ConstructBuilding()
        {
            if (_instance == null)
                _instance = this;
            else
                throw new InvalidOperationException("Cannot have more than one instance of Worker Harvest Resource State");
        }

        public override void Enter(Worker owner)
        {
            owner.ChangeAnimation(WorkerAnimation.Chop);
            owner.FollowPath = false;
        }

        public override void Exit(Worker owner)
        {
            owner.ChangeAnimation(WorkerAnimation.Walk);
            owner.constructing = null;
        }

        public override void Execute(Worker owner)
        {
            owner.constructing.Construct();

            if (owner.constructing.IsActive)
              owner.ExitState();
        }

        public override void HandleMessage(Worker owner, Message message)
        {
        }
    }
}
