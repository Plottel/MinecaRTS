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
            if (owner.resourceTarget != null && owner.resourceTarget.Resource != null)
                owner.resourceTarget.Resource.Detach();

            owner.pathHandler.GetPathToClosestResource(owner.resourceType);
            owner.resourceTarget = owner.pathHandler._path.Last();
            owner.resourceTarget.Resource.Attach();
        }

        public override void Exit(Worker owner)
        {
            owner.resourceTarget.Resource.Detach();
        }

        public override void Execute(Worker owner)
        {
            if (owner.CollisionRect.GetInflated(5, 5).Intersects(owner.resourceTarget.CollisionRect))
                owner.FSM.ChangeState(ReturnResource.Instance);
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
            // Get path to base.
            owner.pathHandler.GetPathTo(owner.resourceDropOff.Mid);
        }

        public override void Exit(Worker owner)
        {
            return;
        }

        public override void Execute(Worker owner)
        {
            if (owner.CollisionRect.GetInflated(5, 5).Intersects(owner.resourceDropOff.CollisionRect))           
                owner.FSM.ChangeState(MoveToResource.Instance);
        }
    }
}
