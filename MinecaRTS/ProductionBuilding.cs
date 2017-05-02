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
    public class ProductionBuilding : Building
    {
        public const int MAX_HEALTH = 200;
        public const int MAX_QUEUE_LENGTH = 5;
        public static Dictionary<Type, uint> productionTimes = new Dictionary<Type, uint>();

        List<Type> _productionTypes;
        private bool _isProducing;
        private List<Type> _productionQueue;
        private uint _timeSpentProducing;
        protected PlayerData _data;      

        private Type BeingProduced
        {
            get
            {
                if (_productionQueue.Count > 0)
                    return _productionQueue[0];
                else
                    return null;
            }
        }

        public ProductionBuilding(Vector2 pos, Vector2 scale, List<Type> productionTypes, PlayerData data) : base(pos, scale, data.Team, MAX_HEALTH)
        {
            _productionTypes = productionTypes;
            _isProducing = false;
            _productionQueue = new List<Type>();
            _timeSpentProducing = 0;
            _data = data;
        }

        public override void HandleInput(int index)
        {
            if (isActive)
            {
                // Only build if there's room in the queue
                if (_productionQueue.Count < MAX_QUEUE_LENGTH)
                {
                    // Only build if passed a valid index and player can afford unit
                    if (index < _productionTypes.Count && _data.CanAffordEntityType(_productionTypes[index]))
                    {
                        _data.BuyUnit(_productionTypes[index]);
                        _isProducing = true;
                        _productionQueue.Add(_productionTypes[index]);
                    }
                }
            }                                
        }

        public override void Update()
        {
            if (isActive)
            {
                base.Update();

                if (_isProducing)
                {
                    if (++_timeSpentProducing >= productionTimes[BeingProduced])
                    {
                        _timeSpentProducing = 0;

                        // TODO: Could use reflection here but perhaps slower - and run into issues with constructors with parameters.
                        if (BeingProduced == typeof(Worker))
                        {
                            _data.world.AddWorker(new Vector2(Mid.X, CollisionRect.Bottom), _data.Team);
                            _productionQueue.RemoveAt(0);
                        }

                        if (_productionQueue.Count == 0)
                            _isProducing = false;

                    }
                }
            }                      
        }

        public override void Render(SpriteBatch spriteBatch)
        {
            base.Render(spriteBatch);

            if (!isActive)
            {
                RenderInactive(spriteBatch);
            }
            else
            {
                spriteBatch.Draw(townHallTexture, Camera.VecToScreen(Pos), Color.White);

                // Render production progress bar
                if (_isProducing)
                {
                    float rectWidth = Scale.X * ((float)_timeSpentProducing / (float)productionTimes[BeingProduced]);
                    Rectangle rect = new Rectangle(RenderRect.X, RenderRect.Y - 10, (int)rectWidth, 8);

                    spriteBatch.FillRectangle(rect, Color.MediumTurquoise);
                }

                // Render list of items in queue
                for (int i = 0; i < _productionQueue.Count; i++)
                {
                    Vector2 pos = new Vector2(Pos.X, Pos.Y + (i * 10));
                    spriteBatch.DrawString(MinecaRTS.smallFont, _productionQueue[i].Name, Camera.VecToScreen(pos), Color.White);
                }
            }           
        }
    }
}
