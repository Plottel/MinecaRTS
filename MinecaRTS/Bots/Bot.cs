using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace MinecaRTS
{
    /// <summary>
    /// The base class defining all common functionality for Bots.
    /// Defines various abstract helper methods to be overridden in concrete bots.
    /// </summary>
    public abstract class Bot : Player
    {
        /// <summary>
        /// Initializes a new Bot.
        /// </summary>
        /// <param name="data">The PlayerData through which all queries are filtered.</param>
        public Bot(PlayerData data) : base(data)
        {
        }

        /// <summary>
        /// Where core Bot logic should be defined.
        /// This is called once per tick.
        /// </summary>
        public override void HandleInput()
        {
        }

        /// <summary>
        /// Invoked whenever a new unit is spawned. 
        /// </summary>
        /// <param name="newUnit">The unit that was spawned.</param>
        public abstract void OnUnitSpawn(Unit newUnit);

        /// <summary>
        /// Invoked whenever a production building finishes its task. It may or may not have more tasks in its queue.
        /// </summary>
        /// <param name="prodBuild">The production building that finished its task.</param>
        public abstract void OnProductionBuildingTaskComplete(ProductionBuilding prodBuild);

        /// <summary>
        /// Invoked whenever supply changes. This could be a unit spawning, a unit dying, a house being completed,
        /// or a house being destroyed.
        /// </summary>
        public abstract void OnSupplyChange();

        /// <summary>
        /// Invoked whenever construction on a building finishes.
        /// </summary>
        /// <param name="building">The newly completed building.</param>
        public abstract void OnBuildingComplete(Building building);

        /// <summary>
        /// Invoked whenever the resource values of the Bot changes.
        /// This includes Wood, Stone but NOT supply.
        /// </summary>
        public abstract void OnResourcesReceived();

        /// <summary>
        /// The method through which all Messages sent to the bot will go through.
        /// Filters the message and calls the relevant method which concrete bots will define.
        /// </summary>
        /// <param name="message">The message.</param>
        public override sealed void HandleMessage(Message message)
        {
            switch (message.type)
            {
                case MessageType.UnitSpawned:
                    OnUnitSpawn(message.extraInfo); break;

                case MessageType.ProductionBuildingTaskComplete:
                    OnProductionBuildingTaskComplete(message.extraInfo); break;

                case MessageType.SupplyChanged:
                    OnSupplyChange(); break;

                case MessageType.BuildingComplete:
                    OnBuildingComplete(message.extraInfo); break;

                case MessageType.ResourcesReceived:
                    OnResourcesReceived(); break;
            }
        }

        /// <summary>
        /// The render function for the bot.
        /// Left blank by default, but can be overridden by concrete bots for debug information e.g..
        /// </summary>
        /// <param name="spriteBatch">The spriteBatch to render to.</param>
        public override void Render(SpriteBatch spriteBatch)
        {
        }
    }
}
