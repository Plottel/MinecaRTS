using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace MinecaRTS
{
    /// <summary>
    /// The base class defining common behaviour for all Players.
    /// This includes Bots and HumanPlayers.
    /// The world will simply hold a collection of Players to run the game.
    /// </summary>
    public abstract class Player : IHandleMessages
    {
        /// <summary>
        /// The PlayerData through which all requests are filtered.
        /// </summary>
        private PlayerData _data;

        /// <summary>
        /// The messaging ID.
        /// </summary>
        private ulong _id;

        /// <summary>
        /// Gets the messaging ID.
        /// </summary>
        public ulong ID
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets the PlayerData through which all requests are filtered.
        /// </summary>
        protected PlayerData Data
        {
            get { return _data; }
        }

        /// <summary>
        /// Initializes a new Player.
        /// </summary>
        /// <param name="data">The PlayerData through which all requests are filtered.</param>
        public Player(PlayerData data)
        {
            _data = data;
            _id = MsgHandlerRegistry.NextID;
            MsgHandlerRegistry.Register(this);
        }

        /// <summary>
        /// The method defining all logic for the Player. Called once per tick.
        /// </summary>
        public abstract void HandleInput();

        /// <summary>
        /// The method defining what happens when a Message is sent to the player.
        /// </summary>
        /// <param name="message">The message.</param>
        public abstract void HandleMessage(Message message);

        /// <summary>
        /// The render method for the player.
        /// </summary>
        /// <param name="spriteBatch"></param>
        public abstract void Render(SpriteBatch spriteBatch);
    }
}
