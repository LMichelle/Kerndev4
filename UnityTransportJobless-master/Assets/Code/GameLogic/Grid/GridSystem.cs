﻿using System.Collections.Generic;
using UnityEngine;

namespace KernDev.GameLogic
{
    public class GridSystem : MonoBehaviour
    {
        public GameObject nodeObject;
        public GameObject wallObject;

        public Vector2 gridWorldSize;
        public float nodeRadius;
        public float distanceBetweenNodes;

        public Node[,] NodeArray { get; private set; }
        private float nodeDiameter;
        private int gridSizeX, gridSizeY;
        public bool finishedGenerating = false;

        public void StartGrid()
        {
            nodeDiameter = nodeRadius * 2;
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
            CreateGrid();
        }

        public void CreateGrid()
        {
            NodeArray = new Node[gridSizeX, gridSizeY];
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    NodeArray[x, y] = new Node(new Vector3(x, y, 0), x, y);
                    NodeArray[x, y].walls = Wall.EAST | Wall.SOUTH | Wall.NORTH | Wall.WEST; // this sets all walls
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

           //SpawnMazeObjects();
            finishedGenerating = true;
        }

        private List<Node> GetUnvisitedNeighbourNodes(List<Node> neighbouringNodesList, List<Node> checkedNodes, Stack<Node> nodeStack)
        {
            List<Node> unvisistedNeighbours = new List<Node>();
            foreach (Node n in neighbouringNodesList)
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
                    if (checkX < 0 || checkX >= gridSizeX || checkY < 0 || checkY >= gridSizeY || Mathf.Abs(x) == Mathf.Abs(y))
                    {
                        // if the checkX or checkY are outside of the grid, or if the nodes are diagonal, then skip them
                        continue;
                    }
                    neighbouringNodesList.Add(NodeArray[checkX, checkY]);
                }
            }
            return neighbouringNodesList;
        }

        private void RemoveWallBetweenNodes(Node nodeA, Node nodeB)
        {
            Vector2Int direction = new Vector2Int(nodeB.gridX, nodeB.gridY) - new Vector2Int(nodeA.gridX, nodeA.gridY);
            if (direction.x != 0) // if NodeA and NodeB are not on the same x coordinate
            {
                // if NodeA's direction is 1, then it lies to the west of NodeB. It's east wall needs to go.
                // If it is -1, then the node lies to the east of NodeB, in which case the wall on the west side needs to go.
                nodeA.RemoveWall(direction.x > 0 ? Wall.EAST : Wall.WEST);
                nodeB.RemoveWall(direction.x > 0 ? Wall.WEST : Wall.EAST);
            }
            if (direction.y != 0)
            {
                nodeA.RemoveWall(direction.y > 0 ? Wall.NORTH : Wall.SOUTH);
                nodeB.RemoveWall(direction.y > 0 ? Wall.SOUTH : Wall.NORTH);
            }
        }

        // testing
        private void SpawnMazeObjects()
        {
            // Create a parent gameobject to orden the hierarchy
            GameObject gridParent = new GameObject();
            gridParent.transform.position = Vector3.zero;
            gridParent.name = "Grid Parent";

            if (NodeArray != null)
            {
                foreach (Node n in NodeArray)
                {
                    GameObject node = Instantiate(nodeObject, n.pos, Quaternion.identity, gridParent.transform);
                    if ((n.walls & Wall.NORTH) != 0) // if n has a wall north
                    {
                        GameObject wallN = Instantiate(wallObject, n.pos + new Vector3(0, nodeRadius), Quaternion.identity, node.transform);

                        wallN.transform.localScale = new Vector3(nodeDiameter, nodeDiameter * .1f, .2f);
                    }
                    if ((n.walls & Wall.EAST) != 0)
                    {
                        GameObject wallE = Instantiate(wallObject, n.pos + new Vector3(nodeRadius, 0), Quaternion.identity, node.transform);
                        wallE.transform.localScale = new Vector3(nodeDiameter * .1f, nodeDiameter, .2f);
                    }
                    if ((n.walls & Wall.WEST) != 0)
                    {
                        GameObject wallW = Instantiate(wallObject, n.pos + new Vector3(-nodeRadius, 0), Quaternion.identity, node.transform);
                        wallW.transform.localScale = new Vector3(nodeDiameter * .1f, nodeDiameter, .2f);
                    }
                    if ((n.walls & Wall.SOUTH) != 0)
                    {
                        GameObject wallS = Instantiate(wallObject, n.pos + new Vector3(0, -nodeRadius), Quaternion.identity, node.transform);
                        wallS.transform.localScale = new Vector3(nodeDiameter, nodeDiameter * .1f, .2f);
                    }
                }
            }
        }

        public ref Node GetRandomNode()
        {
            return ref NodeArray[Random.Range(0, NodeArray.GetLength(0)), Random.Range(0, NodeArray.GetLength(1))];
        }

        public ref Node GetSpecificNodeInstance(Node node)
        {
            return ref NodeArray[node.gridX, node.gridY];
        }

        public ref Node GetSpecificNeighbourNode(Node currentNode, Wall movedDirection)
        {
            int checkX = 0, checkY = 0;
            if ((movedDirection & Wall.NORTH) != 0) // if movedDirection == Wall.north bitwise stuff
                checkY += 1;
            if ((movedDirection & Wall.EAST) != 0)
                checkX += 1;
            if ((movedDirection & Wall.SOUTH) != 0)
                checkY -= 1;
            if ((movedDirection & Wall.WEST) != 0)
                checkX -= 1;

            checkX = currentNode.gridX + checkX;
            checkY = currentNode.gridY + checkY;
            if (checkX < 0 || checkX >= gridSizeX || checkY < 0 || checkY >= gridSizeY) // I don't really know how to deny this, so let's just keep the node the same
                return ref NodeArray[currentNode.gridX, currentNode.gridY];


            return ref NodeArray[checkX, checkY];
        }

    }
}