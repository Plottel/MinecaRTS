using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MinecaRTS
{
    public class ProductionBuilding : Building
    {
        public static Dictionary<Type, uint> productionTimes = new Dictionary<Type, uint>();

        List<Type> _productionTypes;
        private bool _producing;
        private Type _producingType;
        private uint _timeSpentProducing;
        private PlayerData _data;

        public ProductionBuilding(Vector2 pos, Vector2 scale, List<Type> productionTypes, PlayerData data) : base(pos, scale)
        {
            _productionTypes = productionTypes;
            _producing = false;
            _producingType = null;
            _timeSpentProducing = 0;
            _data = data;
        }

        public void StartProducingTypeAtIndex(int index)
        {
            _producing = true;
            _producingType = _productionTypes[index];
        }

        public override void Update()
        {
            base.Update();

            Debug.HookText("Time Spent Producing: " + _timeSpentProducing.ToString());

            if (_producing)
            {
                if (++_timeSpentProducing >= productionTimes[_producingType])
                {
                    _timeSpentProducing = 0;
                    _producing = false;

                    // TODO: Could use reflection here but perhaps slower - and run into issues with constructors with parameters.
                    if (_producingType == typeof(Worker))
                    {
                        _data.world.AddWorker(new Vector2(Mid.X, CollisionRect.Bottom)); 
                    }
                    
                }
            }                
        }
    }
}
