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
    // TODO: Probably (almost certainly) doesn't need to inherit from Entity
    public class Resource : Entity
    {
        public const int HARVEST_DURATION = 180;
        public const int MAX_HARVESTERS = 3;
        public const int HARVEST_AMOUNT = 5;
        public const int MAX_VALUE = 100;

        public static Texture2D WOOD_TEXTURE;
        public static Texture2D WOOD_DEPLETED_TEXTURE;
        public static Texture2D STONE_TEXTURE;
        public static Texture2D STONE_DEPLETED_TEXTURE;

        private Texture2D texture;
        private Texture2D depletedTexture;

        /// <summary>
        /// How much bigger the texture is than 32 x 32.
        /// Depleted textures will fit within the cell, others may not.
        /// </summary>
        private Vector2 renderOffset;

        public ResourceType Type;
        private int _value = MAX_VALUE;
        private HashSet<Worker> _harvesters = new HashSet<Worker>();

        public Resource(Vector2 pos, Vector2 scale, ResourceType t) : base(pos, scale)
        {
            Type = t;

            if (Type == ResourceType.Wood)
            {
                texture = WOOD_TEXTURE;
                depletedTexture = WOOD_DEPLETED_TEXTURE;
                renderOffset = new Vector2(0, 20);
            }
            else if (Type == ResourceType.Stone)
            {
                renderOffset = new Vector2(0, 20);
                texture = WOOD_TEXTURE;
                depletedTexture = WOOD_DEPLETED_TEXTURE;
            }
        }

        public HashSet<Worker> Harvesters
        {
            get { return _harvesters; }
        }

        public bool IsSaturated
        {
            get { return _harvesters.Count >= 3; }
        }

        public bool IsDepleted
        {
            get { return _value <= 0; }
        }

        public void GiveResources(Worker harvester)
        {
            harvester.resrcHolding = Type;
            _value -= HARVEST_AMOUNT;

            if (IsDepleted)
                MsgBoard.AddMessage(this, World.MSG_ID, MessageType.ResourceDepleted);
        }

        public void RemoveHarvester(Worker harvester)
        {
            _harvesters.Remove(harvester);
        }

        public void AddHarvester(Worker harvester)
        {
            _harvesters.Add(harvester);
        }

        public bool HasHarvester(Worker harvester)
        {
            return _harvesters.Contains(harvester);
        }

        public override void Update()
        {
        }

        public override void Render(SpriteBatch spriteBatch)
        {
            if (_value >= (MAX_VALUE / 2))
                spriteBatch.Draw(texture, RenderPos - renderOffset, Color.White);
            else
                spriteBatch.Draw(depletedTexture, RenderRect, Color.White);

            if (Type == ResourceType.Stone)
                spriteBatch.DrawString(MinecaRTS.smallFont, "LOLSTONE", RenderPos, Color.White);
        }

        public override void HandleMessage(Message message)
        {
            return;
        }

        public override void ExitState()
        { }

        public override void RenderDebug(SpriteBatch spriteBatch)
        {
            return;
        }
    }
}
