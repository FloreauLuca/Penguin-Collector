using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class CellularAutomata : MonoBehaviour
{
    [SerializeField] [Range(0, 250)] private int mapSize = 50;
    [SerializeField] [Range(0, 100)] private int cellularIteration = 10;
    [SerializeField] private int maxSizeOfRoom = 10;
    [SerializeField] private int minSizeOfRoom = 10;
    [SerializeField] private int nbEnemy = 10;
    [SerializeField] private int nbChest = 10;
    [SerializeField] private int nbPenguin = 10;
    [SerializeField] private TileBase tile;
    [SerializeField] private SO_Enemy[] enemies;
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private GameObject penguinPrefab;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private int seed;


    bool isRunning; // Display Gizmos

    struct Cell
    {
        public bool isAlive;
        public bool futureState;

        public Vector2Int position;
        public int region;
        public int room;
        public bool edge;

        public bool occuped;
        public int enemyRegion;
        public int chestRegion;
    }

    class Room
    {
        public List<Cell> cells;
        public List<Cell> edgeCells;
        public bool island;
        public bool occuped;

        public Room()
        {
            cells = new List<Cell>();
            edgeCells = new List<Cell>();

        }
    }

    class Region
    {
        public List<Cell> cells;
        public List<Region> connectedRegions;
        public List<Cell> edgeCells;

        public Region()
        {
            cells = new List<Cell>();
            connectedRegions = new List<Region>();
            edgeCells = new List<Cell>();
        }
    }

    private Cell[,] mapOfCells;

    //private List<List<Cell>> regionOfCell;
    //private List<List<Cell>> roomOfCell;
    private List<Region> regionList;
    private List<Room> roomList;
    private List<Room> priorityRoomList;

    private int currentRegion = 0;
    private int biggestRegion = 0;
    private int currentEnemyRegion = 0;
    private int currentChestRegion = 0;
    private int currentRoom = 0;

    private List<Color> colors;

    private List<Vector2> debug;

    void Start()
    {
        //Initialize seed
        if (seed == 0)
        {
            seed = Random.Range(0, 10000);
        }
        Random.InitState(seed);

        debug = new List<Vector2>();

        //Create array
        mapOfCells = new Cell[mapSize, mapSize];

        regionList = new List<Region>();
        roomList = new List<Room>();

        colors = new List<Color> {
            new Color(1, 1, 1, 0.2f),
            new Color(1, 0, 0, 0.2f),
            new Color(0, 0, 1, 0.2f),
            new Color(0, 1, 0, 0.2f),
            new Color(1, 0, 1, 0.2f),
            new Color(0, 1, 1, 0.2f),
            new Color(1, 1, 0, 0.2f),
            new Color(0.5f, 1, 1, 0.2f),
            new Color(1, 0.5f, 0, 0.2f),
            new Color(0, 0, 0.5f, 0.2f),
            new Color(0, 0.5f, 0, 0.2f),
            new Color(0.5f, 0, 1, 0.2f),
            new Color(0, 0.5f, 1, 0.2f),
            new Color(1, 1, 0.5f, 0.2f),
            new Color(0.5f, 0.5f, 1, 0.2f),
            new Color(0.5f, 0.5f, 0, 0.2f),
            new Color(0, 0.5f, 0.5f, 0.2f),
            new Color(0, 0.5f, 0.5f, 0.2f),
            new Color(0.5f, 0, 0.5f, 0.2f),
            new Color(0.5f, 0.5f, 1, 0.2f),
            new Color(1, 0.5f, 0.5f, 0.2f),
            new Color(0.5f, 0.5f, 0.5f, 0.2f),
        };

        //Fill array by random
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                float f = Random.Range(0f, 1f);
                mapOfCells[x, y].isAlive = f > 0.5f;
            }
        }

        StartCoroutine(WorldGeneration());

    }

    IEnumerator WorldGeneration()
    {
        isRunning = true;

        Init();

        for (int i = 0; i < cellularIteration; i++)
        {
            Cellular();
        }

        yield return null;
        

        GenerateRegion();
        yield return null;
        

        ConnectClosestRegions();
        yield return null;

        GenerateRoom();
        yield return null;

        GenerateCube();
        yield return null;

        
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
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                mapOfCells[x, y] = new Cell();

                mapOfCells[x, y].region = -1;
                mapOfCells[x, y].room = -1;
                mapOfCells[x, y].enemyRegion = -1;
                mapOfCells[x, y].chestRegion = -1;
                mapOfCells[x, y].position = new Vector2Int(x, y);

                float isAlive = Random.Range(0f, 1f);

                mapOfCells[x, y].isAlive = isAlive < 0.5f;
            }
        }
    }


    void Cellular()
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                int neighboursAlive = 0;

                //Check Neighbours
                foreach (Vector3Int b in bounds.allPositionsWithin)
                {
                    if (b.x == 0 && b.y == 0) continue;
                    if (x + b.x < 0 || x + b.x >= mapSize) continue;
                    if (y + b.y < 0 || y + b.y >= mapSize) continue;

                    if (mapOfCells[x + b.x, y + b.y].isAlive)
                    {
                        neighboursAlive++;
                    }
                }

                //Apply rules
                if (!mapOfCells[x, y].isAlive && neighboursAlive >= 5)
                {
                    mapOfCells[x, y].futureState = true;
                }
                else if (mapOfCells[x, y].isAlive && (neighboursAlive >= 4 || neighboursAlive == 1))
                {
                    mapOfCells[x, y].futureState = true;
                }
                else
                {
                    mapOfCells[x, y].futureState = false;
                }
            }
        }

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                mapOfCells[x, y].isAlive = mapOfCells[x, y].futureState;
            }
        }
    }



    void GenerateRegion()
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);
        regionList.Add(new Region());
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                if (!mapOfCells[x, y].isAlive) continue;
                if (mapOfCells[x, y].region != -1) continue;

                List<Vector2Int> openList = new List<Vector2Int>();
                List<Vector2Int> closedList = new List<Vector2Int>();

                openList.Add(new Vector2Int(x, y));

                while (openList.Count > 0)
                {
                    mapOfCells[openList[0].x, openList[0].y].region = currentRegion;
                    regionList[currentRegion].cells.Add(mapOfCells[openList[0].x, openList[0].y]);
                    closedList.Add(openList[0]);

                    foreach (Vector2Int b in bounds.allPositionsWithin)
                    {
                        //Check not self
                        if (b.x == 0 && b.y == 0) continue;

                        //Check if is on cross
                        if (b.x != 0 && b.y != 0) continue;

                        Vector2Int pos = new Vector2Int(openList[0].x + b.x, openList[0].y + b.y);

                        //Check inside bounds
                        if (pos.x < 0 || pos.x >= mapSize || pos.y < 0 || pos.y >= mapSize) continue;

                        //Check is alive
                        if (!mapOfCells[pos.x, pos.y].isAlive) continue;

                        //check region not yet associated
                        if (mapOfCells[pos.x, pos.y].region != -1) continue;

                        //Check if already visited
                        if (closedList.Contains(pos)) continue;

                        //Check if already set to be visited
                        if (openList.Contains(pos)) continue;

                        openList.Add(new Vector2Int(pos.x, pos.y));
                        
                    }
                    openList.RemoveAt(0);
                }

                
                if (regionList[biggestRegion].cells.Count < regionList[currentRegion].cells.Count)
                {
                    biggestRegion = currentRegion;
                }
                currentRegion++;
                regionList.Add(new Region());
            }
        }
        
        regionList.RemoveAt(regionList.Count-1); // remove the last empty region

        FindEdge();
    }

    void FindEdge()
    {
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                if (mapOfCells[x, y].isAlive)
                {
                    mapOfCells[x, y].edge = false;

                    if (regionList.Count > 0)
                    {
                        if (mapOfCells[x, y].region == -1) continue;
                        if (!regionList[mapOfCells[x, y].region].edgeCells.Contains(mapOfCells[x, y]))
                        {
                            regionList[mapOfCells[x, y].region].edgeCells.Remove(mapOfCells[x, y]);
                        }
                    }

                    if (roomList.Count > 0)
                    {
                        if (mapOfCells[x, y].room == -1) continue;
                        if (!roomList[mapOfCells[x, y].room].edgeCells.Contains(mapOfCells[x, y]))
                        {
                            roomList[mapOfCells[x, y].room].edgeCells.Remove(mapOfCells[x, y]);
                        }
                    }


                    BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

                    foreach (Vector3Int b in bounds.allPositionsWithin)
                    {
                        if (x + b.x < 0 || x + b.x >= mapSize) continue;
                        if (y + b.y < 0 || y + b.y >= mapSize) continue;
                        if (!mapOfCells[x + b.x, y + b.y].isAlive)
                        {
                            mapOfCells[x, y].edge = true;

                            if (regionList.Count > 0)
                            {
                                if (mapOfCells[x, y].region == -1) continue;
                                if (!regionList[mapOfCells[x, y].region].edgeCells.Contains(mapOfCells[x, y]))
                                {
                                    regionList[mapOfCells[x, y].region].edgeCells.Add(mapOfCells[x, y]);
                                }
                            }

                            if (roomList.Count > 0)
                            {
                                if (mapOfCells[x, y].room == -1) continue;
                                if (!roomList[mapOfCells[x, y].room].edgeCells.Contains(mapOfCells[x, y]))
                                {
                                    roomList[mapOfCells[x, y].room].edgeCells.Add(mapOfCells[x, y]);
                                }
                            }
                            
                        }
                    }
                }
            }
        }
    }

    void ConnectClosestRegions()
    {
        int minDistance = 0;
        Cell bestTileA = new Cell();
        Cell bestTileB = new Cell();
        int bestRegionA = -1;
        int bestRegionB = -1;
        bool possibleConnectionFound = false;

        for (int indexRegionA = 0; indexRegionA < regionList.Count; indexRegionA++)
        {
            possibleConnectionFound = false;
            if (regionList[indexRegionA].cells.Count < 10) continue;
            for (int indexRegionB = 0; indexRegionB < regionList.Count; indexRegionB++)
            {
                if (regionList[indexRegionB].cells.Count < 10) continue;
                if (indexRegionA == indexRegionB) continue;
                if (regionList[indexRegionA].connectedRegions.Contains(regionList[indexRegionB])) break;

                for (int cellIndexA = 0; cellIndexA < regionList[indexRegionA].edgeCells.Count; cellIndexA++)
                {
                    for (int cellIndexB = 0; cellIndexB < regionList[indexRegionB].edgeCells.Count; cellIndexB++)
                    {
                        Cell cellA = regionList[indexRegionA].edgeCells[cellIndexA];
                        Cell cellB = regionList[indexRegionB].edgeCells[cellIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(cellA.position.x - cellB.position.x, 2) + Mathf.Pow(cellA.position.y - cellB.position.y, 2));

                        if (distanceBetweenRooms < minDistance || !possibleConnectionFound)
                        {
                            minDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = cellA;
                            bestTileB = cellB;
                            bestRegionA = indexRegionA;
                            bestRegionB = indexRegionB;
                        }
                    }
                }
            }

            if (possibleConnectionFound)
            {
                GenerateBridge(bestRegionA, bestRegionB, bestTileA, bestTileB);
            }
        }
    }

    void GenerateBridge(int bestRegionA, int bestRegionB, Cell bestEdgeA, Cell bestEdgeB)
    { 
        Vector2Int startPosition = bestEdgeA.position;
        Vector2Int currentPosition = startPosition;
        Vector2Int targetPosition = bestEdgeB.position;

        Vector2 direction = targetPosition - currentPosition;

        if (direction.x == 0)
        {
            direction.x = 100000f;
        }

        if (direction.y == 0)
        {
            direction.y = 100000f;
        }


        int timer = 0;
        while (currentPosition != targetPosition && timer < 1000)
        {
            timer++;
            if (Mathf.Abs(currentPosition.x - targetPosition.x) / (float) Mathf.Abs(direction.x) >
                Mathf.Abs(currentPosition.y - targetPosition.y) / (float) Mathf.Abs(direction.y))
            {
                currentPosition.x -= Math.Sign(currentPosition.x - targetPosition.x);
            }
            else
            {
                currentPosition.y -= Math.Sign(currentPosition.y - targetPosition.y);
            }

            if (!mapOfCells[currentPosition.x, currentPosition.y].isAlive)
            {
                BoundsInt bounds = new BoundsInt(-1, -1, 0, 2, 2, 1);

                foreach (Vector3Int b in bounds.allPositionsWithin)
                {
                    if (currentPosition.x + b.x < 0 || currentPosition.x + b.x >= mapSize) continue;
                    if (currentPosition.y + b.y < 0 || currentPosition.y + b.y >= mapSize) continue;
                    if (mapOfCells[currentPosition.x + b.x, currentPosition.y + b.y].isAlive) continue;
                    mapOfCells[currentPosition.x + b.x, currentPosition.y + b.y].isAlive = true;
                    mapOfCells[currentPosition.x + b.x, currentPosition.y + b.y].room = -2;
                }
            }
        }

        regionList[bestRegionA].connectedRegions.Add(regionList[bestRegionB]);
        regionList[bestRegionB].connectedRegions.Add(regionList[bestRegionA]);
    }


    void GenerateRoom()
    {
        foreach (Region region in regionList)
        {
            if ((region.cells.Count < 10 || region.cells.Count > 100)||region.connectedRegions.Count != 1) continue;

            roomList.Add(new Room());
            for (int cellIndex = 0; cellIndex < region.cells.Count; cellIndex++)
            {
                Cell newCell = region.cells[cellIndex];
                newCell.room = currentRoom;
                mapOfCells[newCell.position.x, newCell.position.y] = newCell;
                roomList[currentRoom].cells.Add(newCell);
                roomList[currentRoom].island = true;
            }
            currentRoom++;
        }


        
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);
        roomList.Add(new Room());
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                if (!mapOfCells[x, y].isAlive) continue;
                if (mapOfCells[x, y].region >= 0)
                {
                   
                    if (regionList[mapOfCells[x, y].region].cells.Count < 10)
                    {
                        continue;
                    }
                }
                if (mapOfCells[x, y].room != -1) continue;

                List<Vector2Int> openList = new List<Vector2Int>();
                List<Vector2Int> closedList = new List<Vector2Int>();

                openList.Add(new Vector2Int(x, y));
                int timer = 0;
                while (openList.Count > 0)
                {
                    mapOfCells[openList[0].x, openList[0].y].room = currentRoom;
                    roomList[currentRoom].cells.Add(mapOfCells[openList[0].x, openList[0].y]);
                    closedList.Add(openList[0]);
                    
                    timer++;
                    if (timer == maxSizeOfRoom)
                    {
                        break;
                    }
                    foreach (Vector2Int b in bounds.allPositionsWithin)
                    {
                        //Check not self
                        if (b.x == 0 && b.y == 0) continue;

                        //Check if is on cross
                        if (b.x != 0 && b.y != 0) continue;

                        Vector2Int pos = new Vector2Int(openList[0].x + b.x, openList[0].y + b.y);

                        //Check inside bounds
                        if (pos.x < 0 || pos.x >= mapSize || pos.y < 0 || pos.y >= mapSize) continue;

                        //Check is alive
                        if (!mapOfCells[pos.x, pos.y].isAlive) continue;

                        //check region not yet associated
                        if (mapOfCells[pos.x, pos.y].room != -1) continue;

                        //Check if already visited
                        if (closedList.Contains(pos)) continue;

                        //Check if already set to be visited
                        if (openList.Contains(pos)) continue; //Error

                        openList.Add(new Vector2Int(pos.x, pos.y));

                    }
                    openList.RemoveAt(0);
                }
                
                currentRoom++;
                roomList.Add(new Room());

            }
        }
        roomList.RemoveAt(roomList.Count - 1);

        OrganizeRoom();
        FindEdge();
        PriorityCalculation();
    }

    void OrganizeRoom()
    {
        List<Room> toDestroy = new List<Room>();
        foreach (Room room in roomList)
        {
            if (room.cells.Count < minSizeOfRoom && !room.island)
            {
                int newRoom = -1;
                foreach (Cell cell in room.cells)
                {
                    BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

                    foreach (Vector3Int b in bounds.allPositionsWithin)
                    {
                        if (cell.position.x + b.x < 0 || cell.position.x + b.x >= mapSize) continue;
                        if (cell.position.y + b.y < 0 || cell.position.y + b.y >= mapSize) continue;
                        if (!mapOfCells[cell.position.x + b.x, cell.position.y + b.y].isAlive) continue;
                        if (mapOfCells[cell.position.x + b.x, cell.position.y + b.y].region != cell.region) continue;
                        
                        if (mapOfCells[cell.position.x + b.x, cell.position.y + b.y].room != cell.room)
                        {
                            newRoom = mapOfCells[cell.position.x + b.x, cell.position.y + b.y].room;
                            break;
                        }
                    }

                    if (newRoom >= 0)
                    {
                        break;
                    }
                }

                for (int i = 0; i < room.cells.Count; i++)
                {
                    Cell newCell = room.cells[i];
                    newCell.room = newRoom;
                    room.cells[i] = newCell;
                    roomList[newRoom].cells.Add(newCell);
                    mapOfCells[newCell.position.x, newCell.position.y] = newCell;
                }
            }
        }
        
    }

    void PriorityCalculation()
    {
        priorityRoomList = new List<Room>();
        foreach (Room room in roomList)
        {
            if (room.edgeCells.Count > 0)
            {
                bool inserted = false;
                if (priorityRoomList.Count > 0)
                {
                    for (int i = 0; i < priorityRoomList.Count; i++)
                    {
                        if ((room.island && priorityRoomList[i].island) ||
                            (!room.island && !priorityRoomList[i].island))
                        {
                            if (room.edgeCells.Count > priorityRoomList[i].edgeCells.Count)
                            {
                                priorityRoomList.Insert(i, room);
                                inserted = true;
                                break;
                            }
                        }
                        else if (room.island && !priorityRoomList[i].island)
                        {
                            priorityRoomList.Insert(i, room);
                            inserted = true;
                            break;
                        }
                    }

                    if (!inserted)
                    {
                        priorityRoomList.Add(room);
                    }

                }
                else
                {
                    priorityRoomList.Add(room);
                }
            }

        }

        foreach (Room room1 in priorityRoomList)
        {
            Debug.Log(room1.edgeCells.Count + "   " + room1.island + room1.cells[0].position);
        }
    }

    void GenerateCube()
    {
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                if (mapOfCells[x, y].isAlive)
                {

                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
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
                if (newPosition.x + b.x < 0 || newPosition.x + b.x >= mapSize) continue;
                if (newPosition.y + b.y < 0 || newPosition.y + b.y >= mapSize) continue;
                if (!mapOfCells[newPosition.x + b.x, newPosition.y + b.y].occuped) continue;

                if (mapOfCells[newPosition.x + b.x, newPosition.y + b.y].enemyRegion != -1)
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
            mapOfCells[newPosition.x, newPosition.y].occuped = true;

            foreach (Vector3Int b in boundsEnemy.allPositionsWithin)
            {
                if (newPosition.x + b.x < 0 || newPosition.x + b.x >= mapSize) continue;
                if (newPosition.y + b.y < 0 || newPosition.y + b.y >= mapSize) continue;

                if (mapOfCells[newPosition.x + b.x, newPosition.y + b.y].isAlive)
                {
                    mapOfCells[newPosition.x + b.x, newPosition.y + b.y].enemyRegion = currentEnemyRegion;
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
                if (newPosition.x + b.x < 0 || newPosition.x + b.x >= mapSize) continue;
                if (newPosition.y + b.y < 0 || newPosition.y + b.y >= mapSize) continue;
                if (!mapOfCells[newPosition.x + b.x, newPosition.y + b.y].occuped) continue;

                    if (mapOfCells[newPosition.x + b.x, newPosition.y + b.y].chestRegion != -1)
                {
                    detectZone = true;
                }
            }

            if (detectZone)
            {
                continue;
            }

            Instantiate(chestPrefab, new Vector3(newPosition.x + 0.5f, newPosition.y + 0.5f, 0), Quaternion.identity);
            mapOfCells[newPosition.x, newPosition.y].occuped = true;

            foreach (Vector3Int b in boundsChest.allPositionsWithin)
            {
                if (newPosition.x + b.x < 0 || newPosition.x + b.x >= mapSize) continue;
                if (newPosition.y + b.y < 0 || newPosition.y + b.y >= mapSize) continue;

                if (mapOfCells[newPosition.x + b.x, newPosition.y + b.y].isAlive)
                {
                    mapOfCells[newPosition.x + b.x, newPosition.y + b.y].chestRegion = currentChestRegion;
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
                if (newPosition.x + b.x < 0 || newPosition.x + b.x >= mapSize) continue;
                if (newPosition.y + b.y < 0 || newPosition.y + b.y >= mapSize) continue;
                if (mapOfCells[newPosition.x + b.x, newPosition.y + b.y].isAlive) continue;
                if (!mapOfCells[newPosition.x + b.x, newPosition.y + b.y].occuped)
                {
                    detectZone = true;
                }
            }

            if (detectZone)
            {
                continue;
            }

            Instantiate(penguinPrefab, new Vector3(newPosition.x + 0.5f, newPosition.y + 0.5f, 0), Quaternion.identity);
            mapOfCells[newPosition.x, newPosition.y].occuped = true;
            break;
        }

    }

    public Vector2Int GetSpawn()
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);
        for (int i = 0; i < 1000; i++)
        { 
            int neighboursAlive = 0;
            Vector2Int newPosition = new Vector2Int(Random.Range(0, mapSize), Random.Range(0, mapSize));
            //Check Neighbours
            foreach (Vector3Int b in bounds.allPositionsWithin)
            {
                if (newPosition.x + b.x < 0 || newPosition.x + b.x >= mapSize) continue;
                if (newPosition.y + b.y < 0 || newPosition.y + b.y >= mapSize) continue;
                //if (mapOfCells[newPosition.x + b.x, newPosition.y + b.y].region != biggestRegion) continue;
                //Debug.Log(mapOfCells[newPosition.x + b.x, newPosition.y + b.y].region + " " + biggestRegion);

                if (mapOfCells[newPosition.x + b.x, newPosition.y + b.y].isAlive)
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
        return mapOfCells[position.x, position.y].region;
    }


    void OnDrawGizmos()
    {

        if (!isRunning) return;

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                if (mapOfCells[x, y].room > -1)
                {

                    Gizmos.color = mapOfCells[x, y].room < 0 ? Color.clear : colors[mapOfCells[x, y].room % colors.Count];
                    Gizmos.DrawCube(new Vector3(x + 0.5f, y + 0.5f, 0), Vector2.one);
                }
            }
        }

        //Gizmos.DrawIcon(new Vector3(11 + 0.5f, 42+0.5f, 0), "penguoin3.png");
        /*
        foreach (Room room in roomList)
        {
            foreach (Cell edgeCell in room.edgeCells)
            {

                Gizmos.DrawIcon(new Vector3(edgeCell.position.x + 0.5f, edgeCell.position.y + 0.5f, 0), "hammeritem.png");
            }
        }*/

        foreach (var point in debug)
        {
            Gizmos.DrawIcon(point, "hammeritem.png");
        }

    }
}