﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MinecaRTS
{

    /// <summary>
    /// The abstract base class from which all game entities inherit. Each entity has an id unique to the program lifetime 
    /// which is auto assigned on object creation.
    /// Entity provides abstract methods for update, render, message handling and locating itself.
    /// </summary>
    public abstract class Entity : IRenderable, IHandleMessages
    {
        /// <summary>
        /// The entity's unique id.
        /// </summary>
        private ulong _id;

        /// <summary>
        /// The entity's position.
        /// </summary>
        public Vector2 Pos;

        /// <summary>
        /// The size of the entity - used for rendering and collisions.
        /// </summary>
        public Vector2 Scale;

        /// <summary>
        /// Gets the entity's unique id.
        /// </summary>
        public ulong ID
        {
            get { return _id; }
        }

        /// <summary>
        /// Initializes a new Entity.
        /// </summary>
        /// <param name="pos">The position the entity is created at.</param>
        /// <param name="scale">The size</param>
        protected Entity(Vector2 pos, Vector2 scale)
        {
            _id = MsgHandlerRegistry.NextID;
            MsgHandlerRegistry.Register(this);

            Pos = pos;
            Scale = scale;
        }

        ~Entity()
        {
            MsgHandlerRegistry.RemoveEntity(this);
        }

        /// <summary>
        /// The top left corner of the entity in screen co-ordinates
        /// </summary>
        public Vector2 RenderPos
        {
            get { return Camera.VecToScreen(Pos); }
        }

        /// <summary>
        /// Gets the world coordinate collision rectangle of the entity.
        /// </summary>
        public Rectangle CollisionRect
        {
            // TODO: This shouldn't create a new rectangle every time.
            get { return new Rectangle((Pos + new Vector2(3, 3)).ToPoint(), (Scale - new Vector2(6, 6)).ToPoint()); }
        }

        /// <summary>
        /// Gets the screen coordinate render rectangle of the entity.
        /// </summary>
        public Rectangle RenderRect
        {
            get { return new Rectangle(Pos.ToPoint() - Camera.Pos, Scale.ToPoint()); }
        }

        /// <summary>
        /// Gets the mid point of the world coordinate collision rectangle.
        /// </summary>
        public Vector2 Mid
        {
            get { return CollisionRect.Center.ToVector2(); }
        }

        /// <summary>
        /// Gets the mid point of the screen coordinate render rectangle.
        /// </summary>
        public Vector2 RenderMid
        {
            get { return RenderRect.Center.ToVector2(); }
        }

        public abstract void Update();
        public abstract void Render(SpriteBatch spriteBatch);
        public abstract void HandleMessage(Message message);
        public abstract void ExitState();
        public abstract void RenderDebug(SpriteBatch spriteBatch);
    }
}
