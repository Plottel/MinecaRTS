using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MinecaRTS
{ 
    // TODO: This is a dodgy class for lack of an animation text parser.
    // Contains a bunch of predefined factory methods.
    public static class AnimationCreator
    {
        public static List<Frame> CreateWorkerWalk()
        {
            SpriteSheet spriteSheet = Worker.walkSS;
            int width = spriteSheet.texture.Width / spriteSheet.cols;
            int height = spriteSheet.texture.Height / spriteSheet.rows;

            // Create S direction animation.
            // Row 0, Col 0 - 6
            // Defining Animation -> Direction -> Frame
            List<Frame> southFrames = new List<Frame>();

            // Setup rect locations in loop simply.
            for (int i = 0; i < Worker.walkSS.cols; i++)
            {
                Frame newFrame = new Frame(new Rectangle(Worker.walkSS.cellWidth * i,
                                            0,
                                            Worker.walkSS.cellWidth,
                                            Worker.walkSS.cellHeight), 4);
                southFrames.Add(newFrame);
            }

            return southFrames;

            // Each row is a direction


            // To access row: int row = (int)((float)currentFrame / (float)Columns);
            // To access col: int column = currentFrame % Columns;
        }
    }
}
