using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class CellularAutomata : MonoBehaviour
{
    [SerializeField] [Range(0, 250)] private int size = 50;
    [SerializeField] [Range(0, 100)] private int iteration = 10;
    [SerializeField] private int sizeOfRoom = 10;
    [SerializeField] private int nbEnemy = 10;
    [SerializeField] private int nbChest = 10;
    [SerializeField] private int nbPenguin = 10;
    [SerializeField] private TileBase tile;
    [SerializeField] private SO_Enemy[] enemies;
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private GameObject penguinPrefab;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private int seed;



    struct Cell
    {
        public bool isAlive;
        public bool futureState;
        public int region;
        public int enemyRegion;
        public int chestRegion;
        public int room;
        public bool occuped;
        public Vector2Int position;
    }

    private Cell[,] cells;

    private List<List<Cell>> regionOfCell;
    private List<List<Cell>> roomOfCell;

    bool isRunning;


    private int currentRegion = 0;
    private int biggestRegion = 0;
    private int currentEnemyRegion = 0;
    private int currentChestRegion = 0;
    private int currentRoom = 0;
    private List<Color> colors;





    void Start()
    {
        if (seed == 0)
        {
            seed = Random.Range(0, 10000);
        }
        Random.InitState(seed);
        //Create array
        cells = new Cell[size, size];
        regionOfCell = new List<List<Cell>>();
        roomOfCell = new List<List<Cell>>();
        colors = new List<Color> {
            new Color(1, 1, 1, 0.2f),
            new Color(1, 0, 0, 0.2f),
            new Color(0, 0, 1, 0.2f),
            new Color(0, 1, 0, 0.2f),
            new Color(1, 0, 1, 0.2f),
            new Color(0, 1, 1, 0.2f),
            new Color(1, 1, 0, 0.2f),
        };

        //Fille array by random
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float f = Random.Range(0f, 1f);
                cells[x, y].isAlive = f > 0.5f;
            }
        }

        WorldGeneration();

    }

    void WorldGeneration()
    {
        isRunning = true;

        Init();

        for (int i = 0; i < iteration; i++)
        {
            Cellular();
        }

        GenerateRegion();

        GenerateBridge();

        //GenerateRoom();
        //Cut Cube
        //CutCube();

        //Generate cube
        GenerateCube();



        GameManager.Instance.MapLoaded();

        for (int i = 0; i < nbEnemy; i++)
        {
            GenerateEnemy();
        }

        for (int i = 0; i < nbChest; i++)
        {
            GenerateChest();
        }

        for (int i = 0; i < nbPenguin; i++)
        {
            GeneratePenguin();
        }
    }

    void Init()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                cells[x, y] = new Cell();

                cells[x, y].region = -1;
                cells[x, y].room = -1;
                cells[x, y].enemyRegion = -1;
                cells[x, y].chestRegion = -1;
                cells[x, y].position = new Vector2Int(x, y);

                float isAlive = Random.Range(0f, 1f);

                cells[x, y].isAlive = isAlive < 0.5f;
            }
        }
    }


    void Cellular()
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int neighboursAlive = 0;

                //Check Neighbours
                foreach (Vector3Int b in bounds.allPositionsWithin)
                {
                    if (b.x == 0 && b.y == 0) continue;
                    if (x + b.x < 0 || x + b.x >= size) continue;
                    if (y + b.y < 0 || y + b.y >= size) continue;

                    if (cells[x + b.x, y + b.y].isAlive)
                    {
                        neighboursAlive++;
                    }
                }

                //Apply rules
                if (!cells[x, y].isAlive && neighboursAlive >= 5)
                {
                    cells[x, y].futureState = true;
                }
                else if (cells[x, y].isAlive && (neighboursAlive >= 4 || neighboursAlive == 1))
                {
                    cells[x, y].futureState = true;
                }
                else
                {
                    cells[x, y].futureState = false;
                }
            }
        }

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    cells[x, y].isAlive = cells[x, y].futureState;
                }
            }
        }
    }



    void GenerateRegion()
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);
        regionOfCell.Add(new List<Cell>());
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (!cells[x, y].isAlive) continue;
                if (cells[x, y].region != -1) continue;

                List<Vector2Int> openList = new List<Vector2Int>();
                List<Vector2Int> closedList = new List<Vector2Int>();

                openList.Add(new Vector2Int(x, y));

                while (openList.Count > 0)
                {
                    cells[openList[0].x, openList[0].y].region = currentRegion;
                    regionOfCell[currentRegion].Add(cells[openList[0].x, openList[0].y]);
                    closedList.Add(openList[0]);

                    foreach (Vector2Int b in bounds.allPositionsWithin)
                    {
                        //Check not self
                        if (b.x == 0 && b.y == 0) continue;

                        //Check if is on cross
                        if (b.x != 0 && b.y != 0) continue;

                        Vector2Int pos = new Vector2Int(openList[0].x + b.x, openList[0].y + b.y);

                        //Check inside bounds
                        if (pos.x < 0 || pos.x >= size || pos.y < 0 || pos.y >= size) continue;

                        //Check is alive
                        if (!cells[pos.x, pos.y].isAlive) continue;

                        //check region not yet associated
                        if (cells[pos.x, pos.y].region != -1) continue;

                        //Check if already visited
                        if (closedList.Contains(pos)) continue;

                        //Check if already set to be visited
                        if (openList.Contains(pos)) continue; //Error

                        openList.Add(new Vector2Int(pos.x, pos.y));
                        
                    }
                    openList.RemoveAt(0);
                }

                if (regionOfCell[biggestRegion].Count > regionOfCell[currentRegion].Count)
                {
                    biggestRegion = currentRegion;
                }
                currentRegion++;
                regionOfCell.Add(new List<Cell>());
                
            }
        }
        regionOfCell.RemoveAt(regionOfCell.Count-1);
    }

    void GenerateBridge()
    {

        Vector2Int currentPosition = new Vector2Int();

        Vector2Int targetPosition = regionOfCell[biggestRegion][Random.Range(0, regionOfCell[biggestRegion].Count)].position;

        for (int i = 0; i < regionOfCell.Count; i++)
        {
            if (regionOfCell[i].Count > 10 && i != biggestRegion)
            {
                int rdmI = Random.Range(0, regionOfCell[i].Count);

                currentPosition = regionOfCell[i][rdmI].position;

                while (currentPosition != targetPosition)
                {
                    if (Mathf.Abs(currentPosition.x - targetPosition.x) >
                        Mathf.Abs(currentPosition.y - targetPosition.y))
                    {
                        currentPosition.x -= Math.Sign(currentPosition.x - targetPosition.x);
                    }
                    else
                    {
                        currentPosition.y -= Math.Sign(currentPosition.y - targetPosition.y);
                    }

                    if (!cells[currentPosition.x, currentPosition.y].isAlive)
                    {
                        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

                        foreach (Vector3Int b in bounds.allPositionsWithin)
                        {
                            if (currentPosition.x + b.x < 0 || currentPosition.x + b.x >= size) continue;
                            if (currentPosition.y + b.y < 0 || currentPosition.y + b.y >= size) continue;
                            cells[currentPosition.x + b.x, currentPosition.y + b.y].isAlive = true;
                        }
                    }
                }
            }
        }
    }


    void GenerateRoom()
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);
        roomOfCell.Add(new List<Cell>());
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (!cells[x, y].isAlive) continue;
                if (cells[x, y].room != -1) continue;

                List<Vector2Int> openList = new List<Vector2Int>();
                List<Vector2Int> closedList = new List<Vector2Int>();

                openList.Add(new Vector2Int(x, y));
                int timer = 0;
                while (openList.Count > 0)
                {
                    cells[openList[0].x, openList[0].y].room = currentRoom;
                    roomOfCell[currentRoom].Add(cells[openList[0].x, openList[0].y]);
                    closedList.Add(openList[0]);

                    foreach (Vector2Int b in bounds.allPositionsWithin)
                    {
                        //Check not self
                        if (b.x == 0 && b.y == 0) continue;

                        //Check if is on cross
                        if (b.x != 0 && b.y != 0) continue;

                        Vector2Int pos = new Vector2Int(openList[0].x + b.x, openList[0].y + b.y);

                        //Check inside bounds
                        if (pos.x < 0 || pos.x >= size || pos.y < 0 || pos.y >= size) continue;

                        //Check is alive
                        if (!cells[pos.x, pos.y].isAlive) continue;

                        //check region not yet associated
                        if (cells[pos.x, pos.y].room != -1) continue;

                        //Check if already visited
                        if (closedList.Contains(pos)) continue;

                        //Check if already set to be visited
                        if (openList.Contains(pos)) continue; //Error

                        openList.Add(new Vector2Int(pos.x, pos.y));

                    }
                    openList.RemoveAt(0);
                    timer++;
                    if (timer == sizeOfRoom)
                    {
                        break;
                    }
                }
                
                currentRoom++;
                roomOfCell.Add(new List<Cell>());

            }
        }
        roomOfCell.RemoveAt(roomOfCell.Count - 1);
    }

    void GenerateCube()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (cells[x, y].isAlive)
                {

                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }


    void OnDrawGizmos()
    { 
        
        if (!isRunning) return;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (cells[x, y].region>-1)
                {

                    Gizmos.color = cells[x, y].region < 0 ? Color.clear : colors[cells[x, y].region % colors.Count];
                    Gizmos.DrawCube(new Vector3(x + 0.5f, y + 0.5f, 0), Vector2.one);
                }
            }
        }
        
    }
   
    void GenerateEnemy()
    {
        BoundsInt boundsEnemy = new BoundsInt(-2, -2, 0, 5, 5, 1);
        for (int i = 0; i < 1000; i++)
        {
            Vector2Int newPosition = GetSpawn();
            bool detectZone = false;
            //Check Neighbours
            foreach (Vector3Int b in boundsEnemy.allPositionsWithin)
            {
                if (newPosition.x + b.x < 0 || newPosition.x + b.x >= size) continue;
                if (newPosition.y + b.y < 0 || newPosition.y + b.y >= size) continue;
                if (!cells[newPosition.x + b.x, newPosition.y + b.y].occuped) continue;

                if (cells[newPosition.x + b.x, newPosition.y + b.y].enemyRegion != -1)
                {
                    detectZone = true;
                }
            }

            if (detectZone)
            {
                continue;
            }

            int index = Random.Range(0, enemies.Length);
            enemies[index].Instantiate(new Vector2(newPosition.x+0.5f, newPosition.y+0.5f), GetRegion(newPosition));
            cells[newPosition.x, newPosition.y].occuped = true;

            foreach (Vector3Int b in boundsEnemy.allPositionsWithin)
            {
                if (newPosition.x + b.x < 0 || newPosition.x + b.x >= size) continue;
                if (newPosition.y + b.y < 0 || newPosition.y + b.y >= size) continue;

                if (cells[newPosition.x + b.x, newPosition.y + b.y].isAlive)
                {
                    cells[newPosition.x + b.x, newPosition.y + b.y].enemyRegion = currentEnemyRegion;
                }
            }

            currentEnemyRegion++;
            break;
        }
    }

    void GenerateChest()
    {
        BoundsInt boundsChest = new BoundsInt(-4, -4, 0, 9, 9, 1);
        for (int i = 0; i < 1000; i++)
        {
            Vector2Int newPosition = GetSpawn();
            bool detectZone = false;
            //Check Neighbours
            foreach (Vector3Int b in boundsChest.allPositionsWithin)
            {
                if (newPosition.x + b.x < 0 || newPosition.x + b.x >= size) continue;
                if (newPosition.y + b.y < 0 || newPosition.y + b.y >= size) continue;
                if (!cells[newPosition.x + b.x, newPosition.y + b.y].occuped) continue;

                    if (cells[newPosition.x + b.x, newPosition.y + b.y].chestRegion != -1)
                {
                    detectZone = true;
                }
            }

            if (detectZone)
            {
                continue;
            }

            Instantiate(chestPrefab, new Vector3(newPosition.x + 0.5f, newPosition.y + 0.5f, 0), Quaternion.identity);
            cells[newPosition.x, newPosition.y].occuped = true;

            foreach (Vector3Int b in boundsChest.allPositionsWithin)
            {
                if (newPosition.x + b.x < 0 || newPosition.x + b.x >= size) continue;
                if (newPosition.y + b.y < 0 || newPosition.y + b.y >= size) continue;

                if (cells[newPosition.x + b.x, newPosition.y + b.y].isAlive)
                {
                    cells[newPosition.x + b.x, newPosition.y + b.y].chestRegion = currentChestRegion;
                }
            }

            currentChestRegion++;
            break;
        }

    }

    void GeneratePenguin()
    {
        BoundsInt boundsPenguin = new BoundsInt(-1, -1, 0, 3, 3, 1);
        for (int i = 0; i < 1000; i++)
        {
            Vector2Int newPosition = GetSpawn();
            bool detectZone = false;
            //Check Neighbours
            foreach (Vector3Int b in boundsPenguin.allPositionsWithin)
            {
                if (newPosition.x + b.x < 0 || newPosition.x + b.x >= size) continue;
                if (newPosition.y + b.y < 0 || newPosition.y + b.y >= size) continue;
                if (cells[newPosition.x + b.x, newPosition.y + b.y].isAlive) continue;
                if (!cells[newPosition.x + b.x, newPosition.y + b.y].occuped)
                {
                    detectZone = true;
                }
            }

            if (detectZone)
            {
                continue;
            }

            Instantiate(penguinPrefab, new Vector3(newPosition.x + 0.5f, newPosition.y + 0.5f, 0), Quaternion.identity);
            cells[newPosition.x, newPosition.y].occuped = true;
            break;
        }

    }

    public Vector2Int GetSpawn()
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);
        for (int i = 0; i < 1000; i++)
        { 
            int neighboursAlive = 0;
            Vector2Int newPosition = new Vector2Int(Random.Range(0, size), Random.Range(0, size));
            //Check Neighbours
            foreach (Vector3Int b in bounds.allPositionsWithin)
            {
                if (newPosition.x + b.x < 0 || newPosition.x + b.x >= size) continue;
                if (newPosition.y + b.y < 0 || newPosition.y + b.y >= size) continue;
                //if (cells[newPosition.x + b.x, newPosition.y + b.y].region != biggestRegion) continue;
                //Debug.Log(cells[newPosition.x + b.x, newPosition.y + b.y].region + " " + biggestRegion);

                if (cells[newPosition.x + b.x, newPosition.y + b.y].isAlive)
                {
                    neighboursAlive++;

                }

                if (neighboursAlive == 9)
                {
                    return newPosition;
                }
            }

        }
        return new Vector2Int(0, 0);
    }
    

    public int GetRegion(Vector2Int position)
    {
        return cells[position.x, position.y].region;
    }

}