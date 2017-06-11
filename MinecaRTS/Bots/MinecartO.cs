using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MinecaRTS
{
    /// <summary>
    /// The MinecartO bot!
    /// Simple bot implemented for the project which has the following goal sequence:
    ///     - Find some Wood
    ///     - Build a base there
    ///     - Find some stone
    ///     - Build a track network from base out to the stone
    ///     - Build a deposit box and a Minecart to traffic the stone back to base
    ///     - Build a perimeter around the base
    ///     
    /// MinecartO also utilises the Message-Based system in conjunction with its goals.
    /// </summary>
    public class MinecartO : Bot
    {
        /// <summary>
        /// The current goals MinecartO knows about. Stored in a stack to allow for an interrupt-and-resume style.
        /// </summary>
        private Stack<Goal<Worker, MinecartO>> goals;

        /// <summary>
        /// Workers which are busy. These will not be considered for some tasks requiring worker selection.
        /// </summary>
        private List<Worker> busyWorkers;

        /// <summary>
        /// All the areas containing wood that MinecartO knows about.
        /// </summary>
        public List<Cell> knownWoodCells;

        /// <summary>
        /// All the areas containing stone that MinecartO knows about.
        /// </summary>
        public List<Cell> knownStoneCells;

        /// <summary>
        /// The first Town Hall built by MinecartO. Serves as a reference point for many decisions.
        /// </summary>
        public TownHall townHall;

        /// <summary>
        /// The Deposit Box currently being treated as a place of importance.
        /// Used as a reference point for many decisions.
        /// </summary>
        public DepositBox targetDepositBox;

        /// <summary>
        /// Whether or not MinecartO is currently building a house.
        /// Used to prevent accidentally building multiple houses at once.
        /// </summary>
        private bool isBuildingHouse = false;

        /// <summary>
        /// Initializes a new MinecartO bot.
        /// </summary>
        /// <param name="data">The PlayerData through which all requests are filtered.</param>
        public MinecartO(PlayerData data) : base(data)
        {
            goals = new Stack<Goal<Worker, MinecartO>>();
            busyWorkers = new List<Worker>();
            knownWoodCells = new List<Cell>();
            knownStoneCells = new List<Cell>();
        }

        /// <summary>
        /// The method containing all the decision-making logic for MinecartO.
        /// </summary>
        public override void HandleInput()
        {
            #region Add MinecartO Goals as Debug Text
            if (goals.Count > 0)
            {
                foreach (Goal<Worker, MinecartO> goal in goals)
                {
                    var compGoal = goal as CompositeGoal<Worker, MinecartO>;

                    if (compGoal != null)
                    {
                        foreach (string str in compGoal.ToString(0))
                        {
                            Debug.HookText(str);
                        }
                    }
                    else
                    {
                        Debug.HookText(goal.ToString(0));
                    }
                }
            }
            #endregion Add MinecartO Goals as Debug Text

            // Starts the simulation.
            if (Input.KeyTyped(Keys.R))
            {
                Worker w = Data.GetClosestFreeWorkerToPos(Vector2.Zero);
                goals.Push(new GoalBuildPerimeter(w, this));
                goals.Push(new GoalBuildTrackNetwork(w, this));
                goals.Push(new GoalFindResource(w, this, ResourceType.Stone));
                goals.Push(new GoalBuildExpansion(w, this, ResourceType.Wood));
                busyWorkers.Add(w);
                goals.Peek().Activate();
            }
                
            // Process the first goal on the stack.
            if (goals.Count > 0)
            {
                goals.Peek().Process();

                // Move to the next goal if current goal has been completed.
                if (goals.Peek().State == GoalState.Complete)
                {
                    goals.Pop().Terminate();

                    if (goals.Count > 0)
                        goals.Peek().Activate();
                }
            }                
        }

        public override void OnUnitSpawn(Unit newUnit)
        {
            Type unitType = newUnit.GetType();

            if (unitType == typeof(Worker))
            {
                Worker w = newUnit as Worker;
                w.resrcLookingFor = ResourceType.Wood;
                w.FSM.ChangeState(MoveToResource.Instance);
            }
            else if (unitType == typeof(Minecart))
            {
                Minecart m = newUnit as Minecart;
                m.TargetDepositBox = targetDepositBox;
                m.FSM.ChangeState(MoveToDepositBox.Instance);
            }
        }

        public override void OnProductionBuildingTaskComplete(ProductionBuilding prodBuild)
        {
            if (Data.Supply < 40)
                prodBuild.QueueUpProductionAtIndex(0);
        }        

        public override void OnSupplyChange()
        {
            if (Data.SpareSupply <= 4)
                TryBuildHouse();
             
        }

        public override void OnBuildingComplete(Building building)
        {
            if (building is House)
                isBuildingHouse = false;

            var prodBuild = building as ProductionBuilding;

            if (prodBuild != null)
                prodBuild.QueueUpProductionAtIndex(0);

            foreach (HashSet<Unit> unitsInCell in Data.GetUnitsInCollisionCellsAroundPos(building.Mid))
            {
                foreach (Unit unit in unitsInCell)
                {
                    Worker w = unit as Worker;

                    if (w != null)
                    {
                        if (w.FSM.CurrentState == null && !busyWorkers.Contains(w))
                        {
                            if (w.resrcLookingFor == ResourceType.None)
                                w.resrcLookingFor = ResourceType.Wood;

                            w.FSM.ChangeState(MoveToResource.Instance);
                        }
                    }                       
                }
            }
        }

        public override void OnResourcesReceived()
        {
            if (Data.CanAffordEntityType(typeof(Worker)))
            {
                if (Data.Supply < 40 && townHall.BeingProduced == null)
                    townHall.QueueUpProductionAtIndex(0);
            }

            if (Data.SpareSupply <= 4)
                TryBuildHouse();
        }

        /// <summary>
        /// Attempts to build a house. This depends on available resources and positioning.
        /// </summary>
        private void TryBuildHouse()
        {
            if (isBuildingHouse)
                return;

            Vector2 housePos = Data.GetHousePlacementPos();

            if (housePos != Vector2.Zero)
            {
                House house = BuildingFactory.CreateHouse(Data, housePos);

                if (Data.BuyBuilding(house, housePos))
                {
                    Worker worker = Data.GetClosestFreeWorkerToPos(housePos);

                    if (worker != null)
                    {
                        worker.GoConstructBuilding(house);
                        isBuildingHouse = true;
                    }
                }
            }
        }
    }
}
