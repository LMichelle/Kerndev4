using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public float distanceBetweenNodes;

    private Node[,] NodeArray;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;
    [SerializeField]
    private Transform startPos;

    private void Start()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    public void CreateGrid()
    {
        NodeArray = new Node[gridSizeX, gridSizeY];
        Vector3 bottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                NodeArray[x, y] = new Node(worldPoint, x, y);
                NodeArray[x, y].walls = Wall.EAST | Wall.SOUTH | Wall.NORTH | Wall.WEST; // this sets all walls
                //if (x == 0)
                //    NodeArray[x, y].walls |= Wall.WEST;
                //if (x == gridSizeX - 1)
                //    NodeArray[x, y].walls |= Wall.EAST;
                //if (y == 0)
                //    NodeArray[x, y].walls |= Wall.SOUTH;
                //if (y == gridSizeY - 1)
                //    NodeArray[x, y].walls |= Wall.NORTH;
            }
        }

        Stack<Node> nodeStack = new Stack<Node>(); // Last in, first out
        List<Node> checkedNodes = new List<Node>();
        nodeStack.Push(NodeArray[0, 0]);
        Node currentNode;
        while (nodeStack.Count > 0)
        {
            currentNode = nodeStack.Pop();
            List<Node> neighbours = GetNeighbourNodes(currentNode);
            neighbours = GetUnvisitedNeighbourNodes(neighbours, checkedNodes, nodeStack);
            if (neighbours.Count > 1)
            {
                nodeStack.Push(currentNode);
            }
            if (neighbours.Count != 0)
            {
                Node randomNeighbourNode = neighbours[Random.Range(0, neighbours.Count)];
                RemoveWallBetweenNodes(currentNode, randomNeighbourNode);
                checkedNodes.Add(randomNeighbourNode);
                nodeStack.Push(randomNeighbourNode);
            }
        }

        //Debug.Log(NodeArray[0, 0].walls);
        //Debug.Log(NodeArray[gridSizeX - 1, gridSizeY - 1].walls);
        //Debug.Log(NodeArray[4, 7].walls);
    }

    private List<Node> GetUnvisitedNeighbourNodes(List<Node> neighbouringNodesList, List<Node> checkedNodes, Stack<Node> nodeStack)
    {
        List<Node> unvisistedNeighbours = new List<Node>();
        foreach(Node n in neighbouringNodesList)
        {
            if (!checkedNodes.Contains(n) && !nodeStack.Contains(n))
            {
                unvisistedNeighbours.Add(n);
            }
        }

        return unvisistedNeighbours;
    }

    private List<Node> GetNeighbourNodes(Node thisNode)
    {
        List<Node> neighbouringNodesList = new List<Node>();
        int checkX;
        int checkY;

        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                checkX = thisNode.gridX + x;
                checkY = thisNode.gridY + y;
                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) // if it's within
                {
                    neighbouringNodesList.Add(NodeArray[checkX, checkY]);
                }
            }
        }
        return neighbouringNodesList;
    }

    private void RemoveWallBetweenNodes(Node nodeA, Node nodeB)
    {
        //int amountWallsNodeA = nodeA.GetWalls();
        Vector2Int direction = new Vector2Int(nodeB.gridX, nodeB.gridY) - new Vector2Int(nodeA.gridX, nodeA.gridX);
        if(direction.x != 0) // if NodeA and NodeB are not on the same x coordinate
        {
            // if NodeA's direction is 1, than it lies to the west of NodeB. It's east wall needs to go.
            // If it is -1, than the node lies to the east of NodeB, in which case the wall on the west side needs to go.
            nodeA.RemoveWall(direction.x > 0 ? Wall.EAST : Wall.WEST); 
            nodeB.RemoveWall(direction.x > 0 ? Wall.WEST : Wall.EAST); 
        }
        if (direction.y != 0)
        {
            nodeA.RemoveWall(direction.y > 0 ? Wall.NORTH : Wall.SOUTH);
            nodeB.RemoveWall(direction.y > 0 ? Wall.SOUTH : Wall.NORTH);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, .1f));
        
        if (NodeArray != null)
        {
            foreach(Node n in NodeArray)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(n.pos, new Vector3(nodeDiameter - distanceBetweenNodes, nodeDiameter - distanceBetweenNodes, .1f));
                if ((n.walls & Wall.NORTH) != 0) // if n has a wall north
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(n.pos + new Vector3(0, nodeRadius), new Vector3(nodeDiameter, nodeDiameter * .1f, .2f));
                }
                if ((n.walls & Wall.EAST) != 0)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(n.pos + new Vector3(nodeRadius, 0), new Vector3(nodeDiameter * .1f, nodeDiameter, .2f));
                }
                if ((n.walls & Wall.WEST) != 0) 
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(n.pos + new Vector3(-nodeRadius, 0), new Vector3(nodeDiameter * .1f, nodeDiameter, .2f));
                }
                if ((n.walls & Wall.SOUTH) != 0)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(n.pos + new Vector3(0, -nodeRadius), new Vector3(nodeDiameter, nodeDiameter * .1f, .2f));
                }
            }
        }
    }
}
