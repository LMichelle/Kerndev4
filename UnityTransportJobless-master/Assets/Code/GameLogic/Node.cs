using UnityEngine;

public class Node
{
    public int gridX, gridY;
    public Vector3 pos;
    public Wall walls;
    
    public Node(Vector3 pos, int gridX, int gridY)
    {
        this.gridX = gridX;
        this.gridY = gridY;
        this.pos = pos;
    }

    public int GetWalls()
    {
        int amountOfWalls = 0;
        // bitwise operators; walls has to be true (not null) and wall.north e.g. has to be true. Since true is 1, and 1 != 0, the statement is true.
        if ((walls & Wall.NORTH) != 0) { amountOfWalls++; } 
        if ((walls & Wall.EAST) != 0) { amountOfWalls++; }
        if ((walls & Wall.SOUTH) != 0) { amountOfWalls++; }
        if ((walls & Wall.WEST) != 0) { amountOfWalls++; }
        return amountOfWalls;
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
