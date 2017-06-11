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
    /// The Town Hall which serves as a resource return point and can produce Workers and Minecarts.
    /// </summary>
    public class TownHall : ProductionBuilding, ICanAcceptResources
    {
        /// <summary>
        /// Texture to be rendered while construction is complete.
        /// </summary>
        public static Texture2D ACTIVE_TEXTURE;

        /// <summary>
        /// Texture to be rendered while being constructed.
        /// </summary>
        public static Texture2D CONSTRUCTION_TEXTURE;

        /// <summary>
        /// Initializes a new Town Hall.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="scale">The size.</param>
        /// <param name="productionTypes">The Types the Town Hall can produce</param>
        /// <param name="data">The PlayeData for querying</param>
        public TownHall(Vector2 pos, Vector2 scale, List<Type> productionTypes, PlayerData data) : base (pos, scale, productionTypes, data, ACTIVE_TEXTURE, CONSTRUCTION_TEXTURE)
        { }

        /// <summary>
        /// Gives resources to the PlayerData equal to the passed in amounts.
        /// </summary>
        /// <param name="woodAmount">The wood amount to give</param>
        /// <param name="stoneAmount">The stone amount to give</param>
        public void AcceptResources(uint woodAmount, uint stoneAmount)
        {
            Data.ReceiveResources(woodAmount, ResourceType.Wood);
            Data.ReceiveResources(stoneAmount, ResourceType.Stone);
        }
    }
}
