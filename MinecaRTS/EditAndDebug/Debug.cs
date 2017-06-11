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
    /// <summary>
    /// All possible debug options.
    /// </summary>
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
        GroupPathing = 12,
        Count = 13 // For easy iteration
    }

    /// <summary>
    /// Class responsible for keeping track of and updating status of debug options.
    /// Also keeps track of a list of strings to be rendered as debug text.
    /// </summary>
    public static class Debug
    {
        /// <summary>
        /// Font used for debug text.
        /// </summary>
        public static SpriteFont debugFont;

        /// <summary>
        /// Color used for debug text.
        /// </summary>
        private static Color _debugColor = Color.White;

        /// <summary>
        /// Maps debug options to a bool indicating if they are active or not.
        /// </summary>
        private static Dictionary<DebugOp, bool> _settings = new Dictionary<DebugOp, bool>();

        /// <summary>
        /// Strings to be rendered as debug text.
        /// </summary>
        private static List<string> _hookedText = new List<string>();

        /// <summary>
        /// Position for the first string in _hookedText to be rendered.
        /// </summary>
        private static Point _hookedTextStart = new Point(1000, 30);

        /// <summary>
        /// Sets up debug options as false.
        /// </summary>
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
            _settings.Add(DebugOp.GroupPathing, false);
        }

        /// <summary>
        /// User input method for debug.
        /// Turns settings on or off when Numbers are typed corresponding to the DebugOp enum value.
        /// Special cases after 0 where 'G' for group pathfinding and 'I' for influence map.
        /// </summary>
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

            if (Input.KeyTyped(Keys.G))
                _settings[DebugOp.GroupPathing] = !_settings[DebugOp.GroupPathing];
        }

        /// <summary>
        /// Returns whether or not the passed in DebugOp is currently active.
        /// </summary>
        /// <param name="setting">The debugOp to check</param>
        public static bool IsOn(DebugOp setting)
        {
            return _settings[setting];
        }

        /// <summary>
        /// Renders the current status of each debug option.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to render to.</param>
        public static void RenderDebugOptionStates(SpriteBatch spriteBatch)
        {
            for (int i = 1; i < (int)DebugOp.Count; i++)
            {
                DebugOp opt = (DebugOp)i;
                spriteBatch.DrawString(debugFont, "(" + i + ") " + opt.ToString() + " --- " + _settings[opt], new Vector2(5, 5 + (i * 15)), _debugColor);
            }            
        }

        /// <summary>
        /// Clears the list of strings setup to be rendered as debug text.
        /// </summary>
        public static void ClearHookedText()
        {
            _hookedText = new List<string>();
        }

        /// <summary>
        /// Adds a string to be rendered as debug text.
        /// </summary>
        /// <param name="text">The string</param>
        public static void HookText(string text)
        {
            _hookedText.Add(text);
        }

        /// <summary>
        /// Renders the strings set up as debug text.
        /// Renders one per line and accounts for indentation by tracking number of "\t" in the string.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to render to.</param>
        public static void RenderHookedText(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _hookedText.Count; i++)
            {
                string oldString = _hookedText[i];
                string newString = _hookedText[i].Replace("\t", "");

                int numTabs = oldString.Length - newString.Length;

                spriteBatch.DrawString(debugFont, newString, new Vector2(_hookedTextStart.X + (numTabs * 30), _hookedTextStart.Y + (i * 15)), _debugColor);
            }                
        }
    }
}
