using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node
{
    public List<Node> neighbors;
    public Vector2 pos;

    public bool isFree;


    public bool hasBeenVisited = false;
    public bool isPath = false;

    public Node cameFrom = null;

    public float cost;
    public float currentCost = 0;
    
}

public class MapNavigation : MonoBehaviour
{
    private Node[,] nodes;
    private CellularAutomata mapScript;

    // Start is called before the first frame update
    void Start()
    {
        mapScript = GameManager.Instance.MapScript;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(Cell[,] cells, List<Room> rooms)
    {
        nodes = new Node[mapScript.MapSize * 2 + 1, mapScript.MapSize * 2 + 1];
        for (int x = 0; x < mapScript.MapSize; x++)
        {
            for (int y = 0; y < mapScript.MapSize; y++)
            {
                if (cells[x, y].isAlive && cells[x, y].room != -1)
                {
                   
                    BoundsInt boundsCell = new BoundsInt(-1, -1, 0, 3, 3, 1);
                    foreach (Vector2Int b in boundsCell.allPositionsWithin)
                    {
                        if (x + b.x < 0 || x + b.x >= mapScript.MapSize || y + b.y < 0 || y + b.y >= mapScript.MapSize) continue;
                        if (nodes[x * 2 + b.x, y * 2 + b.y] != null) continue;
                        if (!cells[x + b.x, y + b.y].isAlive || cells[x + b.x, y + b.y].room == -1) continue;

                        Node newNode = new Node
                        {
                            pos = new Vector2(x + 0.5f / 2, y + 0.5f),
                            neighbors = new List<Node>(),
                            isFree = true

                        };
                        if (cells[x, y].room > 0)
                        {
                            if (rooms[cells[x, y].room].occuped)
                            {
                                newNode.cost = 5;
                            }
                            else
                            {
                                newNode.cost = 1;
                            }
                        }
                        else
                        {
                            newNode.cost = 1;
                        }
                        newNode.pos = new Vector2(x + 0.5f + b.x / 2f, y + 0.5f + b.y / 2f);
                        nodes[x * 2 + b.x, y * 2 + b.y] = newNode;
                    }
                }
            }
        }


        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        for (int x = 0; x < mapScript.MapSize * 2 + 1; x++)
        {
            for (int y = 0; y < mapScript.MapSize * 2 + 1; y++)
            {
                Node node = nodes[x, y];
                if (node == null) continue;
                if (!node.isFree) continue;
                foreach (Vector2Int b in bounds.allPositionsWithin)
                {
                    if (x + b.x < 0 || x + b.x >= mapScript.MapSize * 2 + 1 || y + b.y < 0 ||
                        y + b.y >= mapScript.MapSize * 2 + 1) continue;
                    if (b.x == 0 && b.y == 0) continue;
                    if (nodes[x + b.x, y + b.y] == null) continue;
                    if (!nodes[x + b.x, y + b.y].isFree) continue;

                    if (x != b.x && y != b.y)
                    {
                        if (nodes[x, y + b.y] == null) continue;
                        if (!nodes[x, y + b.y].isFree) continue;
                        if (nodes[x + b.x, y] == null) continue;
                        if (!nodes[x + b.x, y].isFree) continue;
                    }

                    node.neighbors.Add(nodes[x + b.x, y + b.y]);
                }
            }
        }
    }

    public Node GetNode(Vector2 pos)
    {
        Node returnNode = null;
        float minDistance = float.MaxValue;

        foreach (Node node in nodes)
        {
            if (node == null) continue;
            if (!node.isFree) continue;

            if (Vector2.Distance(node.pos, pos) < minDistance)
            {
                minDistance = Vector2.Distance(node.pos, pos);
                returnNode = node;
            }
        }
        return returnNode;
    }

    public List<Vector2> Astar(Vector2 goalPos, Vector2 startPos)
    {
        Node startingNode = GetNode(startPos);
        Node goalNode = GetNode(goalPos);
        List<Node> openList = new List<Node> { startingNode };
        List<Node> closedList = new List<Node>();
        List<Vector2> path = new List<Vector2>();
        int crashValue = 10000;

        while (openList.Count > 0 && --crashValue > 0)
        {
            openList = openList.OrderBy(x => x.currentCost + Vector2.Distance(x.pos, goalNode.pos) * 5).ToList(); // WTF ?????

            Node currentNode = openList[0];
            openList.RemoveAt(0);

            currentNode.hasBeenVisited = true;

            closedList.Add(currentNode);

            if (currentNode == goalNode)
            {
                break;
            }
            else
            {
                foreach (Node currentNodeNeighbour in currentNode.neighbors)
                {

                    float modifier;
                    if (currentNode.pos.x == currentNodeNeighbour.pos.x ||
                        currentNode.pos.y == currentNodeNeighbour.pos.y)
                    {
                        modifier = 1;
                    }
                    else
                    {
                        modifier = 1.414f;
                    }

                    float newCost = currentNode.currentCost + currentNodeNeighbour.cost * modifier;

                    if (currentNodeNeighbour.currentCost == 0 && currentNodeNeighbour != startingNode || currentNodeNeighbour.currentCost > newCost)
                    {
                        currentNodeNeighbour.cameFrom = currentNode;
                        currentNodeNeighbour.currentCost = newCost;

                        openList.Add(currentNodeNeighbour);
                    }
                }
            }
            
        }

        if (crashValue <= 0)
        {
            Debug.Log("Nico a fait de la merde, CHEH");
        }


        {
            Node currentNode = goalNode;
            while (currentNode.cameFrom != null)
            {
                currentNode.isPath = true;
                path.Add(currentNode.pos);
                currentNode = currentNode.cameFrom;
            }

            currentNode.isPath = true;
        }
        ResetNode();
        return path;
    }

    private void ResetNode()
    {
        foreach (Node node in nodes)
        {
            if (node == null) continue;
            node.cameFrom = new Node();
            node.currentCost = 0;
            node.hasBeenVisited = false;
            node.isPath = false;
        }
    }

    void OnDrawGizmos()
    {
        if (nodes == null) return;
        if (nodes != null) return;
        foreach (Node node in nodes)
        {
            if (node == null) continue;
            Gizmos.color = node.isFree ? Color.blue : Color.red;

            if (node.hasBeenVisited)
            {
                Gizmos.color = new Color(1, 1 / (node.currentCost/500), 1, 1);
            }

            if (node.isPath)
            {
                Gizmos.color = Color.green;
            }

            Gizmos.DrawWireSphere(node.pos, 0.1f);

            foreach (Node nodeNeighbor in node.neighbors)
            {
                Gizmos.DrawLine(node.pos, nodeNeighbor.pos);
            }
        }
    }
}
