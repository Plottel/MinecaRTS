using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    public interface ICanAcceptResources
    {
        Rectangle CollisionRect
        {
            get;
        }

        void AcceptResources(uint woodAmount, uint stoneAmount);

    }
}
