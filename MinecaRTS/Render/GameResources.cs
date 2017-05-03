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
        private static Dictionary<Dir, List<Frame>> CreateFramesForEachDir(int cols, int cellWidth, int cellHeight, uint frameDuration)
        {
            var result = new Dictionary<Dir, List<Frame>>();

            // Create animation for each direction
            for (int i = 0; i < (int)Dir.Count; i++)
            {
                // Frames for current direction.
                var newFrames = new List<Frame>();

                // Create frames for current direction.
                for (int j = 0; j < cols; j++)
                {
                    // Each direction is stored on its own row.
                    // X determined by current index of current direction.
                    // Y determined by current direction.
                    Frame newFrame = new Frame(new Rectangle(cellWidth * j,
                                                                cellHeight * i,
                                                                cellWidth,
                                                                cellHeight),
                                                frameDuration);

                    // Add new frame to current direction animation.
                    newFrames.Add(newFrame);
                }

                // Direction animation is complete, add to reference Dictionary.
                result.Add((Dir)i, newFrames);
            }

            // All directions are complete, return result.
            return result;
        }

        public static void CreateWorkerWalkAnimation()
        {
            Worker.animOffsets.Add(WorkerAnimation.Walk, new Vector2(0, 0));
            SpriteSheet walkSS = Worker.spriteSheets[WorkerAnimation.Walk];
            uint frameDuration = 4;

            var animations = CreateFramesForEachDir(walkSS.cols, 
                walkSS.cellWidth, 
                walkSS.cellHeight, 
                frameDuration);

            // All directions are complete, add to master animation list
            Worker.animFrames.Add(WorkerAnimation.Walk, animations);
        }

        public static void CreateWorkerChopAnimation()
        {
            SpriteSheet walkSS = Worker.spriteSheets[WorkerAnimation.Walk];
            SpriteSheet chopSS = Worker.spriteSheets[WorkerAnimation.Chop];

            float xOffset = chopSS.cellWidth - walkSS.cellWidth;
            float yOffset = chopSS.cellHeight - walkSS.cellHeight;

            Worker.animOffsets.Add(WorkerAnimation.Chop, new Vector2(xOffset, yOffset));

            uint frameDuration = 7;

            var animations = CreateFramesForEachDir(chopSS.cols, 
                chopSS.cellWidth, 
                chopSS.cellHeight, 
                frameDuration);

            // All directions are complete, add to master animation list
            Worker.animFrames.Add(WorkerAnimation.Chop, animations);
        }

        public static void CreateWorkerLogsAnimation()
        {
            SpriteSheet walkSS = Worker.spriteSheets[WorkerAnimation.Walk];
            SpriteSheet logsSS = Worker.spriteSheets[WorkerAnimation.Logs];

            float xOffset = logsSS.cellWidth - walkSS.cellWidth;
            float yOffset = logsSS.cellHeight - walkSS.cellHeight;

            Worker.animOffsets.Add(WorkerAnimation.Logs, new Vector2(xOffset, yOffset));

            uint frameDuration = 4;

            var animations = CreateFramesForEachDir(logsSS.cols,
                logsSS.cellWidth,
                logsSS.cellHeight,
                frameDuration);

            // All directions are complete, add to master animation list
            Worker.animFrames.Add(WorkerAnimation.Logs, animations);
        }

        public static void CreateWorkerMineAnimation()
        {
            SpriteSheet walkSS = Worker.spriteSheets[WorkerAnimation.Walk];
            SpriteSheet mineSS = Worker.spriteSheets[WorkerAnimation.Mine];

            float xOffset = mineSS.cellWidth - walkSS.cellWidth;
            float yOffset = mineSS.cellHeight - walkSS.cellHeight;

            Worker.animOffsets.Add(WorkerAnimation.Mine, new Vector2(xOffset, yOffset));

            uint frameDuration = 7;

            var animations = CreateFramesForEachDir(mineSS.cols,
                mineSS.cellWidth,
                mineSS.cellHeight,
                frameDuration);

            // All directions are complete, add to master animation listg
            Worker.animFrames.Add(WorkerAnimation.Mine, animations);
        }

        public static void CreateWorkerBagAnimation()
        {
            SpriteSheet walkSS = Worker.spriteSheets[WorkerAnimation.Walk];
            SpriteSheet bagSS = Worker.spriteSheets[WorkerAnimation.Bag];

            float xOffset = bagSS.cellWidth - walkSS.cellWidth;
            float yOffset = bagSS.cellHeight - walkSS.cellHeight;

            Worker.animOffsets.Add(WorkerAnimation.Bag, new Vector2(xOffset, yOffset));

            uint frameDuration = 4;

            var animations = CreateFramesForEachDir(bagSS.cols,
                bagSS.cellWidth,
                bagSS.cellHeight,
                frameDuration);

            // All directions are comlpete, add to master animation list
            Worker.animFrames.Add(WorkerAnimation.Bag, animations);
        }
    }
}
