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
    /// Handles the level editing functionality of the program.
    /// </summary>
    public static class WorldEditor
    {
        /// <summary>
        /// The process method. This is called once per frame and will handle any editing input.
        /// </summary>
        /// <param name="world"></param>
        public static void HandleInput(World world)
        {
            // Fetch cell at start of method so we only call CellAtMousePos() once.
            var cell = world.Grid.CellAtMousePos();

            if (Input.KeyTyped(Keys.C))
                world.Grid.AddColumns(1);

            if (Input.KeyTyped(Keys.R))
                world.Grid.AddRows(1);

            if (Input.KeyDown(Keys.U))
                world.AddUnit(typeof(Worker), Camera.VecToWorld(Input.MousePos), Team.One);

            if (Input.KeyTyped(Keys.M))
                world.AddUnit(typeof(Minecart), Camera.VecToWorld(Input.MousePos), Team.One);

            // Make the cell a tree (get wood)
            if (Input.KeyDown(Keys.W))
                world.AddResourceToCell(new Resource(cell.Pos, new Vector2(Cell.CELL_SIZE, Cell.CELL_SIZE), ResourceType.Wood), cell);

            // Make the cell stone
            if (Input.KeyDown(Keys.S))
                world.AddResourceToCell(new Resource(cell.Pos, new Vector2(Cell.CELL_SIZE, Cell.CELL_SIZE), ResourceType.Stone), cell);

            // Make the cell a wall.
            if (Input.LeftMouseDown())
            {
                cell.Passable = false;
                cell.Color = Color.Black;
            }

            // Clear the cell.
            if (Input.RightMouseDown())
            {
                cell.Passable = true;
                world.RemoveResourceFromCell(cell);
                cell.Color = Color.Gray;
            }
        }
    }
}