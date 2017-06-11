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
    /// <summary>
    /// The base class for all buildings capable of production.
    /// Manages a list of Types indicating what the building can produce, and another list of Types
    /// indicating what the building is currently producing.
    /// Sends messages to the World whenever a task is complete.
    /// </summary>
    public class ProductionBuilding : Building
    {
        /// <summary>
        /// Maximum health for production buildings.
        /// </summary>
        public const int MAX_HEALTH = 500;

        /// <summary>
        /// Maximum number of items which can be added to the production queue at once.
        /// </summary>
        public const int MAX_QUEUE_LENGTH = 5;

        /// <summary>
        /// Maps Types to the amount of time it takes to produce them.
        /// </summary>
        public static Dictionary<Type, uint> productionTimes = new Dictionary<Type, uint>();

        /// <summary>
        /// The types this production building can produce.
        /// </summary>
        private List<Type> _productionTypes;

        /// <summary>
        /// Whether or not the production building is currently producing anything
        /// </summary>
        private bool _isProducing;

        /// <summary>
        /// The list of types currently being produced.
        /// First index represents first type being produced and so on..
        /// </summary>
        private List<Type> _productionQueue;

        /// <summary>
        /// How much time has been spent producing the Type currently being produced.
        /// </summary>
        private uint _timeSpentProducing;

        /// <summary>
        /// Gets the list of types this production building can produce.
        /// </summary>
        public List<Type> ProductionTypes
        {
            get { return _productionTypes; }
        }

        /// <summary>
        /// Where units produced by this production building will path towards when created.
        /// </summary>
        public Vector2 rallyPoint;

        /// <summary>
        /// The PlayerData for the team this production building belongs to.
        /// Used to check if entities can be afforded.
        /// </summary>
        private PlayerData _data;
        
        /// <summary>
        /// Gets the PlayerData for the team this production building belongs to.
        /// </summary>
        protected PlayerData Data
        {
            get { return _data; }
        }

        /// <summary>
        /// Gets the type currently being produced by this production building.
        /// </summary>
        public Type BeingProduced
        {
            get
            {
                if (_productionQueue.Count > 0)
                    return _productionQueue[0];
                else
                    return null;
            }
        }

        /// <summary>
        /// Initializes a new Production Building.
        /// </summary>
        /// <param name="pos">The position to create at.</param>
        /// <param name="scale">The size</param>
        /// <param name="productionTypes">The types that can be produced</param>
        /// <param name="data">The PlayerData for querying</param>
        /// <param name="activeTexture">Texture to be rendered while active</param>
        /// <param name="constructionTexture">Texture to be rendered while being constructed</param>
        public ProductionBuilding(Vector2 pos, Vector2 scale, List<Type> productionTypes, PlayerData data, Texture2D activeTexture, Texture2D constructionTexture) : base(pos, scale, data.Team, MAX_HEALTH, activeTexture, constructionTexture)
        {
            _productionTypes = productionTypes;
            _isProducing = false;
            _productionQueue = new List<Type>();
            _timeSpentProducing = 0;
            _data = data;
            rallyPoint = new Vector2(pos.X + (scale.X / 2), pos.Y + scale.Y);
        }

        /// <summary>
        /// Resets the rally point to the default of bottom middle of the building's area.
        /// </summary>
        public void ResetRallyPoint()
        {
            rallyPoint = new Vector2(Mid.X, CollisionRect.Bottom);
        }

        /// <summary>
        /// Adds to the production queue the Type at the passed in index in _productionTypes list
        /// </summary>
        /// <param name="index">The index to queue up production at</param>
        public override void QueueUpProductionAtIndex(int index)
        {
            if (IsActive)
            {
                // Only build if there's room in the queue
                if (_productionQueue.Count < MAX_QUEUE_LENGTH)
                {
                    // Only build if passed a valid index and player can afford unit
                    if (index < _productionTypes.Count && _data.CanAffordEntityType(_productionTypes[index]))
                    {
                        _data.SpendResourcesForUnitType(_productionTypes[index]);
                        _isProducing = true;
                        _productionQueue.Add(_productionTypes[index]);
                    }
                }
            }                                
        }

        /// <summary>
        /// Increments time spent producing.
        /// If enough time has been spent producing the current Type, a message is sent to the world to create
        /// a new entity and production on the next type begins.
        /// </summary>
        public override void Update()
        {
            if (IsActive)
            {
                base.Update();

                if (_isProducing)
                {
                    if (++_timeSpentProducing >= productionTimes[BeingProduced])
                    {
                        _timeSpentProducing = 0;

                        MsgBoard.AddMessage(this, World.MSG_ID, MessageType.UnitSpawned, info:BeingProduced);
                        _productionQueue.RemoveAt(0);

                        if (_productionQueue.Count == 0)
                            _isProducing = false;
                    }
                }
            }                      
        }

        /// <summary>
        /// Renders the production building and a progress bar if it is producing anything.
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Render(SpriteBatch spriteBatch)
        {
            base.Render(spriteBatch);

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
