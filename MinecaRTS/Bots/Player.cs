using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace MinecaRTS
{
    public abstract class Player : IHandleMessages
    {
        private PlayerData _data;
        private ulong _id;

        public ulong ID
        {
            get { return _id; }
        }

        protected PlayerData Data
        {
            get { return _data; }
        }

        public Player(PlayerData data)
        {
            _data = data;
            _id = MsgHandlerRegistry.NextID;
            MsgHandlerRegistry.Register(this);
        }

        public abstract void HandleInput();
        public abstract void HandleMessage(Message message);
        public abstract void Render(SpriteBatch spriteBatch);
    }
}
