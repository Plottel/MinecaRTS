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

        public ResourceType Type;
        private Color _color;
        private int _value = MAX_VALUE;
        private HashSet<Worker> _harvesters = new HashSet<Worker>();

        public Resource(Vector2 pos, Vector2 scale, ResourceType t) : base(pos, scale)
        {
            Type = t;

            if (Type == ResourceType.Wood)
                _color = new Color(102, 51, 0);
            else if (Type == ResourceType.Stone)
                _color = new Color(64, 64, 64);
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
            if (_value == 100)
                spriteBatch.FillRectangle(RenderRect.GetInflated(-4, -4), _color);
            else if (_value > 80)
                spriteBatch.FillRectangle(RenderRect.GetInflated(-6, -6), _color);
            else if (_value > 60)
                spriteBatch.FillRectangle(RenderRect.GetInflated(-8, -8), _color);
            else if (_value > 40)
                spriteBatch.FillRectangle(RenderRect.GetInflated(-10, -10), _color);
            else if (_value > 20)
                spriteBatch.FillRectangle(RenderRect.GetInflated(-12, -12), _color);
            else
                spriteBatch.FillRectangle(RenderRect.GetInflated(-13, -13), _color);
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
