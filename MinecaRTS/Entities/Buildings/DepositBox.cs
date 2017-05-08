using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MinecaRTS
{
    public class DepositBox : Building, ICanAcceptResources
    {
        public const int MAX_HEALTH = 100;
        public static Texture2D ACTIVE_TEXTURE;
        public static Texture2D CONSTRUCTION_TEXTURE;

        private uint _wood;
        private uint _stone;

        public uint Wood
        {
            get { return _wood; }
        }

        public uint Stone
        {
            get { return _stone; }
        }

        public DepositBox(Vector2 pos, Team team) : base(pos, new Vector2(63, 63), team, MAX_HEALTH, ACTIVE_TEXTURE, CONSTRUCTION_TEXTURE)
        {
        }

        public void AcceptResources(uint woodAmount, uint stoneAmount)
        {
            _wood += woodAmount;
            _stone += stoneAmount;
        }

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
