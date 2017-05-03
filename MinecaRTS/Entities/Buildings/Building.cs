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
    public class Building : Entity
    {
        private Texture2D _activeTexture;
        private Texture2D _constructionTexture;

        // For HumanPlayer to render the ActiveTexture when selecting which building to create
        public Texture2D ActiveTexture
        {
            get { return _activeTexture; }
        }

        private int _health = 0;
        private int _maxHealth = 0;

        private bool _isActive;

        public bool IsActive
        {
            get { return _isActive; }
        }

        private Team _team;

        public Team Team
        {
            get { return _team; }
        }

        public Building(Vector2 pos, Vector2 scale, Team team, int maxHealth, Texture2D actText, Texture2D constText) : base(pos, scale)
        {
            _team = team;
            _isActive = false;
            _maxHealth = maxHealth;
            _activeTexture = actText;
            _constructionTexture = constText;            
        }

        public void Construct()
        {
            if (!IsActive)
            {
                if (++_health >= _maxHealth)
                {
                    _isActive = true;
                    _health = _maxHealth;
                }
            }
        }

        public override void Update()
        {
            return;
        }

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

        public virtual void HandleInput(int index)
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
