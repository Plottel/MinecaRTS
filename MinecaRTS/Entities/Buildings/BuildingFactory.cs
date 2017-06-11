using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    /// <summary>
    /// Factory class to normalize all building construction methods to take a PlayerData and position as parameters.
    /// </summary>
    public static class BuildingFactory
    {
        public static TownHall CreateTownHall(PlayerData data, Vector2 pos)
        {
            return new TownHall(pos, 
                new Vector2(127, 127), 
                new List<Type> { typeof(Worker), typeof(Minecart)}, 
                data);
        }

        public static House CreateHouse(PlayerData data, Vector2 pos)
        {
            return new House(pos, data.Team);
        }

        public static Track CreateTrack(PlayerData data, Vector2 pos)
        {
            return new Track(pos, data.Team);
        }

        public static DepositBox CreateDepositBox(PlayerData data, Vector2 pos)
        {
            return new DepositBox(pos, data.Team);
        }

        public static Wall CreateWall(PlayerData data, Vector2 pos)
        {
            return new Wall(pos, data.Team);
        }
    }
}
