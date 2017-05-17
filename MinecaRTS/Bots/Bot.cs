using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace MinecaRTS
{
    public abstract class Bot : Player
    {
        public Bot(PlayerData data) : base(data)
        {
        }

        public override void HandleInput()
        {
        }

        public abstract void OnUnitSpawn(Unit newUnit);
        public abstract void OnProductionBuildingTaskComplete(ProductionBuilding prodBuild);
        public abstract void OnSupplyChange();
        public abstract void OnBuildingComplete(Building building);

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
            }
        }

        public override void Render(SpriteBatch spriteBatch)
        {
        }
    }
}
