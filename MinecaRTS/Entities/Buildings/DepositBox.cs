using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MinecaRTS
{
    /// <summary>
    /// A building which can have resources placed inside it and acts as a resource return point for Workers.
    /// Minecarts can collect the resources from the box and bring them back to the Town Hall.
    /// </summary>
    public class DepositBox : Building, ICanAcceptResources
    {
        /// <summary>
        /// Max Health of all Deposit Boxes.
        /// </summary>
        public const int MAX_HEALTH = 100;

        /// <summary>
        /// Texture to be rendered for Deposit Boxes when construction is finished.
        /// </summary>
        public static Texture2D ACTIVE_TEXTURE;

        /// <summary>
        /// Texture to be rendered for Deposit Boxes while they're being constructed.
        /// </summary>
        public static Texture2D CONSTRUCTION_TEXTURE;

        /// <summary>
        /// The amount of wood the Deposit Box is holding.
        /// </summary>
        private uint _wood;

        /// <summary>
        /// The amount of stone the Deposit Box is holding.
        /// </summary>
        private uint _stone;

        /// <summary>
        /// Gets the amount of wood the Deposit Box is holding.
        /// </summary>
        public uint Wood
        {
            get { return _wood; }
        }

        /// <summary>
        /// Gets the amount of stone the Deposit Box is holding.
        /// </summary>
        public uint Stone
        {
            get { return _stone; }
        }

        /// <summary>
        /// Initializes a new Deposit Box.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="team">The Team the Deposit Box belongs to.</param>
        public DepositBox(Vector2 pos, Team team) : base(pos, new Vector2(63, 63), team, MAX_HEALTH, ACTIVE_TEXTURE, CONSTRUCTION_TEXTURE)
        {
        }

        /// <summary>
        /// Adds the passed in resource values to the deposit box storage.
        /// </summary>
        /// <param name="woodAmount">The wood amount to add.</param>
        /// <param name="stoneAmount">The stone amount to add.</param>
        public void AcceptResources(uint woodAmount, uint stoneAmount)
        {
            _wood += woodAmount;
            _stone += stoneAmount;
        }

        /// <summary>
        /// Determines what happens when a Deposit Box is sent a message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void HandleMessage(Message message)
        {
            switch (message.type)
            {
                case MessageType.GiveMeResources:
                    Minecart givingResourcesTo = message.sender as Minecart;
                    givingResourcesTo.AcceptResources(_wood, _stone);
                    _wood = 0;
                    _stone = 0;
                    break;
                    
            }
        }
    }
}
