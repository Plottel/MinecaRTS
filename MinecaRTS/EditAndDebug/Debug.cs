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
        ShowPaths = 1,
        ShowGrid = 2,
        CalcPath = 3,
        CalcPathSmoothing = 4,
        ShowUnitFeelers = 5,
        ShowWallPushForce = 6,
        ShowStates = 7,
        ShowCoarseGrid = 8,
        Count = 9 // For easy iteration
    }

    public static class Debug
    {
        public static SpriteFont debugFont;
        private static Color _debugColor = Color.White;
        private static Dictionary<DebugOption, bool> _settings = new Dictionary<DebugOption, bool>();
        private static List<string> _hookedText = new List<string>();
        private static Point _hookedTextStart = new Point(1000, 5);

        public static void Init()
        {
            _settings.Add(DebugOption.ShowPaths, false);
            _settings.Add(DebugOption.ShowGrid, false);
            _settings.Add(DebugOption.CalcPath, false);
            _settings.Add(DebugOption.CalcPathSmoothing, false);
            _settings.Add(DebugOption.ShowUnitFeelers, false);
            _settings.Add(DebugOption.ShowWallPushForce, false);
            _settings.Add(DebugOption.ShowStates, false);
            _settings.Add(DebugOption.ShowCoarseGrid, false);
        }

        public static void HandleInput()
        {
            if (Input.KeyTyped(Keys.D1))
                _settings[DebugOption.ShowPaths] = !_settings[DebugOption.ShowPaths];

            if (Input.KeyTyped(Keys.D2))
                _settings[DebugOption.ShowGrid] = !_settings[DebugOption.ShowGrid];

            if (Input.KeyTyped(Keys.D3))
                _settings[DebugOption.CalcPath] = !_settings[DebugOption.CalcPath];

            if (Input.KeyTyped(Keys.D4))
                _settings[DebugOption.CalcPathSmoothing] = !_settings[DebugOption.CalcPathSmoothing];

            if (Input.KeyTyped(Keys.D5))
                _settings[DebugOption.ShowUnitFeelers] = !_settings[DebugOption.ShowUnitFeelers];

            if (Input.KeyTyped(Keys.D6))
                _settings[DebugOption.ShowWallPushForce] = !_settings[DebugOption.ShowWallPushForce];

            if (Input.KeyTyped(Keys.D7))
                _settings[DebugOption.ShowStates] = !_settings[DebugOption.ShowStates];

            if (Input.KeyTyped(Keys.D8))
                _settings[DebugOption.ShowCoarseGrid] = !_settings[DebugOption.ShowCoarseGrid];
        }

        public static bool OptionActive(DebugOption setting)
        {
            return _settings[setting];
        }

        public static void RenderDebugOptionStates(SpriteBatch spriteBatch)
        {
            for (int i = 1; i < (int)DebugOption.Count; i++)
            {
                DebugOption opt = (DebugOption)i;
                spriteBatch.DrawString(debugFont, "(" + i + ") " + opt.ToString() + " --- " + _settings[opt], new Vector2(5, 5 + (i * 15)), _debugColor);
            }
            //spriteBatch.DrawString(debugFont, "Show Paths (1) : " + _settings[DebugOption.ShowPaths], new Vector2(5, 5), _debugColor);
            //spriteBatch.DrawString(debugFont, "Show Grid (2) : " + _settings[DebugOption.ShowGrid], new Vector2(5, 20), _debugColor);
            //spriteBatch.DrawString(debugFont, "Calc Path Smoothing (3) : " + _settings[DebugOption.CalcPathSmoothing], new Vector2(5, 35), _debugColor);
            //spriteBatch.DrawString(debugFont, "Calc Path (4) : " + _settings[DebugOption.CalcPath], new Vector2(5, 50), _debugColor);
            //spriteBatch.DrawString(debugFont, "Show Unit Feelers (5) : " + _settings[DebugOption.ShowUnitFeelers], new Vector2(5, 65), _debugColor);
            //spriteBatch.DrawString(debugFont, "Show Wall Push Force (6) : " + _settings[DebugOption.ShowWallPushForce], new Vector2(5, 80), _debugColor);
            //spriteBatch.DrawString(debugFont, "Show States (7) : " + _settings[DebugOption.ShowStates], new Vector2(5, 95), _debugColor);
        }

        public static void ClearHookedText()
        {
            _hookedText = new List<string>();
        }

        public static void HookText(string text)
        {
            _hookedText.Add(text);
        }

        public static void RenderHookedText(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _hookedText.Count; i++)
            {
                spriteBatch.DrawString(debugFont, _hookedText[i], new Vector2(_hookedTextStart.X, _hookedTextStart.Y + (i * 15)), _debugColor);
            }                
        }
    }
}
