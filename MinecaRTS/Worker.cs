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
    public class Worker : Unit
    {
        public static SpriteSheet walkSS;
        public static List<Frame> walkFrames;


        // TODO: This will eventually be an Interface rather than Building
        // to accommodate town centres and minecarts together.

        public Building resourceReturnBuilding;
        public Cell targetResourceCell;
        public ResourceType resrcLookingFor = ResourceType.None;

        public ResourceType resrcHolding = ResourceType.None;

        public uint timeSpentHarvesting = 0;

        private StateMachine<Worker> _fsm;

        public StateMachine<Worker> FSM
        {
            get { return _fsm; }
        }

        public Resource TargetResource
        {
            get { return _data.world.GetResourceFromCell(targetResourceCell); }
        }

        public Worker(PlayerData data, Vector2 pos, Vector2 scale) : base(data, pos, scale)
        {
            _fsm = new StateMachine<Worker>(this);
        }

        public void DepositResources()
        {
            _data.GiveResources(Resource.HARVEST_AMOUNT, resrcHolding);
            resrcHolding = ResourceType.None;            
        }

        public override void Update()
        {
            base.Update();
            _fsm.Execute();
        }

        public override void HandleMessage(Message message)
        {
            FSM.HandleMessage(message);
        }

        public override void ExitState()
        {
            FSM.ChangeState(null);
        }

        public override void RenderDebug(SpriteBatch spriteBatch)   
        {
            base.RenderDebug(spriteBatch);

            if (targetResourceCell != null)
                spriteBatch.FillRectangle(targetResourceCell.RenderRect, Color.GreenYellow);

            if (FSM.CurrentState != null)
                spriteBatch.DrawString(Debug.debugFont, FSM.CurrentState.GetType().Name, RenderMid - Scale / 2, Color.Black);
        }
    }
}
