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
    /// <summary>
    /// The Minecart Unit in the program.
    /// Moves much faster on Minecart Tracks.
    /// Responsible for collecting resources from Deposit Boxes and bringing them back to the Town Hall.
    /// </summary>
    public class Minecart : Unit
    {
        public static SpriteSheet emptySS;
        public static Dictionary<Dir, List<Frame>> emptyAnimFrames;

        /// <summary>
        /// Movement speed on normal ground. 
        /// </summary>
        public const float BASE_SPEED = 0.8f;

        /// <summary>
        /// Movement speed on Minecart Tracks.
        /// </summary>
        public const float TRACK_SPEED = 7;

        /// <summary>
        /// Deposit Box in the Minecart travel route.
        /// </summary>
        public DepositBox TargetDepositBox;

        /// <summary>
        /// Town Hall in the Minecart travel route.
        /// </summary>
        public TownHall TargetTownHall;

        /// <summary>
        /// Amount of wood the Minecart is carrying.
        /// </summary>
        private uint _wood;

        /// <summary>
        /// Amount of stone the Minecart is carrying.
        /// </summary>
        private uint _stone;

        /// <summary>
        /// Gets the amount of wood the Minecart is carrying.
        /// </summary>
        public uint Wood
        {
            get { return _wood; }
        }

        /// <summary>
        /// Gets the amount of stone the Minecart is carrying.
        /// </summary>
        public uint Stone
        {
            get { return _stone; }
        }

        /// <summary>
        /// The State Machine for the Minecart.
        /// </summary>
        private StateMachine<Minecart> _fsm;
       
        /// <summary>
        /// Gets the State Machine for the Minecart.
        /// </summary>
        public StateMachine<Minecart> FSM
        {
            get { return _fsm; }
        }

        /// <summary>
        /// Initializes a new Minecart.
        /// </summary>
        /// <param name="data">The PlayerData for querying.</param>
        /// <param name="team">The Team the Minecart belongs to</param>
        /// <param name="pos">The position</param>
        /// <param name="scale">The size</param>
        public Minecart(PlayerData data, Team team, Vector2 pos, Vector2 scale) : base(data, team, pos, scale)
        {
            _fsm = new StateMachine<Minecart>(this);
            Speed = BASE_SPEED;

            animation = new Animation(emptySS.texture, emptyAnimFrames[Dir.S], Vector2.Zero, true);
            pathHandler = new MinecartPathHandler(this, data.Grid);
        }

        /// <summary>
        /// Main update method for the Minecart.
        /// </summary>
        public override void Update()
        {
            Debug.HookText("Nodes in Path: " + pathHandler.path.Count.ToString());
            if (Data.CellHasTrack(Data.Grid.CellAt(Mid)))
            {
                Speed = TRACK_SPEED;
                Steering.separationIsOn = false;
            }
            else
            {
                Speed = BASE_SPEED;
                Steering.separationIsOn = true;
            }

            _fsm.Execute();

            if (lastHeading != heading)
                animation.ChangeScript(emptyAnimFrames[heading], Vector2.Zero, true, false);

            animation.Update();

            base.Update();
        }

        /// <summary>
        /// Dumps all resources currently held in the Minecart.
        /// </summary>
        public void EmptySelf()
        {
            _wood = 0;
            _stone = 0;
        }

        /// <summary>
        /// Checks what is at the passed in position and fetches a path accordingly.
        /// </summary>
        /// <param name="pos">The position to path towards</param>
        /// <param name="smoothed">Whether or not the path should be smoothed.</param>
        public override void MoveTowards(Vector2 pos, bool smoothed = false)
        {
            Building buildingAtPos = Data.BuildingAtPos(pos);

            if (buildingAtPos is DepositBox)
            {
                TargetDepositBox = buildingAtPos as DepositBox;
                _fsm.ChangeState(MoveToDepositBox.Instance);
            }                
            else if (buildingAtPos is TownHall)
            {
                TargetTownHall = buildingAtPos as TownHall;
                _fsm.ChangeState(ReturnToTownHall.Instance);
            }
            else
            {
                Cell cellAtPos = Data.Grid.CellAt(pos);

                if (cellAtPos.Passable)
                {
                    ExitState();
                    pathHandler.GetPathTo(pos, smoothed);
                }
            }
        }

        /// <summary>
        /// Adds the passed in resource amounts to the Minecart's storage.
        /// </summary>
        /// <param name="woodAmount">The amount of wood to add</param>
        /// <param name="stoneAmount">The amount of stone to add</param>
        public void AcceptResources(uint woodAmount, uint stoneAmount)
        {
            _wood += woodAmount;
            _stone += stoneAmount;
        }

        /// <summary>
        /// Leaves current state and sets current state to null.
        /// </summary>
        public override void ExitState()
        {
            FSM.ChangeState(null);
        }

        /// <summary>
        /// Renders current state above the Minecart.
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
