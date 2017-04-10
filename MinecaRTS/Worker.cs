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
        // TODO: This will eventually be an Interface rather than Building
        // to accommodate town centres and minecarts together.

        public Building resourceDropOff;
        public Cell resourceTarget;
        public ResourceType resourceType = ResourceType.None;

        private StateMachine<Worker> _fsm;


        public StateMachine<Worker> FSM
        {
            get { return _fsm; }
        }

        public Worker(PlayerData data, Vector2 pos, Vector2 scale) : base(data, pos, scale)
        {
            _fsm = new StateMachine<Worker>(this);
        }

        public override void Update()
        {
            base.Update();
            _fsm.Execute();
        }

        public override void HandleMessage()
        {
            return;
        }

        public override void RenderDebug(SpriteBatch spriteBatch)
        {
            base.RenderDebug(spriteBatch);

            if (FSM.CurrentState != null)
                spriteBatch.DrawString(Debug.debugFont, FSM.CurrentState.GetType().Name, RenderMid - Scale / 2, Color.Black);
        }
    }
}
