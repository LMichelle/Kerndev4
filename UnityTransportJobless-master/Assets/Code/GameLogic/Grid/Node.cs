using UnityEngine;
using System;

namespace KernDev.GameLogic
{
    public class Node
    {
        public int gridX, gridY;
        public Vector3 pos;
        public Wall walls;
        public bool DungeonExit { get; set; }
        public bool Monster { get; set; }
        public bool Treasure { get; set; }
        public int TreasureAmount { get; set; }

        public Node(Vector3 pos, int gridX, int gridY)
        {
            this.gridX = gridX;
            this.gridY = gridY;
            this.pos = pos;
        }

        public Wall GetOpenDirection()
        {
            // Thank you Google
            Wall open = walls;
            Wall allValues = (Wall)0;
            foreach (var v in Enum.GetValues(typeof(Wall)))
                allValues |= (Wall)v;
            var compliment = allValues & ~(open);

            return compliment;
        }

        public void RemoveWall(Wall wall)
        {
            // walls = true, ~wall (wall holds something, but gets removed with ~) is not true, so the statement is not true at this bit. (and thus removed)
            walls = (walls & ~wall);
        }
    }

    [System.Flags]
    public enum Wall
    {
        NORTH = 0x1,
        EAST = 0x2,
        SOUTH = 0x4,
        WEST = 0x8
    }
}