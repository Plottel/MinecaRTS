using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MinecaRTS
{
    public interface IRenderable
    { 
        Rectangle RenderRect
        {
            get;
        }

        Vector2 Mid
        {
            get;
        }

        void Render(SpriteBatch spriteBatch);
    }
}
