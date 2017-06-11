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
    /// A building which provides supply for the team it belongs to.
    /// </summary>
    public class House : Building, IBoostsSupply
    {
        /// <summary>
        /// Maximum health for Houses.
        /// </summary>
        public const int MAX_HEALTH = 300;

        /// <summary>
        /// Texture to be rendered when construction is complete.
        /// </summary>
        public static Texture2D ACTIVE_TEXTURE;

        /// <summary>
        /// Texture to be rendered while being constructed.
        /// </summary>
        public static Texture2D CONSTRUCTION_TEXTURE;

        /// <summary>
        /// Gets the amount that the House boost supply by.
        /// </summary>
        public uint SupplyBoostAmount
        {
            get { return 5; }
        }

        /// <summary>
        /// Initializes a new house.
        /// </summary>
        /// <param name="pos">The position</param>
        /// <param name="team">The team the house belongs to</param>
        public House(Vector2 pos, Team team) : base(pos, new Vector2(63, 63), team, MAX_HEALTH, ACTIVE_TEXTURE, CONSTRUCTION_TEXTURE)
        {
        }
    }
}
