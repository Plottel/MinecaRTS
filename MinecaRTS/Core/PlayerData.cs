using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MinecaRTS
{
    /// <summary>
    /// The interface through which the player interacts with the world.
    /// Bots can be plugged in to a PlayerData and can access it to issue commands.
    /// </summary>
    public class PlayerData
    {
        /// <summary>
        /// Bots can't access the world directly. PlayerData defines various wrapper methods for a limited interface with the world.
        /// Therefore bots can use SOME methods of world, but only those which PlayerData defines a wrapper for.
        /// </summary>
        private World _world;

        /// <summary>
        /// The building currently selected. Affects UI decisions.
        /// </summary>
        public Building selectedBuilding;

        /// <summary>
        /// Current wood value.
        /// </summary>
        private uint _wood = 1000;

        /// <summary>
        /// Current stone value.
        /// </summary>
        private uint _stone = 0;

        /// <summary>
        /// Current supply value.
        /// </summary>
        private uint _currentSupply = 0;

        /// <summary>
        /// UI panel to issue commands to selected building.
        /// </summary>
        private Panel _buildingSelectionPanel;

        /// <summary>
        /// UI panel to issue commands to selected units.
        /// </summary>
        private Panel _unitCommandPanel;

        /// <summary>
        /// UI panel to construct buildings.
        /// </summary>
        private Panel _buildingCommandPanel;

        /// <summary>
        /// The team the PlayerData is on.
        /// </summary>
        private Team _team;

        /// <summary>
        /// Gets the team the PlayerData is on.
        /// </summary>
        public Team Team
        {
            get { return _team; }
        }

        /// <summary>
        /// Gets the current wood value.
        /// </summary>
        public uint Wood
        {
            get { return _wood; }
        }

        /// <summary>
        /// Gets the current stone value.
        /// </summary>
        public uint Stone
        {
            get { return _stone; }
        }

        /// <summary>
        /// Gets the current supply value.
        /// </summary>
        public uint Supply
        {
            get { return _currentSupply; }
        }

        /// <summary>
        /// Gets the value of supply until new houses must be built.
        /// </summary>
        public uint MaxSupply
        {
            get
            {
                uint result = 10;

                foreach (Building b in _world.Buildings)
                {
                    if (b.IsActive)
                    {
                        IBoostsSupply supplyBooster = b as IBoostsSupply;

                        if (supplyBooster != null)
                        {
                            result += supplyBooster.SupplyBoostAmount;
                        }
                    }                    
                }

                return result;
            }
        }

        /// <summary>
        /// Gets how much supply can be spent before new houses must be built.
        /// </summary>
        public uint SpareSupply
        {
            get { return MaxSupply - _currentSupply; }
        }

        /// <summary>
        /// Selects the first worker which is not currently busy.
        /// </summary>
        public Worker SelectedWorker
        {
            get
            {
                foreach (Unit u in _world.SelectedUnits)
                {
                    Worker w = u as Worker;

                    if (w != null && w.FSM.CurrentState != MoveToConstructBuilding.Instance && w.FSM.CurrentState != ConstructBuilding.Instance)
                        return w;
                }

                return null;
            }
        }

        /// <summary>
        /// Initializes a new PlayerData.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="team"></param>
        public PlayerData(World w, Team team)
        {
            _world = w;
            _team = team;

            // Setup building selection panel.
            _buildingSelectionPanel = new Panel("Construct Buildings", new Vector2(250, Camera.HEIGHT - 175), new Vector2(300, 175));
            _buildingSelectionPanel.AddButton(new Button("Town Hall", new Vector2(5, 35), new Vector2(70, 30), _buildingSelectionPanel));
            _buildingSelectionPanel.AddButton(new Button("House", new Vector2(5, 68), new Vector2(70, 30), _buildingSelectionPanel));
            _buildingSelectionPanel.AddButton(new Button("Deposit Box", new Vector2(5, 101), new Vector2(70, 30), _buildingSelectionPanel));
            _buildingSelectionPanel.AddButton(new Button("Track", new Vector2(80, 35), new Vector2(70, 30), _buildingSelectionPanel));

            // Setup unit command panel.
            _unitCommandPanel = new Panel("Selected Units", new Vector2(Camera.WIDTH - 250, Camera.HEIGHT - 250), new Vector2(250, 250));
            _unitCommandPanel.AddButton(new Button("Stop", new Vector2(5, 35), new Vector2(70, 30), _unitCommandPanel));
            _unitCommandPanel.AddButton(new Button("Gather Wood", new Vector2(5, 68), new Vector2(70, 30), _unitCommandPanel));
            _unitCommandPanel.AddButton(new Button("Gather Stone", new Vector2(5, 101), new Vector2(70, 30), _unitCommandPanel));

            // Setup building command panel.
            _buildingCommandPanel = new Panel("Selected Building", new Vector2(Camera.WIDTH - 500, Camera.HEIGHT - 250), new Vector2(250, 250));
            _buildingCommandPanel.AddButton(new Button("Build 0", new Vector2(5, 101), new Vector2(70, 30), _buildingCommandPanel));
            _buildingCommandPanel.AddButton(new Button("Build 1", new Vector2(5, 134), new Vector2(70, 30), _buildingCommandPanel));
            _buildingCommandPanel.AddButton(new Button("Rally Point", new Vector2(5, 167), new Vector2(70, 30), _buildingCommandPanel));
        }

        /// <summary>
        /// Gets the world fine grid.
        /// </summary>
        public Grid Grid
        {
            get { return _world.Grid; }
        }

        /// <summary>
        /// Gets the world coarse grid.
        /// </summary>
        public Grid CoarseGrid
        {
            get { return _world.CoarseGrid; }
        }

        /// <summary>
        /// Tells the world to remove the passed in unit from the collision cells it occupies.
        /// </summary>
        /// <param name="u">The unit.</param>
        public void RemoveUnitFromCollisionCells(Unit u)
        {
            _world.collisionCells.RemoveUnit(u);
        }

        /// <summary>
        /// Tells the world to add the passed in unit to the collision cells it occupies.
        /// </summary>
        /// <param name="u">The unit.</param>
        public void AddUnitToCollisionCells(Unit u)
        {
            _world.collisionCells.AddUnit(u);
        }

        /// <summary>
        /// Updates fog of war for the passed in unit.
        /// Removes vision from cells it can longer see.
        /// Adds vision to cells it can newly see.
        /// </summary>
        /// <param name="u">The unit.</param>
        public void UpdateFogOfWarForUnit(Unit u)
        {
            _world.fogOfWar.UnitMoved(u);
        }

        /// <summary>
        /// Returns whether or not the team has explored the passed in cell.
        /// </summary>
        /// <param name="cell">The cell.</param>
        public bool HasExploredCoarseCell(Cell cell)
        {
            return _world.fogOfWar.TeamHasExploredCell(_team, CoarseGrid.IndexAt(cell.Mid));
        }

        /// <summary>
        /// Returns units in collision cells in a 3x3 grid around the passed in position.
        /// </summary>
        /// <param name="pos">The position</param>
        public List<HashSet<Unit>> GetUnitsInCollisionCellsAroundPos(Vector2 pos)
        {
            return _world.collisionCells.GetUnitsInCellsAroundPos(pos);
        }

        /// <summary>
        /// Returns units in collision cells in a 3x3 grid around the passed in unit.
        /// </summary>
        /// <param name="u">The unit</param>
        public List<HashSet<Unit>> GetUnitsInCollisionCellsAroundUnit(Unit u)
        {
            return _world.collisionCells.GetUnitsInCellsAroundUnit(u);
        }

        /// <summary>
        /// Returns units in the same collision cell as the passed in unit.
        /// </summary>
        /// <param name="u">The unit</param>
        public HashSet<Unit> GetUnitsInSameCollisionCellsAsUnit(Unit u)
        {
            return _world.collisionCells.GetUnitsInSameCellAsUnit(u);
        }

        /// <summary>
        /// Returns the resource residing in the passed in cell.
        /// Only returns if the cell has been explored.
        /// </summary>
        /// <param name="cell">The cell.</param>
        public Resource GetResourceFromCell(Cell cell)
        {
            if (cell != null)
            {
                if (_world.fogOfWar.TeamHasExploredPos(Team, cell.Mid))
                    return _world.GetResourceFromCell(cell);
            }
            
            return null;
        }

        /// <summary>
        /// Returns the track residing in the passed in cell.
        /// Only returns if the cell is currently visible.
        /// </summary>
        /// <param name="cell">The cell.</param>
        public Track GetTrackFromCell(Cell cell)
        {
            if (_world.fogOfWar.TeamCanSeePos(Team, cell.Mid))
                return _world.GetTrackFromCell(cell);
            return null;
        }

        /// <summary>
        /// Returns whether or not there is a resource residing in the passed in cell.
        /// Only returns if the cell has been explored.
        /// </summary>
        /// <param name="cell">The cell.</param>
        public bool CellHasResource(Cell cell)
        {
            if (_world.fogOfWar.TeamHasExploredPos(Team, cell.Mid))
                return _world.CellHasResource(cell);
            return false;
        }

        /// <summary>
        /// Returns whether or not there is a track residing in the passed in cell.
        /// Only returns if the cell is currently visible.
        /// </summary>
        /// <param name="cell">The cell</param>
        public bool CellHasTrack(Cell cell)
        {
            if (_world.fogOfWar.TeamCanSeePos(Team, cell.Mid))
                return _world.CellHasTrack(cell);
            return false;
        }

        /// <summary>
        /// Tells the world to add a new unit of the passed in type.
        /// </summary>
        /// <param name="unitType">The type of unit to be created.</param>
        /// <param name="pos">The position for the unit to be created at.</param>
        /// <param name="team">The team the unit belongs to.</param>
        /// <param name="rallyPoint">Where the unit should head towards upon creation.</param>
        public void AddUnit(Type unitType, Vector2 pos, Team team, Vector2 rallyPoint)
        {
            _world.AddUnit(unitType, pos, Team, rallyPoint);
        }

        /// <summary>
        /// Increments current resource values by the passed in values.
        /// Also notifies the world that this PlayerData has received resources.
        /// </summary>
        /// <param name="amount">The amount to increment by.</param>
        /// <param name="type">The type of resource to increment.</param>
        public void ReceiveResources(uint amount, ResourceType type)
        {
            if (type == ResourceType.Wood)
                _wood += amount;
            else if (type == ResourceType.Stone)
                _stone += amount;
            else
                throw new Exception("Invalid resource type in PlayerData.GiveResources");

            MsgBoard.AddMessage(this, World.MSG_ID, MessageType.ResourcesReceived);
        }

        /// <summary>
        /// Adds the units in the passed in rectangle to the SelectedUnits list.
        /// </summary>
        /// <param name="selectAt">The rectangle to select units at.</param>
        public void SelectUnitsInRect(Rectangle selectAt)
        {
            _world.SelectedUnits = new List<Unit>();

            foreach (Unit u in _world.Units)
            {
                if (selectAt.Intersects(u.CollisionRect))
                    _world.SelectedUnits.Add(u);
            }
        }

        /// <summary>
        /// Orders units in the SelectedUnits list to path towards the passed in position.
        /// It is up to individual units to determine how they interpret the request.
        /// </summary>
        /// <param name="pos"></param>
        public void MoveSelectedUnitsTo(Vector2 pos)
        {
            if (!Debug.IsOn(DebugOp.GroupPathing))
            {
                foreach (Unit u in _world.SelectedUnits)
                    u.MoveTowards(pos);
            }
            else
            {
                if (!Grid.CellAt(pos).Passable)
                    return;

                // Pick a leader (first in list, not important since everything is just relative).
                Unit leader = _world.SelectedUnits[0];
                // Fetch a path to location for it
                leader.MoveTowards(pos);
                // Use cells in that path
                List<Cell> leaderPath = leader.pathHandler.path;

                // Calcualte offset for each other unit
                // Manually set path to list of cells offset from that
                for (int i = 1; i < _world.SelectedUnits.Count; i++)
                {
                    Unit u = _world.SelectedUnits[i];

                    Vector2 offset = u.Mid - leader.Mid;
                    var offsetPath = new List<Cell>();

                    foreach (Cell c in leaderPath)
                        offsetPath.Add(Grid.CellAt(c.Mid + offset));

                    u.pathHandler.SetPath(offsetPath);
                }
            }                                   
        }

        /// <summary>
        /// Orders workers in the SelectedUnits list to harvest the closest resource of the passed in type.
        /// Will fetch a path and change state of workers.
        /// </summary>
        /// <param name="desiredResource">The resource type to start harvesting.</param>
        public void OrderSelectedWorkersToGatherClosestResource(ResourceType desiredResource)
        {
            foreach (Unit u in _world.SelectedUnits)
            {
                Worker w = u as Worker;
                
                // If this is actually a worker and not some other unit type
                if (w != null)
                {
                    w.resrcLookingFor = desiredResource;
                    w.returningResourcesTo = null;
                    w.targetResourceCell = null;
                    w.FSM.ChangeState(MoveToResource.Instance);
                }               
            }
        }

        /// <summary>
        /// Orders workers in the SelectedUnits list to construct the passed in building.
        /// Will fetch a path and change state of workers.
        /// </summary>
        /// <param name="building">The building to construct.</param>
        public void OrderSelectedWorkerToConstructBuilding(Building building)
        {
            if (SelectedWorker != null)
            {
                SelectedWorker.GoConstructBuilding(building);
            }
        }

        /// <summary>
        /// Orders all units to stop whatever they're doing.
        /// It is up to individual units to determine how they interpret this request.
        /// </summary>
        public void OrderSelectedUnitsToStop()
        {
            foreach (Unit u in _world.SelectedUnits)
                u.Stop();
        }

        /// <summary>
        /// Returns the worker closest to the passed in position that is not occupied with another task.
        /// </summary>
        /// <param name="pos">The position.</param>
        // TODO: SLOW - can be optimised with a floodfill using spatial partion cells.
        public Worker GetClosestFreeWorkerToPos(Vector2 pos)
        {
            float closestDistance = float.MaxValue;
            Worker closestWorker = null;

            foreach (Unit u in _world.Units)
            {
                if (u is Worker)
                {
                    Worker w = u as Worker;

                    if (w.FSM.CurrentState != MoveToConstructBuilding.Instance && w.FSM.CurrentState != ConstructBuilding.Instance)
                    {
                        var distance = Vector2.Distance(pos, u.Mid);

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestWorker = u as Worker;
                        }
                    }                    
                }
            }
            return closestWorker;
        }

        /// <summary>
        /// Returns all the units currently colliding with the passed in unit.
        /// Only checks units in the same coarse cell as the passed in unit.
        /// </summary>
        /// <param name="unit">The unit.</param>
        public List<Unit> GetCollidingUnits(Unit unit)
        {
            var result = new List<Unit>();

            foreach (Unit u in GetUnitsInSameCollisionCellsAsUnit(unit))
            {
                if (unit.CollisionRect.Intersects(u.CollisionRect))
                    result.Add(u);
            }

            // Remove the unit being checked from list to prevent checks when this list is used.
            result.Remove(unit);

            return result;
        }

        /// <summary>
        /// Returns the units within a given radius of the passed in unit.
        /// Used for steering behaviour neighbourhood radius calculations.
        /// </summary>
        /// <param name="unit">The unit</param>
        /// <param name="radius">The radius</param>
        public List<Unit> GetUnitsInRadius(Unit unit, float radius)
        {
            var result = new List<Unit>();

            float taggableDistance = (float)Math.Pow(radius + unit.Scale.X / 2, 2);

            foreach (HashSet<Unit> units in GetUnitsInCollisionCellsAroundUnit(unit))
            {
                foreach (Unit u in units)
                {
                    if (Vector2.DistanceSquared(unit.Mid, u.Mid) < taggableDistance)
                        result.Add(u);
                }
            }            

            // Remove the unit being checked from list to prevent checks when this list is used.
            result.Remove(unit);

            return result;
        }

        /// <summary>
        /// Returns the building at the passed in position.
        /// Returns null if there is no building there.
        /// </summary>
        /// <param name="pos">The position</param>
        public Building BuildingAtPos(Vector2 pos)
        {
            foreach (Building b in _world.Buildings)
            {
                if (b.CollisionRect.Contains(pos))
                    return b;
            }

            return null;
        }

        /// <summary>
        /// Returns the first building in the passed in rectangle.
        /// Returns null if there is no building there.
        /// Returns first because only one building can be selected at a time.
        /// </summary>
        /// <param name="rect">The rectangle</param>
        public void SelectFirstBuildingInRect(Rectangle rect)
        {
            foreach (Building b in _world.Buildings)
            {
                if (rect.Intersects(b.CollisionRect))
                {
                    selectedBuilding = b;
                    return;
                }
            }
        }

        /// <summary>
        /// Attempts to spend resources to purchase the passed in building and place it at the passed in position.
        /// This request will fail if EITHER:
        ///     - The PlayerData has insufficient resources
        ///     - The passed in position is not valid.
        /// </summary>
        /// <param name="building">The building</param>
        /// <param name="pos">The position</param>
        public bool BuyBuilding(Building building, Vector2 pos)
        {
            ProductionBuilding prodBuild = building as ProductionBuilding;

            if (prodBuild != null)
                prodBuild.ResetRallyPoint();

            // Can only buy building if area is clear and there are no minecart tracks around
            foreach (Cell c in _world.Grid.CellsInRect(building.CollisionRect))
            {
                if (_world.CellHasTrack(c))
                    return false;
            }

            if (_world.Grid.RectIsClear(building.CollisionRect))
            {                
                Type buildingType = building.GetType();

                if (CanAffordEntityType(buildingType))
                {
                    Cost cost = World.entityCosts[buildingType];

                    SpendResources(cost.woodCost, cost.stoneCost);

                    _world.AddBuilding(building);

                    // If you bought a building and you have a selected worker, it will go construct it.
                    OrderSelectedWorkerToConstructBuilding(building);

                    return true;
                }              
            }

            return false;
        }

        /// <summary>
        /// Decerements resource values corresponding to the passed in unit type.
        /// Also notifies the world that a new unit has been purchased.
        /// </summary>
        /// <param name="unitType">The unit type.</param>
        public void SpendResourcesForUnitType(Type unitType)
        {
            if (CanAffordEntityType(unitType))
            {
                Cost cost = World.entityCosts[unitType];

                SpendResources(cost.woodCost, cost.stoneCost);
                _currentSupply += cost.supplyCost;

                MsgBoard.AddMessage(this, World.MSG_ID, MessageType.SupplyChanged, info: unitType);
            }
        }

        /// <summary>
        /// Returns whether or not the PlayerData has sufficient resources to purchase an entity of the passed in type.
        /// This could be buildings OR units.
        /// </summary>
        /// <param name="entityType">The entity type/</param>
        public bool CanAffordEntityType(Type entityType)
        {
            Cost cost = World.entityCosts[entityType];
            return Wood >= cost.woodCost && Stone >= cost.stoneCost && SpareSupply >= cost.supplyCost;
        }

        /// <summary>
        /// Orders the selected building to start producing whatever type it produces at the passed in index.
        /// It is up to the individual building to determine how this request is interpreted.
        /// </summary>
        /// <param name="index"></param>
        public void QueueUpProductionOnSelectedBuildingAtIndex(int index)
        {
            if (selectedBuilding != null)
            {
                selectedBuilding.QueueUpProductionAtIndex(index);
            }
        }

        /// <summary>
        /// Decrements resource values by the passed in amount.
        /// </summary>
        /// <param name="wood">Wood amount</param>
        /// <param name="stone">Stone amount</param>
        public void SpendResources(uint wood, uint stone)
        {
            _wood -= wood;
            _stone -= stone;

            if (_wood < 0)
                _wood = 0;

            if (_stone < 0)
                _stone = 0;
        }

        /// <summary>
        /// Returns the building of the passed in Type closest to the passed in unit which has completed construction.
        /// Returns null if no buildings meet this criteria.
        /// </summary>
        /// <typeparam name="T">The building type</typeparam>
        /// <param name="u">The unit</param>
        public T GetClosestActiveBuilding<T>(Unit u) where T : Building
        {
            float closestDistance = float.MaxValue;
            Building closestBuilding = null;

            foreach (Building b in _world.Buildings)
            {
                if (b.IsActive && b is T)
                {
                    float distance = Vector2.Distance(u.Mid, b.Mid);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestBuilding = b;
                    }
                }
            }

            return closestBuilding as T;
        }

        /// <summary>
        /// Sets the rally point of the selected building to the passed in position.
        /// </summary>
        /// <param name="pos">The rally point.</param>
        public void SetSelectedBuildingRallyPointTo(Vector2 pos)
        {
            ProductionBuilding prodBuild = selectedBuilding as ProductionBuilding;

            if (prodBuild != null)
                prodBuild.rallyPoint = pos;
        }

        /// <summary>
        /// Returns the closest resource return point building to the passed in unit.
        /// This could be a TownHall or a DepositBox.
        /// </summary>
        /// <param name="u">The Unit</param>
        public Building GetClosestResourceReturnPoint(Unit u)
        {
            float closestDistance = float.MaxValue;
            Building closestBuilding = null;

            foreach (Building b in _world.Buildings)
            {
                if (b.IsActive && b as ICanAcceptResources != null)
                {
                    float distance = Vector2.Distance(u.Mid, b.Mid);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestBuilding = b;
                    }
                }                
            }

            return closestBuilding;
        }

        /// <summary>
        /// Returns a position for the passed in building where it can be validly placed.
        /// Runs a floodfill out from the current position of the building until it finds a valid one.
        /// </summary>
        /// <param name="building">The building</param>
        public Vector2 GetValidBuildingPlacementPos(Building building)
        {
            if (Grid.RectIsClear(building.CollisionRect))
                return building.Pos;

            var open = new List<Cell>();
            var closed = new List<Cell>();
            var current = Grid.CellAt(building.Pos);

            open.Add(current);

            while (open.Count > 0)
            {
                current = open[0];

                foreach (Cell cell in current.Neighbours)
                {
                    if (!open.Contains(cell) && !closed.Contains(cell))
                    {
                        open.Add(cell);

                        building.Pos = cell.Pos;

                        if (Grid.RectIsClear(building.CollisionRect))
                            return building.Pos;
                    }
                }

                open.Remove(current);
                closed.Add(current);
            }

            return Vector2.Zero;
        }

        /// <summary>
        /// Gets a valid and optimal house placement position.
        /// Utilises the influence map to find an area with a MEDIUM amount of influence which is not near resources.
        /// </summary>
        public Vector2 GetHousePlacementPos()
        {
            InfluenceMapData infMap;
            var open = new List<Cell>();
            var closed = new List<Cell>();

            // If player doesn't own a house, start from town centre - otherwise, start from existing house.
            Cell current = null;

            foreach (Building b in _world.Buildings)
            {
                if (b is House)
                {
                    current = Grid.CellAt(b.Mid);
                    break;
                }
            }

            if (current == null)
                current = Grid.CellAt(_world.Buildings[0].Mid);

            if (_team == Team.One)
                infMap = _world.playerOneInfluence;
            else
                infMap = _world.playerTwoInfluence;

            open.Add(current);

            while (open.Count > 0)
            {
                current = open[0];

                foreach (Cell cell in current.Neighbours)
                {
                    if (!closed.Contains(cell) && !open.Contains(cell))
                    {
                        open.Add(cell);

                        if (IsGoodHousePos(cell))
                            return cell.Pos;
                    }
                }

                open.Remove(current);
                closed.Add(current);
            }

            // Return zero vector if valid house pos could not be found.
            return Vector2.Zero;

            // Returns whether or not the passed in cell matches the required criteria for a good house position.
            bool IsGoodHousePos(Cell cellToCheck)
            {
                Point checkIdx = Grid.IndexAt(cellToCheck.Mid);
                float influence = infMap[checkIdx];

                // Influence must be between 20 and 70.
                if (influence < 20 || influence > 70)
                    return false;

                // Must not be within 4 x 4 space of resources.
                foreach (Cell cell in Grid.CellsInRect(cellToCheck.CollisionRect.GetInflated(128, 128)))
                {
                    if (_world.CellHasResource(cell))
                        return false;
                }

                // Must actually fit with house size. No walls or minecart tracks.
                var houseCells = new List<Cell> { cellToCheck };
                Grid.AddCell(Grid[checkIdx.Col() + 1, checkIdx.Row()], houseCells);
                Grid.AddCell(Grid[checkIdx.Col() + 1, checkIdx.Row() + 1], houseCells);
                Grid.AddCell(Grid[checkIdx.Col(), checkIdx.Row() + 1], houseCells);

                foreach (Cell cell in houseCells)
                {
                    if (CellHasTrack(cell) || !cell.Passable)
                        return false;
                }

                // Good house position!
                return true;                
            }
        }

        /// <summary>
        /// Calculates the influence border around the passed in position and returns it.
        /// Returns as a list of cells which are not necessarily spatially in order.
        /// </summary>
        /// <param name="pos">The starting position.</param>
        public List<Cell> GetInfluenceBorderAroundPos(Vector2 pos)
        {
            _world.playerOneInfluence.CalculateInfluenceBorderAroundCell(Grid.CellAt(pos));
            return _world.playerOneInfluence.InfluenceBorder;
        }

        /// <summary>
        /// The render function for the PlayerData.
        /// Renders various UI elements including selected entities, UI panels and the minimap.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to render to.</param>
        public void Render(SpriteBatch spriteBatch)
        {
            foreach (Unit u in _world.SelectedUnits)
                spriteBatch.DrawRectangle(u.RenderRect.GetInflated(3, 3), Color.SpringGreen);

            if (selectedBuilding != null)
            {
                spriteBatch.DrawRectangle(selectedBuilding.RenderRect.GetInflated(3, 3), Color.SpringGreen);

                ProductionBuilding prodBuild = selectedBuilding as ProductionBuilding;

                if (prodBuild != null)
                    spriteBatch.DrawLine(prodBuild.RenderMid, Camera.VecToScreen(prodBuild.rallyPoint), Color.SpringGreen, 1);
            }            
        }

        /// <summary>
        /// Renders UI elements for the PlayerData.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to render to.</param>
        public void RenderUI(SpriteBatch spriteBatch)
        {
            RenderMinimap(spriteBatch);

            _buildingSelectionPanel.Render(spriteBatch);
            _unitCommandPanel.Render(spriteBatch);

            _buildingCommandPanel.Render(spriteBatch);

            if (selectedBuilding != null)
            {
                spriteBatch.DrawString(MinecaRTS.smallFont, selectedBuilding.GetType().Name, new Vector2(_buildingCommandPanel.Pos.X + 5, _buildingCommandPanel.Pos.Y + 25), Color.White);

                ProductionBuilding prodBuild = selectedBuilding as ProductionBuilding;

                if (prodBuild != null)
                {
                    for (int i = 0; i < prodBuild.ProductionTypes.Count; i++)
                    {
                        spriteBatch.DrawString(MinecaRTS.smallFont, "(" + i + ") " + prodBuild.ProductionTypes[i].Name, new Vector2(_buildingCommandPanel.Pos.X + 10, _buildingCommandPanel.Pos.Y + 35 + (i * 10)), Color.White);
                    }
                }
            }

            spriteBatch.DrawString(MinecaRTS.largeFont, "WOOD: " + Wood.ToString(), new Vector2(500, 10), Color.White);
            spriteBatch.DrawString(MinecaRTS.largeFont, "STONE: " + Stone.ToString(), new Vector2(700, 10), Color.White);
            spriteBatch.DrawString(MinecaRTS.largeFont, "SUPPLY: " + _currentSupply.ToString() + "/" + MaxSupply.ToString(), new Vector2(900, 10), Color.White);
        }

        /// <summary>
        /// Returns whether or not there is a panel at the passed in position.
        /// </summary>
        public bool PanelAtPos(string panelName, Vector2 pos)
        {
            if (panelName == "Construct Buildings")
            {
                return _buildingSelectionPanel.RenderRect.Contains(pos);
            }

            return false;
        }

        /// <summary>
        /// Returns the button at the passed in position.
        /// Returns null if there is no button there.
        /// </summary>
        /// <param name="pos">The position.</param>
        public Button ButtonAtPos(Vector2 pos)
        {
            if (_buildingSelectionPanel.RenderRect.Contains(pos))
                return _buildingSelectionPanel.ButtonAtPos(pos);

            if (_unitCommandPanel.RenderRect.Contains(pos))
                return _unitCommandPanel.ButtonAtPos(pos);

            if (_buildingCommandPanel.RenderRect.Contains(pos))
                return _buildingCommandPanel.ButtonAtPos(pos);

            return null;
        }

        /// <summary>
        /// Returns whether or not the current mouse position is on a UI element.
        /// </summary>
        public bool ClickedOnUI()
        {
            Vector2 mousePos = Input.MousePos;

            if (_buildingSelectionPanel.RenderRect.Contains(mousePos))
                return true;
            if (_unitCommandPanel.RenderRect.Contains(mousePos))
                return true;
            if (_buildingCommandPanel.RenderRect.Contains(mousePos))
                return true;
            if (Camera.MinimapRect.Contains(mousePos))
                return true;

            return false;
        }

        /// <summary>
        /// Renders the minimap.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to render to.</param>
        public void RenderMinimap(SpriteBatch spriteBatch)
        {
            float xRatio = 1 + (World.Width / Camera.MINIMAP_SIZE);
            float yRatio = 1 + (World.Height / Camera.MINIMAP_SIZE);

            spriteBatch.FillRectangle(new Rectangle(Camera.MINIMAP_X, Camera.MINIMAP_Y, Camera.MINIMAP_SIZE, Camera.MINIMAP_SIZE), Color.DarkSlateGray);

            foreach (Building b in _world.Buildings)
                spriteBatch.FillRectangle(Camera.WorldRectToMinimapRect(b.CollisionRect), Color.SpringGreen);

            foreach (Unit u in _world.Units)
                spriteBatch.FillRectangle(Camera.WorldRectToMinimapRect(u.CollisionRect), Color.LawnGreen);

            foreach (Resource r in _world.Resources.Values)
            {
                if (!Debug.IsOn(DebugOp.ShowFogOfWar) || _world.fogOfWar.TeamHasExploredPos(_team, r.Mid))
                {
                    if (r.Type == ResourceType.Wood)
                        spriteBatch.FillRectangle(Camera.WorldRectToMinimapRect(r.CollisionRect), Color.Chocolate);
                    else if (r.Type == ResourceType.Stone)
                        spriteBatch.FillRectangle(Camera.WorldRectToMinimapRect(r.CollisionRect), Color.DimGray);
                }                
            }

            // Draw box representing current view
            spriteBatch.DrawRectangle(Camera.WorldRectToMinimapRect(Camera.Rect), Color.White);

            spriteBatch.DrawRectangle(Camera.MinimapRect, Color.White, 3);

        }

        public void TestPathfindingCalculationTime()
        {
            Unit u = new Unit(this, Team, new Vector2(50, 50), new Vector2(20, 20));

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            double total = 0;

            for (int loop = 0; loop < 30; loop++)
            {
                sw.Start();

                int numTests = 1000000;

                for (int i = 0; i < numTests; i++)
                    u.MoveTowards(Grid[Grid.Cols - 1, Grid.Rows - 1].Mid);
                sw.Stop();

                total += sw.Elapsed.TotalMilliseconds / numTests;

                Console.WriteLine(sw.Elapsed.TotalMilliseconds / numTests);

                sw.Reset();
            }

            Console.WriteLine("Global Average: " + total / 30);
        }
    }
}
