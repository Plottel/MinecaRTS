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
    public class Resource : Entity
    {
        public const int HARVEST_DURATION = 300;
        public const int MAX_HARVESTERS = 3;

        public ResourceType Type;
        private Color _color;
        private int _value = 100;
        private HashSet<Worker> _harvesters = new HashSet<Worker>();

        public Resource(Vector2 pos, Vector2 scale, ResourceType t) : base(pos, scale)
        {
            Type = t;

            if (Type == ResourceType.Wood)
                _color = new Color(102, 51, 0);
            else if (Type == ResourceType.Stone)
                _color = new Color(64, 64, 64);
        }

        public bool IsSaturated
        {
            get { return _harvesters.Count >= 3; }
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
            spriteBatch.FillRectangle(RenderRect.GetInflated(-4, -4), _color);
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
