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
    public class Minecart : Unit
    {
        public static SpriteSheet emptySS;

        public static Dictionary<Dir, List<Frame>> emptyAnimFrames;


        public DepositBox TargetDepositBox;
        public TownHall TargetTownHall;

        private uint _wood;
        private uint _stone;

        public uint Wood
        {
            get { return _wood; }
        }

        public uint Stone
        {
            get { return _stone; }
        }

        private StateMachine<Minecart> _fsm;
       
        public StateMachine<Minecart> FSM
        {
            get { return _fsm; }
        }

        public Minecart(PlayerData data, Team team, Vector2 pos, Vector2 scale) : base(data, team, pos, scale)
        {
            _fsm = new StateMachine<Minecart>(this);
            Speed = 2;

            animation = new Animation(emptySS.texture, emptyAnimFrames[Dir.S], Vector2.Zero, true);
        }

        public override void Update()
        {
            if (Data.CellHasTrack(Data.Grid.CellAt(Mid)))
            {
                Speed = 10;
                Steering.separationOn = false;
            }
            else
            {
                Speed = 2;
                Steering.separationOn = true;
            }

            _fsm.Execute();

            if (lastHeading != heading)
                animation.ChangeScript(emptyAnimFrames[heading], Vector2.Zero, true, false);

            animation.Update();

            base.Update();
        }

        public void EmptySelf()
        {
            _wood = 0;
            _stone = 0;
        }

        public override void MoveTowards(Vector2 pos)
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
                    pathHandler.GetPathToPosFollowingTracks(pos);
                    ExitState();
                }
            }
        }

        public void AcceptResources(uint woodAmount, uint stoneAmount)
        {
            _wood += woodAmount;
            _stone += stoneAmount;
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
