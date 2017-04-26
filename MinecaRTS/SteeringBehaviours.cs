using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Microsoft.Xna.Framework.Graphics;

namespace MinecaRTS
{
    public class SteeringBehaviours
    {
        // TODO: Maybe this should belong to Unit? If it's used for other calculations apart from obstacle avoidance?
        public static int LOOK_AHEAD_DISTANCE = 50;

        private Unit _owner;
        private PlayerData _data;
        private List<Unit> _neighbours;

        public bool separationOn = true;

        // Vectors for Obstacle Avoidance calculation - primarly debug.
        // TODO: Clean this up!!!!
        public Vector2 from1, to1, from2, to2;
        public Vector2 wallPushForce;
        public Vector2 closestUnpassableCellMid;

        public SteeringBehaviours(Unit owner, PlayerData data)
        {
            _owner = owner;
            _data = data;
            _neighbours = new List<Unit>();
            from1 = Vector2.Zero;
            from2 = Vector2.Zero;
            to1 = Vector2.Zero;
            to2 = Vector2.Zero;
        }

        public Vector2 Calculate()
        {
            var force = Vector2.Zero;

            _neighbours = _data.GetUnitsInRadius(_owner, Unit.NEIGHBOUR_RADIUS);

            if (separationOn)
                force += Separation();

            force += UnpassableCellAvoidance();

            return force;
        }

        public Vector2 Separation()
        {
            var force = Vector2.Zero;

            foreach (Unit u in _neighbours)
            {
                Vector2 toNeighbour = _owner.Mid - u.Mid;

                // Scale based on inverse distance to neighbour.
                if (toNeighbour.Length() > 0)
                    force += Vector2.Normalize(toNeighbour) / toNeighbour.Length();
            }

            return force * 20;
        }

        // TODO: Projection Rect extends beyond boundaries of game.. CRASH because null cell reference
        public Vector2 UnpassableCellAvoidance()
        {
            closestUnpassableCellMid = Vector2.Zero;
            var force = Vector2.Zero;

            // Get the normalised perp to my heading
            Vector2 normPerp = Vector2.Normalize(_owner.Vel.PerpendicularClockwise());
            // Mid + (normPerp * scale)
            from1 = _owner.Mid + (normPerp * (_owner.Scale / 2));
            // Mid - (normPerp * scale)
            from2 = _owner.Mid - (normPerp * (_owner.Scale / 2));
            //      These are the two projection points

            // To1, 2 = Projection + (velocity * someLookAheadDistanceValue);
            to1 = from1 + (_owner.Vel * LOOK_AHEAD_DISTANCE);
            to2 = from2 + (_owner.Vel * LOOK_AHEAD_DISTANCE);

            // Get cells in rect created from projected lines.
            HashSet<Cell> cellsInVision = _data.world.Grid.CellsInRectFromLines(from1.ToPoint(), to1.ToPoint(), from2.ToPoint(), to2.ToPoint());

            // Find closest unpassable cell
            // TODO: Use distSq - faster.
            float closestDistance = float.MaxValue;
            Cell closestUnpassableCell = null;

            // Find closest unpassable cell.
            foreach (Cell cell in cellsInVision)
            {
                if (!cell.Passable)
                {
                    float distance = Vector2.Distance(_owner.Mid, cell.Mid);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestUnpassableCell = cell;
                    }
                }
            }

            // Steer away from closest wall if we found one -> Exert force in 1 of 4 cardinal directions.
            if (closestUnpassableCell != null)
            {
                // Debug cell mid
                closestUnpassableCellMid = closestUnpassableCell.Mid;

                // Calculate ticks to collide on each axis, whichever axis is less
                // is the axis we collide on.
                Vector2 toWall = closestUnpassableCell.Mid - _owner.Mid;

                float ticksOnX = Math.Abs(toWall.X / _owner.Vel.X);
                float ticksOnY = Math.Abs(toWall.Y / _owner.Vel.Y);

                if (ticksOnX < ticksOnY) // Collide on X axis -> Vertical wall.
                {
                    if (_owner.Mid.X < closestUnpassableCell.Mid.X) // Approaching from left, push left.
                        force = new Vector2(-1, 0);
                    else //Approaching from right, push right.
                        force = new Vector2(1, 0);
                }
                else // Collide on Y axis -> Horizontal wall.
                {
                    if (_owner.Mid.Y < closestUnpassableCell.Mid.Y) // Approaching from top, push up.
                        force = new Vector2(0, -1);
                    else // Approaching from below, push down.
                        force = new Vector2(0, 1);
                }
            }

            // If really close to a wall, apply a stronger force.
            if (closestDistance < 30)
                force *= 20;

            // Return resultant force.
            wallPushForce = force;
            return force;
        }

        public void ZeroOverlapCells()
        {
            foreach (Cell cell in _data.world.Grid.CellsInRect(_owner.CollisionRect))
            {
                if (!cell.Passable)
                {
                    Vector2 toOwner = _owner.Mid - cell.Mid;
                    float distanceApart = toOwner.Length();
                    float amountOfOverlap = (_owner.Scale.Length() / 2) + (Cell.CELL_SIZE / 2) - distanceApart;

                    _owner.Pos += (toOwner / distanceApart) * amountOfOverlap;
                }
            }
        }

        public void ZeroOverlapUnits()
        {
            foreach (Unit collider in _data.GetCollidingUnits(_owner))
            {
                Vector2 toOwner = _owner.Mid - collider.Mid;
                float distanceApart = toOwner.Length();
                float amountOfOverlap = (_owner.Scale.Length() / 2) + (collider.Scale.Length() / 2) - distanceApart;

                _owner.Pos += (toOwner / distanceApart) * amountOfOverlap;
            }                       
        }
    }
}
