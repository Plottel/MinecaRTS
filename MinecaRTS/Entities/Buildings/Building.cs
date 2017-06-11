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
    /// The base class for all buildings in the game.
    /// </summary>
    public class Building : Entity
    {
        /// <summary>
        /// Texture to be rendered when the building has finished construction.
        /// </summary>
        private Texture2D _activeTexture;

        /// <summary>
        /// Texture to be rendered while the building is still being constructed.
        /// </summary>
        private Texture2D _constructionTexture;

        /// <summary>
        /// Gets the current texture to be rendered by the building.
        /// </summary>
        public Texture2D ActiveTexture
        {
            get { return _activeTexture; }
        }

        /// <summary>
        /// Current health of the building.
        /// </summary>
        private int _health = 0;

        /// <summary>
        /// Maximum health of the building.
        /// </summary>
        private int _maxHealth = 0;

        /// <summary>
        /// Whether or not the building has finished construction.
        /// </summary>
        private bool _isActive;

        /// <summary>
        /// Returns whether or not the building has finished construction.
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
        }

        /// <summary>
        /// The team the building belongs to.
        /// </summary>
        private Team _team;

        /// <summary>
        /// Gets the team the building belongs to.
        /// </summary>
        public Team Team
        {
            get { return _team; }
        }

        /// <summary>
        /// Initializes a new building.
        /// </summary>
        /// <param name="pos">The position</param>
        /// <param name="scale">The size</param>
        /// <param name="team">The team</param>
        /// <param name="maxHealth">The maximmum health</param>
        /// <param name="actText">The texture to be rendered when active</param>
        /// <param name="constText">The texture to be rendered while constructing</param>
        public Building(Vector2 pos, Vector2 scale, Team team, int maxHealth, Texture2D actText, Texture2D constText) : base(pos, scale)
        {
            _team = team;
            _isActive = false;
            _maxHealth = maxHealth;
            _activeTexture = actText;
            _constructionTexture = constText;            
        }

        /// <summary>
        /// Progresses the construction of the building by incrementing its health.
        /// Once its health reaches its max health, construction is finished.
        /// Once complete, notifies the world that a new building has been completed.
        /// </summary>
        public void Construct()
        {
            if (!IsActive)
            {
                if (++_health >= _maxHealth)
                {
                    _isActive = true;
                    _health = _maxHealth;
                    MsgBoard.AddMessage(this, World.MSG_ID, MessageType.BuildingComplete);
                }
            }
        }

        public override void Update()
        {
            return;
        }

        /// <summary>
        /// The render method for the building.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to render to.</param>
        public override void Render(SpriteBatch spriteBatch)
        {
            if (IsActive)
                spriteBatch.Draw(_activeTexture, RenderRect, Color.White);
            else
            {
                spriteBatch.Draw(_constructionTexture, RenderRect, Color.White);
                RenderConstructionBar(spriteBatch);
            }                
        }

        private void RenderConstructionBar(SpriteBatch spriteBatch)
        {
            float rectWidth = Scale.X * ((float)_health / (float)_maxHealth);
            Rectangle rect = new Rectangle(RenderRect.X, RenderRect.Y - 10, (int)rectWidth, 8);

            spriteBatch.FillRectangle(rect, Color.MediumTurquoise);
        }

        public virtual void QueueUpProductionAtIndex(int index)
        { }

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
