using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace MinecaRTS
{
    public class MinecaRTS : Game
    {
        /// <summary>
        /// Singleton instance for global debug purposes.
        /// Sometimes for debug it's just easier to have everything wherever we need it.
        /// </summary>
        public static MinecaRTS Instance;

        public static SpriteFont smallFont;
        public static SpriteFont largeFont;

        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public World world;
        bool editMode;
        bool debugMode;

        public MinecaRTS()
        {
            Instance = this;            

            // Setup all the state singletons.
            new MoveToResource();
            new HarvestResource();
            new ReturnResource();
            new MoveToConstructBuilding();
            new ConstructBuilding();

            ProductionBuilding.productionTimes.Add(typeof(Worker), 120);
            ProductionBuilding.productionTimes.Add(typeof(Minecart), 120);

            // Setup Entity costs
            // WOOD, STONE, SUPPLY
            World.entityCosts.Add(typeof(Worker), new Cost(50, 0, 1));
            World.entityCosts.Add(typeof(House), new Cost(100, 0, 0));
            World.entityCosts.Add(typeof(TownHall), new Cost(0, 0, 0));
            World.entityCosts.Add(typeof(Track), new Cost(0, 0, 0));
            World.entityCosts.Add(typeof(Minecart), new Cost(0, 0, 0));
            World.entityCosts.Add(typeof(DepositBox), new Cost(0, 0, 0));

            // MUST BE CREATED FIRST TO HAVE ID OF 0.
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
            debugMode = false;
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
            smallFont = Content.Load<SpriteFont>("SmallFont");
            largeFont = Content.Load<SpriteFont>("largeFont");

            TownHall.ACTIVE_TEXTURE = Content.Load<Texture2D>("images/buildings/town_hall");
            TownHall.CONSTRUCTION_TEXTURE = Content.Load<Texture2D>("images/buildings/town_hall_construction");

            House.ACTIVE_TEXTURE = Content.Load<Texture2D>("images/buildings/house");
            House.CONSTRUCTION_TEXTURE = Content.Load<Texture2D>("images/buildings/house_construction");

            Track.ACTIVE_TEXTURE = Content.Load<Texture2D>("images/buildings/track");
            Track.CONSTRUCTION_TEXTURE = Content.Load<Texture2D>("images/buildings/track");

            DepositBox.ACTIVE_TEXTURE = Content.Load<Texture2D>("images/buildings/deposit_box");
            DepositBox.CONSTRUCTION_TEXTURE = Content.Load<Texture2D>("images/buildings/deposit_box");

            Resource.WOOD_TEXTURE = Content.Load<Texture2D>("images/resources/tree");
            Resource.WOOD_DEPLETED_TEXTURE = Content.Load<Texture2D>("images/resources/tree_stump");

            Worker.LoadSpriteSheet(this, "worker_walk", WorkerAnimation.Walk, 7, 8);
            Worker.LoadSpriteSheet(this, "worker_chop", WorkerAnimation.Chop, 5, 8);
            Worker.LoadSpriteSheet(this, "worker_logs", WorkerAnimation.Logs, 7, 8);
            Worker.LoadSpriteSheet(this, "worker_mine", WorkerAnimation.Mine, 5, 8);
            Worker.LoadSpriteSheet(this, "worker_bag", WorkerAnimation.Bag, 7, 8);

            GameResources.CreateWorkerWalkAnimation();
            GameResources.CreateWorkerChopAnimation();
            GameResources.CreateWorkerLogsAnimation();
            GameResources.CreateWorkerMineAnimation();
            GameResources.CreateWorkerBagAnimation();
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

            if (debugMode)
            {
                Debug.ClearHookedText();
                Debug.HandleInput();
            }           

            if (editMode)
                WorldEditor.HandleInput(world);
            else
                world.HandleInput();

            if (Input.KeyTyped(Keys.E))
            {
                editMode = !editMode;
                world.Grid.ShowGrid = !world.Grid.ShowGrid; 
            }

            if (Input.KeyTyped(Keys.OemTilde))
                debugMode = !debugMode;
            
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

            if (debugMode)
            {
                world.RenderDebug(spriteBatch);

                Debug.RenderDebugOptionStates(spriteBatch);
                Debug.RenderHookedText(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
