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
    public static class GameResources
    {
        public static void CreateWorkerWalkAnimation()
        {
            Worker.animOffsets.Add(WorkerAnimation.Walk, new Vector2(0, 0));
            SpriteSheet spriteSheet = Worker.walkSS;
            int width = spriteSheet.texture.Width / spriteSheet.cols;
            int height = spriteSheet.texture.Height / spriteSheet.rows;
            uint frameDuration = 4;

            var animations = new Dictionary<Dir, List<Frame>>();

            // Create animation for each direction
            for (int i = 0; i < (int)Dir.Count; i++)
            {
                // Frames for current direction.
                var newFrames = new List<Frame>();

                // Create frames for current direction.
                for (int j = 0; j < Worker.walkSS.cols; j++)
                {
                    // Each direction is stored on its own row.
                    // X determined by current index of current direction.
                    // Y determined by current direction.
                    Frame newFrame = new Frame(new Rectangle(Worker.walkSS.cellWidth * j,
                                                                Worker.walkSS.cellHeight * i,
                                                                Worker.walkSS.cellWidth,
                                                                Worker.walkSS.cellHeight), 
                                                frameDuration);

                    // Add new frame to current direction animation.
                    newFrames.Add(newFrame);
                }

                // Direction animation is complete, add to reference Dictionary.
                animations.Add((Dir)i, newFrames);
            }

            // All directions are complete, add to master animation list
            Worker.animFrames.Add(WorkerAnimation.Walk, animations);
        }

        public static void CreateWorkerChopAnimation()
        {
            float xOffset = Worker.chopSS.cellWidth - Worker.walkSS.cellWidth;
            float yOffset = Worker.chopSS.cellHeight - Worker.walkSS.cellHeight;

            Worker.animOffsets.Add(WorkerAnimation.Chop, new Vector2(xOffset, yOffset));

            uint frameDuration = 7;

            var animations = new Dictionary<Dir, List<Frame>>();

            // Create animation for each direction
            for (int i = 0; i < (int)Dir.Count; i++)
            {
                // Frames for current direction.
                var newFrames = new List<Frame>();

                // Create frames for current direction. Each direction is stored as a row in the spritesheet.
                for (int j = 0; j < Worker.chopSS.cols; j++)
                {
                    // Each direction is stored on its own row.
                    // X determined by current index of current direction.
                    // Y determined by current direction.
                    Frame newFrame = new Frame(new Rectangle(Worker.chopSS.cellWidth * j,
                                                                Worker.chopSS.cellHeight * i,
                                                                Worker.chopSS.cellWidth,
                                                                Worker.chopSS.cellHeight),
                                                frameDuration);

                    // Add new frame to current direction animation.
                    newFrames.Add(newFrame);
                }

                // Direction animation is complete, add to reference Dictionary.
                animations.Add((Dir)i, newFrames);
            }

            // All directions are complete, add to master animation list
            Worker.animFrames.Add(WorkerAnimation.Chop, animations);
        }
    }
}
