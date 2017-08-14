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
    /// The Minecart Tracks in the game. Minecarts move much faster on these.
    /// Unlike other buildings, Tracks can be walked over.
    /// </summary>
    public class Track : Building
    {
        /// <summary>
        /// Max health for tracks.
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
        /// Initializes a new Track.
        /// </summary>
        /// <param name="pos">The position</param>
        /// <param name="team">The team the track belongs to</param>
        public Track(Vector2 pos, Team team) : base (pos, new Vector2(31, 31), team, MAX_HEALTH, ACTIVE_TEXTURE, CONSTRUCTION_TEXTURE)
        {
        }
    }
}
