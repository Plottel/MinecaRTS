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

        public static void CreateMinecartEmptyAnimation()
        {
            uint frameDuration = uint.MaxValue;

            SpriteSheet emptySS = Minecart.emptySS;

            // All directions are complete, add to master animation list.
            Minecart.emptyAnimFrames = CreateFramesForEachDir(emptySS.cols, emptySS.cellWidth, emptySS.cellHeight, frameDuration);          
        }      
        
        public static void CreateWorkerAnimation(SpriteSheet newSS, WkrAnim animType, uint frameDuration)
        {
            SpriteSheet walkSS = Worker.spriteSheets[WkrAnim.Walk];

            float xOffset = newSS.cellWidth - walkSS.cellWidth;
            float yOffset = newSS.cellWidth - walkSS.cellWidth;

            Worker.animOffsets.Add(animType, new Vector2(xOffset, yOffset));

            var animations = CreateFramesForEachDir(newSS.cols, newSS.cellWidth, newSS.cellHeight, frameDuration);

            Worker.animFrames.Add(animType, animations);
        }       

        public static SpriteSheet LoadSpriteSheet(MinecaRTS game, string filename, int cols, int rows)
        {
            SpriteSheet toAdd;
            toAdd.texture = game.Content.Load<Texture2D>(filename);
            toAdd.cols = cols;
            toAdd.rows = rows;
            toAdd.cellWidth = toAdd.texture.Width / cols;
            toAdd.cellHeight = toAdd.texture.Height / rows;

            return toAdd;
        }
    }
}
