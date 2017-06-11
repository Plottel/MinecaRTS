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
    /// Extends Entity with steering behaviour and pathing functionality.
    /// </summary>
    public class Unit : Entity
    {
        /// <summary>
        /// Neighbourhood radius for steering behaviour calulations.
        /// </summary>
        public static float NEIGHBOUR_RADIUS = 21;

        /// <summary>
        /// Which compass direction the unit was facing last tick.
        /// </summary>
        protected Dir lastHeading;

        /// <summary>
        /// Current compass direction the unit is facing.
        /// </summary>
        protected Dir heading;

        /// <summary>
        /// The team the unit belongs to.
        /// </summary>
        private Team _team;

        /// <summary>
        /// The speed of the unit.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Gets the team the Unit belongs to.
        /// </summary>
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

        /// <summary>
        /// The PlayerData for the team the unit belongs to.
        /// Used for querying.
        /// </summary>
        private PlayerData _data;

        /// <summary>
        /// The middle position of the unit last tick.
        /// Used for collision cell and fog of war calculations.
        /// </summary>
        public Vector2 lastMid;

        /// <summary>
        /// Gets the PlayerData used for querying.
        /// </summary>
        public PlayerData Data
        {
            get { return _data; }
        }

        /// <summary>
        /// The steering behaviours component for the unit.
        /// </summary>
        private SteeringBehaviours _steering;

        /// <summary>
        /// Gets the steering behaviours component for the unit.
        /// </summary>
        public SteeringBehaviours Steering
        {
            get { return _steering; }
        }

        /// <summary>
        /// The current animation the unit is playing.
        /// </summary>
        public Animation animation;

        /// <summary>
        /// Specifies if the unit's PathHandler should update.
        /// </summary>
        public bool FollowPath { get; set; }

        /// <summary>
        /// Initializes a new unit.
        /// </summary>
        /// <param name="data">The PlayerData used for querying.</param>
        /// <param name="team">The team the unit belongs to.</param>
        /// <param name="pos">The position the unit is created at </param>
        /// <param name="scale">The size</param>
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
            //Data.RemoveUnitFromCollisionCells(this);

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
            //Data.AddUnitToCollisionCells(this);

            if (lastMid != Mid)
                MsgBoard.AddMessage(this, World.MSG_ID, MessageType.UnitMoved);
        }

        /// <summary>
        /// Orders the unit to stop whatever it's doing.
        /// Child classes can define their own implementations.
        /// </summary>
        public virtual void Stop()
        {
            FollowPath = false;
            Vel = Vector2.Zero;
            ExitState();
        }
        
        /// <summary>
        /// Orders the unit to start moving towards the passed in position.
        /// Child classes can define their own implementations.
        /// </summary>
        /// <param name="pos">The position to move towards</param>
        /// <param name="smoothed">Whether or not the path should be smoothed</param>
        public virtual void MoveTowards(Vector2 pos, bool smoothed = true)
        {
            Cell cellAtPos = Data.Grid.CellAt(pos);

            if (cellAtPos.Passable)
            {
                pathHandler.GetPathTo(pos, smoothed);
                ExitState(); // Change to "Neutral state"
            }            
        }

        /// <summary>
        /// Renders the unit's current animation.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to render to.</param>
        public override void Render(SpriteBatch spriteBatch)
        {
            animation.Render(spriteBatch, new Rectangle(Camera.PtToScreen(Pos.ToPoint()), Scale.ToPoint()));
        }
        
        /// <summary>
        /// Determines what happens when a Unit is sent a message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void HandleMessage(Message message)
        {
            return;
        }

        public override void RenderDebug(SpriteBatch spriteBatch)
        {
            // TODO: Maybe some stuff in here about collision boxes or... something?
            // Possibly can encapuslate some of the World Debug stuff here.
        }

        /// <summary>
        /// Wrapper method at Unit level to tell units to exit current state.
        /// Due to simplicity with Generics, this needs to exist at base level since 
        /// FSM variables only exist in child unit classes.
        /// </summary>
        public override void ExitState()
        {
        }
    }
}
