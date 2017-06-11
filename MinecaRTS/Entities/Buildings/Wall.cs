using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MinecaRTS
{
    /// <summary>
    /// A building which does nothing but sits there as a Wall.
    /// Used for fortification.
    /// </summary>
    public class Wall : Building
    {
        /// <summary>
        /// The maximum health.
        /// </summary>
        public const int MAX_HEALTH = 50;

        /// <summary>
        /// Texture to be rendered while active.
        /// </summary>
        public static Texture2D ACTIVE_TEXTURE;

        /// <summary>
        /// Texture to be rendered while being constructed.
        /// </summary>
        public static Texture2D CONSTRUCTION_TEXTURE;

        /// <summary>
        /// Initialies a new Wall
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="team">The team the wall belongs to</param>
        public Wall(Vector2 pos, Team team) : base (pos, new Vector2(31, 31), team, MAX_HEALTH, ACTIVE_TEXTURE, CONSTRUCTION_TEXTURE)
        {
        }
    }
}
