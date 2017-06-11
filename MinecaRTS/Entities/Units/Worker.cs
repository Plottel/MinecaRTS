using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MinecaRTS
{
    // To simplify reasoning and save the hands!
    using AnimationDictionary = Dictionary<WkrAnim, Dictionary<Dir, List<Frame>>>;

    /// <summary>
    /// All possible Worker Animation States.
    /// </summary>
    public enum WkrAnim
    {
        Walk,
        Chop,
        Logs,
        Mine,
        Bag,
        Build
    }

    /// <summary>
    /// A unit which can construct buildings and gather resources and return them to the Town Hall.
    /// </summary>
    public class Worker : Unit
    {
        /// <summary>
        /// All sprite sheets for all possible animations.
        /// </summary>
        public static Dictionary<WkrAnim, SpriteSheet> spriteSheets = new Dictionary<WkrAnim, SpriteSheet>();

        /// <summary>
        /// Individual frames for each animation.
        /// </summary>
        public static AnimationDictionary animFrames = new AnimationDictionary();

        /// <summary>
        /// Offset vector to keep animations rendered at same position since some of them are different sizes.
        /// </summary>
        public static Dictionary<WkrAnim, Vector2> animOffsets = new Dictionary<WkrAnim, Vector2>();

        /// <summary>
        /// Base worker speed.
        /// </summary>
        public const float BASE_SPEED = 2;
        
        /// <summary>
        /// Point where the worker should return resources to. This could be a Deposit Box or a Town Hall.
        /// </summary>
        public ICanAcceptResources returningResourcesTo;

        /// <summary>
        /// The cell where the resource the Worker is harvesting resides.
        /// </summary>
        public Cell targetResourceCell;

        /// <summary>
        /// The building the Worker is constructing.
        /// </summary>
        public Building constructing;

        /// <summary>
        /// The type of resource the worker is searching to harvest.
        /// </summary>
        public ResourceType resrcLookingFor = ResourceType.None;

        /// <summary>
        /// The type of resource the worker is currently holding.
        /// </summary>
        public ResourceType resrcHolding = ResourceType.None;

        /// <summary>
        /// How long the worker has been harvesting the current resource.
        /// </summary>
        public uint timeSpentHarvesting = 0;

        /// <summary>
        /// The current animation of the worker.
        /// </summary>
        public WkrAnim currentAnim = WkrAnim.Walk;

        /// <summary>
        /// The Worker's state machine.
        /// </summary>
        private StateMachine<Worker> _fsm;

        /// <summary>
        /// Gets the Worker's state machine.
        /// </summary>
        public StateMachine<Worker> FSM
        {
            get { return _fsm; }
        }
        
        /// <summary>
        /// Gets the resource residing at the Worker's targetResourceCell.
        /// </summary>
        public Resource TargetResource
        {
            get { return Data.GetResourceFromCell(targetResourceCell); }
        }

        /// <summary>
        /// Initializes a new Worker
        /// </summary>
        /// <param name="data">The PlayerData for querying</param>
        /// <param name="team">The team the worker belongs to</param>
        /// <param name="pos">The position the worker is created at</param>
        /// <param name="scale">The size</param>
        public Worker(PlayerData data, Team team, Vector2 pos, Vector2 scale) : base(data, team, pos, scale)
        {
            _fsm = new StateMachine<Worker>(this);
            Speed = BASE_SPEED;
            animation = new Animation(spriteSheets[WkrAnim.Walk].texture, animFrames[WkrAnim.Walk][Dir.S], animOffsets[WkrAnim.Walk], true);
            pathHandler = new WorkerPathHandler(this, data.Grid);
        }

        /// <summary>
        /// Dumps whatever resources the worker is holding into its resource return point.
        /// </summary>
        public void DepositResources()
        {
            if (resrcHolding == ResourceType.Wood)
                returningResourcesTo.AcceptResources(Resource.HARVEST_AMOUNT, 0);
            else if (resrcHolding == ResourceType.Stone)
                returningResourcesTo.AcceptResources(0, Resource.HARVEST_AMOUNT);

            resrcHolding = ResourceType.None;            
        }

        /// <summary>
        /// Updates the animation and state macine of the worker.
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (lastHeading != heading)
                animation.ChangeScript(animFrames[currentAnim][heading], animOffsets[currentAnim], true, false);

            // Don't update animation if not moving
            // TODO: Add checks - some animations (chopping) will still update if not moving
            if (Vel != Vector2.Zero || currentAnim == WkrAnim.Chop || currentAnim == WkrAnim.Mine || currentAnim == WkrAnim.Build)
                animation.Update();

            _fsm.Execute();
        }

        /// <summary>
        /// Orders the worker to start moving towards the passed in position.
        /// Will look at what is at the destination and act accordingly.
        /// </summary>
        /// <param name="pos">The position to path towards.</param>
        /// <param name="smoothed">Whether or not the path should be smoothed.</param>
        public override void MoveTowards(Vector2 pos, bool smoothed = true)
        {
            Building buildingAtPos = Data.BuildingAtPos(pos);

            if (buildingAtPos != null)
                GoConstructBuilding(buildingAtPos);
            else             
            {
                Cell cell = Data.Grid.CellAt(pos);
                Resource resource = Data.GetResourceFromCell(cell);
                
                if (resource != null)
                {
                    targetResourceCell = cell;
                    resrcLookingFor = resource.Type;

                    FSM.ChangeState(MoveToResource.Instance);
                }
                else
                {
                    base.MoveTowards(pos, smoothed);
                }                    
            }
                
        }

        /// <summary>
        /// Orders the worker to go and construct the passed in building.
        /// Will fetch a path and change worker state.
        /// </summary>
        /// <param name="building"></param>
        public void GoConstructBuilding(Building building)
        {
            constructing = building;
            FSM.ChangeState(MoveToConstructBuilding.Instance);
        }

        /// <summary>
        /// Changes the worker's current animation to the passed in type.
        /// </summary>
        /// <param name="newAnim">The new animation.</param>
        public void ChangeAnimation(WkrAnim newAnim)
        {
            currentAnim = newAnim;
            animation.ChangeScript(animFrames[currentAnim][heading], animOffsets[currentAnim], true, true);

            animation.texture = spriteSheets[newAnim].texture;
        }

        /// <summary>
        /// Determines what happens when a worker is sent a message.
        /// The state machine will handle it.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void HandleMessage(Message message)
        {
            FSM.HandleMessage(message);
        }

        /// <summary>
        /// Exits current state and sets current state to null.
        /// </summary>
        public override void ExitState()
        {
            FSM.ChangeState(null);
        }

        /// <summary>
        /// Renders the current state above the Worker.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to render to.</param>
        public override void RenderDebug(SpriteBatch spriteBatch)   
        {
            base.RenderDebug(spriteBatch);

            if (FSM.CurrentState != null)
                spriteBatch.DrawString(Debug.debugFont, FSM.CurrentState.GetType().Name, RenderMid - Scale / 2, Color.White);
        }       
    }
}
