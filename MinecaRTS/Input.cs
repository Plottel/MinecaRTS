using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace MinecaRTS
{
    // TODO: Make mouse position relative to top-left corner of screen.
    // TODO: Game crashes if i make a unit in same position twice without moving mouse.



    /// <summary>
    /// Defines the core Input handling methods for the program.
    /// 
    /// Key States are defined as:
    ///     Down        - the key is currently being pressed.
    ///     Typed       - the key was pressed this tick.
    ///     Released    - the key was released this tick.
    ///     
    /// Mouse States are defined as:
    ///     Down        - the button is currently being pressed.
    ///     Pressed     - the button was pressed this tick.
    ///     Clicked     - the button was released this tick.
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// The state of the keyboard last tick. Used for evaluating KeyTyped and KeyReleased.
        /// </summary>
        private static KeyboardState oldKeyStates = Keyboard.GetState();

        /// <summary>
        /// The current state of the keyboard.
        /// </summary>
        private static KeyboardState currentKeyStates = Keyboard.GetState();

        /// <summary>
        /// The state of the mouse last tick. Used for evaluating MousePressed and MouseClicked.
        /// </summary>
        private static MouseState oldMouseState = Mouse.GetState();

        /// <summary>
        /// The current state of the mouse.
        /// </summary>
        private static MouseState currentMouseState = Mouse.GetState();

        /// <summary>
        /// The maximum position of the mouse. Prevents returning mouse position outside the window.
        /// </summary>
        private static int _maxMouseX, _maxMouseY;

        /// <summary>
        /// Returns the current x position of the mouse.
        /// </summary>
        public static int MouseX
        {
            get
            {
                return MathHelper.Clamp(currentMouseState.X, 0, _maxMouseX);
            }
        }

        /// <summary>
        /// Returns the current y position of the mouse.
        /// </summary>
        public static int MouseY
        {
            get
            {
                return MathHelper.Clamp(currentMouseState.Y, 0, _maxMouseY);
            }
        }

        /// <summary>
        /// Returns the mouse x / y positions as a Vector2.
        /// </summary>
        public static Vector2 MousePos
        {
            get
            {
                // New Vector2 created to accommodate clamping.
                return new Vector2(MouseX, MouseY);
            }
        }

        /// <summary>
        /// The Input process method to be called once per tick. Updates the current and previous keyboard and mouse states.
        /// </summary>
        public static void UpdateStates()
        {
            // Update keyboard states.
            oldKeyStates = currentKeyStates;
            currentKeyStates = Keyboard.GetState();

            // Update mouse states.
            oldMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
        }

        /// <summary>
        /// Specifies if the passed in key is currently being pressed.
        /// </summary>
        /// <param name="key">The key to check.</param>
        public static bool KeyDown(Keys key)
        {
            return currentKeyStates.IsKeyDown(key);
        }

        /// <summary>
        /// Specifies if the passed in key was pressed this tick.
        /// </summary>
        /// <param name="key">The key to check.</param>
        public static bool KeyTyped(Keys key)
        {
            return oldKeyStates.IsKeyUp(key) && currentKeyStates.IsKeyDown(key);
        }

        /// <summary>
        /// Specifies if the passed in key was released this tick.
        /// </summary>
        /// <param name="key">The key to check.</param>
        public static bool KeyReleased(Keys key)
        {
            return oldKeyStates.IsKeyDown(key) && currentKeyStates.IsKeyUp(key);
        }

        /// <summary>
        /// Specifies if the left mouse button is currently held down.
        /// </summary>
        public static bool LeftMouseDown()
        {
            return currentMouseState.LeftButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Specifies if the left mouse button was pushed down this tick.
        /// </summary>
        public static bool LeftMousePressed()
        {
            return oldMouseState.LeftButton == ButtonState.Released && currentMouseState.LeftButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Specifies if the left mouse button was released this tick.
        /// </summary>
        public static bool LeftMouseClicked()
        {
            return oldMouseState.LeftButton == ButtonState.Pressed && currentMouseState.LeftButton == ButtonState.Released;
        }

        /// <summary>
        /// Specifies if the right mouse button is currently held down.
        /// </summary>
        /// <returns></returns>
        public static bool RightMouseDown()
        {
            return currentMouseState.RightButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Specifies if the right mouse button was pushed down this tick.
        /// </summary>
        public static bool RightMousePressed()
        {
            return oldMouseState.RightButton == ButtonState.Released && currentMouseState.RightButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Specifies if the right mouse button was released this tick.
        /// </summary>
        public static bool RightMouseClicked()
        {
            return oldMouseState.RightButton == ButtonState.Pressed && currentMouseState.RightButton == ButtonState.Released;
        }

        /// <summary>
        /// Sets the maximum returnable X value of the mouse position.
        /// </summary>
        public static void SetMaxMouseX(int max)
        {
            _maxMouseX = max;
        }

        /// <summary>
        /// Sets the maximum returnable Y value of the mouse position.
        /// </summary>
        public static void SetMaxMouseY(int max)
        {
            _maxMouseY = max;
        }
    }
}