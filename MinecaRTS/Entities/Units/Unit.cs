using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Diagnostics;

namespace MinecaRTS
{
    /// <summary>
    /// Extends Entity with velocity, path following and goal planning / execution capabilities.
    /// </summary>
    public class Unit : Entity
    {
        public static float NEIGHBOUR_RADIUS = 21;

        protected Dir lastHeading;
        protected Dir heading;

        private Team _team;

        public float Speed { get; set; }

        public Team Team
        {
            get { return _team; }
        }

        /// <summary>
        /// Current velocity of the unit.
        /// </summary>
        public Vector2 Vel;

        /// <summary>
        /// Path finding component. Requests paths from Pathfinder and handles unit movement along path.
        /// </summary>
        public PathHandler pathHandler;

        private PlayerData _data;

        public Vector2 lastMid;

        public PlayerData Data
        {
            get { return _data; }
        }

        private SteeringBehaviours _steering;

        public SteeringBehaviours Steering
        {
            get { return _steering; }
        }

        public Animation animation;

        /// <summary>
        /// Specifies if the unit's PathHandler should update.
        /// </summary>
        public bool FollowPath { get; set; }

        public Unit(PlayerData data, Team team, Vector2 pos, Vector2 scale) : base(pos, scale)
        {
            Vel = new Vector2();
            _steering = new SteeringBehaviours(this, data);
            _data = data;
            _team = team;            
            heading = Dir.S;
            lastHeading = Dir.S;
            lastMid = pos;
        }              

        /// <summary>
        /// Called once per frame, handles all relevant updating methods including PathHandler.
        /// </summary>
        public override void Update()
        {
            lastMid = Mid;
            // Remove from old collision cell
            Data.RemoveUnitFromCollisionCells(this);

            lastHeading = heading;

            if (Vel != Vector2.Zero)
                heading = Utils.VectorToDir(Vel);           

            Vector2 vel = Vector2.Zero;
            vel += Steering.Calculate();

            if (FollowPath && pathHandler.HasPath)
                vel += pathHandler.Update();

            Vel = vel;

            if (Vel != Vector2.Zero)
                Vel.Normalize();

            Vel *= Speed;

            Pos += Vel;           

            // Zero overlap units makes it super jittery.
            Steering.ZeroOverlapCells();
            //_steering.ZeroOverlapUnits();

            // Add to new collision cell
            Data.AddUnitToCollisionCells(this);

            if (lastMid != Mid)
                Data.UpdateFogOfWarForUnit(this);
        }

        public virtual void Stop()
        {
            FollowPath = false;
            Vel = Vector2.Zero;
            ExitState();
        }
        
        public virtual void MoveTowards(Vector2 pos)
        {
            Cell cellAtPos = Data.Grid.CellAt(pos);

            if (cellAtPos.Passable)
            {
                pathHandler.GetPathTo(pos);
                ExitState(); // Change to "Neutral state"
            }            
        }

        public override void Render(SpriteBatch spriteBatch)
        {
            animation.Render(spriteBatch, new Rectangle(Camera.PtToScreen(Pos.ToPoint()), Scale.ToPoint()));
        }

        public override void HandleMessage(Message message)
        {
            return;
        }

        public override void RenderDebug(SpriteBatch spriteBatch)
        {
            // TODO: Maybe some stuff in here about collision boxes or... something?
            // Possibly can encapuslate some of the World Debug stuff here.
        }

        public override void ExitState()
        {
        }
    }
}
