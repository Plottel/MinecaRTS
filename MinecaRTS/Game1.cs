using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace MinecaRTS
{
    public class Game1 : Game
    {
        /// <summary>
        /// Singleton instance for global debug purposes.
        /// Sometimes for debug it's just easier to have everything wherever we need it.
        /// </summary>
        public static Game1 Instance;

        public static int num = 5;
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public World world;
        bool editMode;

        public Game1()
        {
            Instance = this;            

            // Setup all the state singletons.
            new MoveToResource();
            new HarvestResource();
            new ReturnResource();


            world = new World();
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1366;
            graphics.PreferredBackBufferHeight = 768;

            Camera.WIDTH = 1366;
            Camera.HEIGHT = 768;

            Input.SetMaxMouseX(graphics.PreferredBackBufferWidth);
            Input.SetMaxMouseY(graphics.PreferredBackBufferHeight);

            Debug.Init();

            editMode = false;
            world.Grid.ShowGrid = false;


            //graphics.ToggleFullScreen();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Debug.debugFont = Content.Load<SpriteFont>("DebugFont");

            Worker.walkSS.texture = Content.Load<Texture2D>("images/worker/worker_walk");
            Worker.walkSS.cols = 7;
            Worker.walkSS.rows = 8;
            Worker.walkSS.cellWidth = 26;
            Worker.walkSS.cellHeight = 35;

            GameResources.CreateWorkerWalkAnimation();
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Input.UpdateStates();

            Debug.ClearHookedText();
            Debug.HandleInput();

            if (editMode)
                WorldEditor.HandleInput(world);
            else
                world.HandleInput();

            if (Input.KeyTyped(Keys.E))
            {
                editMode = !editMode;
                world.Grid.ShowGrid = !world.Grid.ShowGrid; 
            }

            
            world.Update();

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            spriteBatch.Begin();

            world.Render(spriteBatch);
            world.RenderDebug(spriteBatch);

            Debug.RenderDebugOptionStates(spriteBatch);
            Debug.RenderHookedText(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
