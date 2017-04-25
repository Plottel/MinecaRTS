﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MinecaRTS
{
    /// <summary>
    /// The interface through which the player interacts with the world.
    /// Bots can be plugged in to a PlayerData and can access it to issue commands.
    /// </summary>
    public class PlayerData
    {
        // This represents the facade. All queries are done through this and the real world figures it out.
        public World world;

        private uint _wood = 0;
        private uint _stone = 0;

        public uint Wood
        {
            get { return _wood; }
        }

        public uint Stone
        {
            get { return _stone; }
        }

        public PlayerData(World w)
        {
            world = w;
        }

        public void GiveResources(uint amount, ResourceType type)
        {
            if (type == ResourceType.Wood)
                _wood += amount;
            else if (type == ResourceType.Stone)
                _stone += amount;
            else
                throw new Exception("Invalid resource type in PlayerData.GiveResources");
        }

        public void SelectUnitsInRect(Rectangle selectAt)
        {
            // TODO: Figure out how to do this with LINQ.

            world.SelectedUnits = new List<Unit>();

            foreach (Unit u in world.Units)
            {
                if (selectAt.Intersects(u.CollisionRect))
                    world.SelectedUnits.Add(u);
            }
        }

        public void MoveSelectedUnitsTo(Vector2 pos)
        {
            foreach (Unit u in world.SelectedUnits)
            {
                u.pathHandler.GetPathTo(pos);
                u.ExitState(); // Change to "Neutral state"
            }
        }

        public void OrderWorkersToGatherClosestResource(ResourceType desiredResource)
        {
            foreach (Worker w in world.Units)
            {
                w.resrcLookingFor = desiredResource;
                w.resourceReturnBuilding = null;
                w.targetResourceCell = null;
                w.FSM.ChangeState(MoveToResource.Instance);
            }
        }

        public List<Unit> GetCollidingUnits(Unit unit)
        {
            var result = new List<Unit>();

            foreach (Unit u in world.Units)
            {
                if (unit.CollisionRect.Intersects(u.CollisionRect))
                    result.Add(u);
            }

            // Remove the unit being checked from list to prevent checks when this list is used.
            result.Remove(unit);

            return result;
        }

        public List<Unit> GetUnitsInRadius(Unit unit, float radius)
        {
            var result = new List<Unit>();

            float taggableDistance = (float)Math.Pow(radius + unit.Scale.X / 2, 2);

            foreach (Unit u in world.Units)
            {
                if (Vector2.DistanceSquared(unit.Mid, u.Mid) < taggableDistance)
                    result.Add(u);
            }

            // Remove the unit being checked from list to prevent checks when this list is used.
            result.Remove(unit);

            return result;
        }

        // TODO: As more building types exist, this will take some sort of parameter
        // indicating what kind of building it is (enum or actual object).
        // Returns if the placement was successful so the bot knows whether or not to exit "placing building mode".
        public bool PlaceBuilding(Building building)
        {
            if (world.Grid.RectIsClear(building.CollisionRect))
            {
                world.AddBuilding(building);
                return true;
            }

            return false;
        }

        public Building GetClosestResourceReturnBuilding(Unit u)
        {
            float closestDistance = float.MaxValue;
            Building closestBuilding = null;

            foreach (Building b in world.Buildings)
            {
                float distance = Vector2.Distance(u.Mid, b.Mid);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBuilding = b;
                }
            }

            return closestBuilding;

        }
    }
}
