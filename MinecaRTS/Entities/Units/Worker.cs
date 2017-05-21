﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MinecaRTS
{
    using AnimationDictionary = Dictionary<WkrAnim, Dictionary<Dir, List<Frame>>>;

    public enum WkrAnim
    {
        Walk,
        Chop,
        Logs,
        Mine,
        Bag,
        Build
    }

    public class Worker : Unit
    {
        public static Dictionary<WkrAnim, SpriteSheet> spriteSheets = new Dictionary<WkrAnim, SpriteSheet>();
        public static AnimationDictionary animFrames = new AnimationDictionary();
        public static Dictionary<WkrAnim, Vector2> animOffsets = new Dictionary<WkrAnim, Vector2>();

        public const float BASE_SPEED = 2;

        public ICanAcceptResources returningResourcesTo;
        public Cell targetResourceCell;
        public Building constructing;

        public ResourceType resrcLookingFor = ResourceType.None;

        public WkrAnim currentAnim = WkrAnim.Walk;

        public ResourceType resrcHolding = ResourceType.None;

        public uint timeSpentHarvesting = 0;

        private StateMachine<Worker> _fsm;

        public StateMachine<Worker> FSM
        {
            get { return _fsm; }
        }

        public Resource TargetResource
        {
            get { return Data.GetResourceFromCell(targetResourceCell); }
        }

        public Worker(PlayerData data, Team team, Vector2 pos, Vector2 scale) : base(data, team, pos, scale)
        {
            _fsm = new StateMachine<Worker>(this);
            Speed = BASE_SPEED;
            animation = new Animation(spriteSheets[WkrAnim.Walk].texture, animFrames[WkrAnim.Walk][Dir.S], animOffsets[WkrAnim.Walk], true);
            pathHandler = new WorkerPathHandler(this, data.Grid);
        }

        public void DepositResources()
        {
            if (resrcHolding == ResourceType.Wood)
                returningResourcesTo.AcceptResources(Resource.HARVEST_AMOUNT, 0);
            else if (resrcHolding == ResourceType.Stone)
                returningResourcesTo.AcceptResources(0, Resource.HARVEST_AMOUNT);

            resrcHolding = ResourceType.None;            
        }

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

        public override void MoveTowards(Vector2 pos)
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
                    base.MoveTowards(pos);
                }                    
            }
                
        }

        public void GoConstructBuilding(Building building)
        {
            constructing = building;
            FSM.ChangeState(MoveToConstructBuilding.Instance);
        }

        public void ChangeAnimation(WkrAnim newAnim)
        {
            currentAnim = newAnim;
            animation.ChangeScript(animFrames[currentAnim][heading], animOffsets[currentAnim], true, true);

            animation.texture = spriteSheets[newAnim].texture;
        }

        public override void HandleMessage(Message message)
        {
            FSM.HandleMessage(message);
        }

        public override void ExitState()
        {
            FSM.ChangeState(null);
        }

        public override void RenderDebug(SpriteBatch spriteBatch)   
        {
            base.RenderDebug(spriteBatch);

            if (FSM.CurrentState != null)
                spriteBatch.DrawString(Debug.debugFont, FSM.CurrentState.GetType().Name, RenderMid - Scale / 2, Color.Black);
        }       
    }
}
