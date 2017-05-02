using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    public struct Cost
    {
        public uint woodCost;
        public uint stoneCost;
        public uint supplyCost;

        public Cost(uint wood, uint stone, uint supply)
        {
            woodCost = wood;
            stoneCost = stone;
            supplyCost = supply;
        }
    }
}
