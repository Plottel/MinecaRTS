using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MinecaRTS
{
    public struct SpriteSheet
    {
        public Texture2D texture;
        public int cols;
        public int rows;
        public int cellWidth;
        public int cellHeight;
    }

    public class Frame
    {
        public Rectangle rect;
        public uint duration;

        public Frame()
        { }

        public Frame(Rectangle aRect, uint aDuration)
        {
            rect = aRect;
            duration = aDuration;
        }
    }

    public class Animation
    {
        public Texture2D _texture;
        private List<Frame> _frames;
        private Vector2 _offset;
        private uint _timeOnCurrentFrame;
        private bool _looped;
        private uint _currentFrameIndex;

        public Animation(Texture2D texture, List<Frame> frames, Vector2 offset, bool looped)
        {
            _texture = texture;
            _frames = frames;
            _offset = offset;
            _looped = looped;
            _timeOnCurrentFrame = 0;
            _currentFrameIndex = 0;
        }

        public Frame CurrentFrame
        {
            get { return _frames[(int)_currentFrameIndex]; }
        }

        public void ChangeScript(List<Frame> newFrames, Vector2 offset, bool looped, bool resetIndex)
        {
            _frames = newFrames;
            _offset = offset;
            _looped = looped;

            if (resetIndex)
            {
                _currentFrameIndex = 0;
                _timeOnCurrentFrame = 0;
            }                
        }

        public void Update()
        {
            if (++_timeOnCurrentFrame >= CurrentFrame.duration)
            {
                _timeOnCurrentFrame = 0;

                if (++_currentFrameIndex >= _frames.Count && _looped)
                    _currentFrameIndex = 0;
            }
        }

        public void Render(SpriteBatch spriteBatch, Rectangle destinationRect)
        {
            spriteBatch.Draw(_texture, destinationRect.GetInflated(_offset.X / 2, _offset.Y / 2), CurrentFrame.rect, Color.White);
        }
    }
}
