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
    /// Extends Entity with velocity, path following and goal planning / execution capabilities.
    /// </summary>
    public class Unit : Entity
    {
        public static float NEIGHBOUR_RADIUS = 21;

        /// <summary>
        /// Current velocity of the unit.
        /// </summary>
        public Vector2 Vel;

        /// <summary>
        /// Path finding component. Requests paths from Pathfinder and handles unit movement along path.
        /// </summary>
        public PathHandler pathHandler;

        public PlayerData _data;
        public SteeringBehaviours _steering;

        /// <summary>
        /// Specifies if the unit's PathHandler should update.
        /// </summary>
        public bool FollowPath { get; set; }

        public Unit(PlayerData data, Vector2 pos = new Vector2(), Vector2 scale = new Vector2()) : base(pos, scale)
        {
            Vel = new Vector2();
            pathHandler = new PathHandler(this, data.world.Grid);
            _steering = new SteeringBehaviours(this, data);
            _data = data;
        }

        /// <summary>
        /// Called once per frame, handles all relevant updating methods including PathHandler.
        /// </summary>
        public override void Update()
        {
            Vector2 vel = Vector2.Zero;
            vel += _steering.Calculate();

            if (FollowPath && pathHandler.HasPath)
                vel += pathHandler.Update();

            Vel = vel;

            Pos += Vel * 2;

            _steering.EnforceZeroOverlap();
        }        

        public override void Render(SpriteBatch spriteBatch)
        {
            spriteBatch.FillRectangle(RenderRect, Color.DarkBlue);
        }

        public override void HandleMessage()
        {
            return;
        }

        public override void RenderDebug(SpriteBatch spriteBatch)
        {
            // TODO: Maybe some stuff in here about collision boxes or... something?
            // Possibly can encapuslate some of the World Debug stuff here.
        }
    }
}
