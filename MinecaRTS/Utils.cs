using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    public static class Utils
    {
        public static void Swap(ref float first, ref float second)
        {
            float temp = first;
            first = second;
            second = temp;
        }

        public static Dir VectorToDir(Vector2 v)
        {
            Vector2 vNorm = Vector2.Normalize(v);
            float thresholdValue = 0.1f;

            if (vNorm.X > 0) // Moving right
            {
                if (vNorm.Y > thresholdValue)
                    return Dir.SE;
                else if (vNorm.Y < -thresholdValue)
                    return Dir.NE;
                else
                    return Dir.E;
            }
            else if (vNorm.X < 0) // Moving left
            {
                if (vNorm.Y > thresholdValue)
                    return Dir.SW;
                else if (vNorm.Y < -thresholdValue)
                    return Dir.NW;
                else
                    return Dir.W;
            }
            else // Not moving on X axis.
            {
                if (vNorm.Y > thresholdValue)
                    return Dir.S;
                else if (vNorm.Y < -thresholdValue)
                    return Dir.N;
                else // If not moving, default to south.
                    return Dir.S;
            }
        }
    }
}
