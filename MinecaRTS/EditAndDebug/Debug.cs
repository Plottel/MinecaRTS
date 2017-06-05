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
    public enum DebugOp
    {
        ShowPaths = 1,
        ShowGrid = 2,
        CalcPath = 3,
        CalcPathSmoothing = 4,
        ShowUnitFeelers = 5,
        ShowWallPushForce = 6,
        ShowStates = 7,
        ShowCoarseGrid = 8,
        ShowFogOfWar = 9,
        EnableTimeSlicedPathing = 10,
        ShowInfluence = 11,
        Count = 12 // For easy iteration
    }

    public static class Debug
    {
        public static SpriteFont debugFont;
        private static Color _debugColor = Color.White;
        private static Dictionary<DebugOp, bool> _settings = new Dictionary<DebugOp, bool>();
        private static List<string> _hookedText = new List<string>();
        private static Point _hookedTextStart = new Point(1000, 5);

        public static void Init()
        {
            _settings.Add(DebugOp.ShowPaths, false);
            _settings.Add(DebugOp.ShowGrid, false);
            _settings.Add(DebugOp.CalcPath, false);
            _settings.Add(DebugOp.CalcPathSmoothing, false);
            _settings.Add(DebugOp.ShowUnitFeelers, false);
            _settings.Add(DebugOp.ShowWallPushForce, false);
            _settings.Add(DebugOp.ShowStates, false);
            _settings.Add(DebugOp.ShowCoarseGrid, false);
            _settings.Add(DebugOp.ShowFogOfWar, false);
            _settings.Add(DebugOp.EnableTimeSlicedPathing, false);
            _settings.Add(DebugOp.ShowInfluence, false);
        }

        public static void HandleInput()
        {
            if (Input.KeyTyped(Keys.D1))
                _settings[DebugOp.ShowPaths] = !_settings[DebugOp.ShowPaths];

            if (Input.KeyTyped(Keys.D2))
                _settings[DebugOp.ShowGrid] = !_settings[DebugOp.ShowGrid];

            if (Input.KeyTyped(Keys.D3))
                _settings[DebugOp.CalcPath] = !_settings[DebugOp.CalcPath];

            if (Input.KeyTyped(Keys.D4))
                _settings[DebugOp.CalcPathSmoothing] = !_settings[DebugOp.CalcPathSmoothing];

            if (Input.KeyTyped(Keys.D5))
                _settings[DebugOp.ShowUnitFeelers] = !_settings[DebugOp.ShowUnitFeelers];

            if (Input.KeyTyped(Keys.D6))
                _settings[DebugOp.ShowWallPushForce] = !_settings[DebugOp.ShowWallPushForce];

            if (Input.KeyTyped(Keys.D7))
                _settings[DebugOp.ShowStates] = !_settings[DebugOp.ShowStates];

            if (Input.KeyTyped(Keys.D8))
                _settings[DebugOp.ShowCoarseGrid] = !_settings[DebugOp.ShowCoarseGrid];

            if (Input.KeyTyped(Keys.D9))
                _settings[DebugOp.ShowFogOfWar] = !_settings[DebugOp.ShowFogOfWar];

            if (Input.KeyTyped(Keys.D0))
                _settings[DebugOp.EnableTimeSlicedPathing] = !_settings[DebugOp.EnableTimeSlicedPathing];

            if (Input.KeyTyped(Keys.I))
                _settings[DebugOp.ShowInfluence] = !_settings[DebugOp.ShowInfluence];
        }

        public static bool IsOn(DebugOp setting)
        {
            return _settings[setting];
        }

        public static void RenderDebugOptionStates(SpriteBatch spriteBatch)
        {
            for (int i = 1; i < (int)DebugOp.Count; i++)
            {
                DebugOp opt = (DebugOp)i;
                spriteBatch.DrawString(debugFont, "(" + i + ") " + opt.ToString() + " --- " + _settings[opt], new Vector2(5, 5 + (i * 15)), _debugColor);
            }            
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
