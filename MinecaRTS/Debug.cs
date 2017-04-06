using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MinecaRTS
{
    public enum DebugOption
    {
        ShowPaths = 0,
        ShowGrid = 1,
        CalcPath = 2,
        CalcPathSmoothing = 3,
        ShowUnitVisionRect = 4,
        ShowWallPushForce = 5
    }

    public static class Debug
    {
        public static SpriteFont debugFont;
        private static Color debugColor = Color.White;
        private static Dictionary<DebugOption, bool> _settings = new Dictionary<DebugOption, bool>();

        public static void Init()
        {
            _settings.Add(DebugOption.ShowPaths, false);
            _settings.Add(DebugOption.ShowGrid, false);
            _settings.Add(DebugOption.CalcPath, false);
            _settings.Add(DebugOption.CalcPathSmoothing, false);
            _settings.Add(DebugOption.ShowUnitVisionRect, false);
            _settings.Add(DebugOption.ShowWallPushForce, false);
        }

        public static void HandleInput()
        {
            if (Input.KeyTyped(Keys.D1))
                _settings[DebugOption.ShowPaths] = !_settings[DebugOption.ShowPaths];

            if (Input.KeyTyped(Keys.D2))
                _settings[DebugOption.ShowGrid] = !_settings[DebugOption.ShowGrid];

            if (Input.KeyTyped(Keys.D3))
                _settings[DebugOption.CalcPathSmoothing] = !_settings[DebugOption.CalcPathSmoothing];

            if (Input.KeyTyped(Keys.D4))
                _settings[DebugOption.CalcPath] = !_settings[DebugOption.CalcPath];

            if (Input.KeyTyped(Keys.D5))
                _settings[DebugOption.ShowUnitVisionRect] = !_settings[DebugOption.ShowUnitVisionRect];

            if (Input.KeyTyped(Keys.D6))
                _settings[DebugOption.ShowWallPushForce] = !_settings[DebugOption.ShowWallPushForce];
        }

        public static bool OptionActive(DebugOption setting)
        {
            return _settings[setting];
        }

        public static void RenderDebugOptionStates(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(debugFont, "Show Paths (1) : " + _settings[DebugOption.ShowPaths], new Vector2(5, 5), debugColor);
            spriteBatch.DrawString(debugFont, "Show Grid (2) : " + _settings[DebugOption.ShowGrid], new Vector2(5, 20), debugColor);
            spriteBatch.DrawString(debugFont, "Calc Path Smoothing (3) : " + _settings[DebugOption.CalcPathSmoothing], new Vector2(5, 35), debugColor);
            spriteBatch.DrawString(debugFont, "Calc Path (4) : " + _settings[DebugOption.CalcPath], new Vector2(5, 50), debugColor);
            spriteBatch.DrawString(debugFont, "Show Unit Vision Rectangle (5) : " + _settings[DebugOption.ShowUnitVisionRect], new Vector2(5, 65), debugColor);
            spriteBatch.DrawString(debugFont, "Show Wall Push Force (6) : " + _settings[DebugOption.ShowWallPushForce], new Vector2(5, 80), debugColor);
        }
    }
}
