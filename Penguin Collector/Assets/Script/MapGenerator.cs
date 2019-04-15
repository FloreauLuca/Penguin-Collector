using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public struct Cell
{
    // Generation du terrain
    public bool isAlive;
    public bool futureState;
    public Vector2Int position;
    //Generation des regions
    public int region;
    public bool edge;
    public bool bridge;
    // Generations des salles
    public int room;
    // Generation des enemies
    public bool occuped;
}


public class Room
{
    public List<Cell> cells;
    public List<Cell> edgeCells;
    public bool island;
    public bool occuped;
    public List<Room> neighboursRooms;

    public Room()
    {
        cells = new List<Cell>();
        edgeCells = new List<Cell>();
        neighboursRooms = new List<Room>();

    }
}

public class Region
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

public class MapGenerator : MonoBehaviour
{
    // Generation du terrain
    [SerializeField] [Range(0, 250)] private int mapSize = 50;
    public int MapSize => mapSize;
    [SerializeField] [Range(0, 100)] private int cellularIteration = 10;
    [SerializeField] private int maxSizeOfRoom = 40;
    [SerializeField] private int minSizeOfRoom = 15;
    [SerializeField] private int nbBear = 10;
    [SerializeField] private int nbSeal = 10;

    //[SerializeField] private int nbChest = 0;
    [SerializeField] private int nbPenguin = 0;

    // Affichage du terrain
    [SerializeField] private TileBase tileIce;
    [SerializeField] private TileBase tileWater;
    [SerializeField] private Tilemap tilemapIce;
    [SerializeField] private Tilemap tilemapWater;

    // Generation des enemies
    [SerializeField] private SO_Enemy[] enemies;
    //[SerializeField] private GameObject chestPrefab;
    [SerializeField] private GameObject penguinPrefab;

    // POint de départ personnage
    [SerializeField] private GameObject boatPrefab;


    [SerializeField] private int seed;
    


    private Cell[,] mapOfCells;
    public Cell[,] MapOfCells => mapOfCells;
    
    private List<Region> regionList;
    private List<Room> roomList;
    private List<Room> priorityRoomList;

    private int currentRegion = 0;
    private int biggestRegion = 0;

    private int currentRoom = 0;

    private List<GameObject> walrusList;
    private List<GameObject> bearList;
    private List<GameObject> penguinList;

    // debug
    private List<Color> colors;
    private List<Color> colorsDebug;
    private List<Vector2> debug;
    bool isRunning; // Affichage Gizmos
    private enum GizmoState
    {
        CELLULES,
        REGION,
        BRIDGES,
        ROOM,
        PRIORITYROOM,
        ENEMY,
        MAPNAV
    }
    private GizmoState gizmoState;

    void Start()
    {
        //Initialisation de la seed
        if (seed == 0)
        {
            seed = Random.Range(0, 1000000000);
        }
        Random.InitState(seed);
        Debug.Log(seed);

        debug = new List<Vector2>();
        
        mapOfCells = new Cell[mapSize, mapSize];

        regionList = new List<Region>();
        roomList = new List<Room>();

        colors = new List<Color> {
            new Color(1, 1, 1, 1f),
            new Color(1, 0, 0, 1f),
            new Color(0, 0, 1, 1f),
            new Color(0, 1, 0, 1f),
            new Color(1, 0, 1, 1f),
            new Color(0, 1, 1, 1f),
            new Color(1, 1, 0, 1f),
            new Color(0.5f, 1, 1, 1f),
            new Color(1, 0.5f, 0, 1f),
            new Color(0, 0, 0.5f, 1f),
            new Color(0, 0.5f, 0, 1f),
            new Color(0.5f, 0, 1, 1f),
            new Color(0, 0.5f, 1, 1f),
            new Color(1, 1, 0.5f, 1f),
            new Color(0.5f, 0.5f, 1, 1f),
            new Color(0.5f, 0.5f, 0, 1f),
            new Color(0, 0.5f, 0.5f, 1f),
            new Color(0, 0.5f, 0.5f, 1f),
            new Color(0.5f, 0, 0.5f, 1f),
            new Color(0.5f, 0.5f, 1, 1f),
            new Color(1, 0.5f, 0.5f, 1f),
            new Color(0.5f, 0.5f, 0.5f, 1f),
        };

        colorsDebug = new List<Color> {
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

        penguinList = new List<GameObject>();
        bearList = new List<GameObject>();
        walrusList = new List<GameObject>();


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
        
        //Initialisation du tableau
        Init();

        // Generation du Cellular Automata
        for (int i = 0; i < cellularIteration; i++)
        {
            Cellular();
        }
        yield return null;
        GameManager.Instance.UiManagerScript.DisplayLoad(10);
        gizmoState = GizmoState.CELLULES;

        // Definition des régions
        GenerateRegion();
        yield return null;
        GameManager.Instance.UiManagerScript.DisplayLoad(20);
        gizmoState = GizmoState.REGION;

        // Connection des région prochent
        ConnectClosestRegions();
        yield return null;
        GameManager.Instance.UiManagerScript.DisplayLoad(30);
        gizmoState = GizmoState.BRIDGES;

        // Définition et organisations des salles
        GenerateRoom();
        yield return null;
        GameManager.Instance.UiManagerScript.DisplayLoad(40);
        gizmoState = GizmoState.ROOM;
        yield return null;
        gizmoState = GizmoState.PRIORITYROOM;
        yield return null;

        // Affichage du terrain
        GenerateCube();
        yield return null;
        GameManager.Instance.UiManagerScript.DisplayLoad(50);

        // Positionnement du joueur
        GameManager.Instance.SpawnPlayer();
        yield return null;
        GameManager.Instance.UiManagerScript.DisplayLoad(60);

        // Générations des ours polaires
        for (int i = 0; i < nbBear; i++)
        {
            GenerateBear();
        }
        yield return null;
        GameManager.Instance.UiManagerScript.DisplayLoad(70);

        // Generations des morses
        for (int i = 0; i < nbSeal; i++)
        {
            GenerateWalrus();
        }
        yield return null;
        GameManager.Instance.UiManagerScript.DisplayLoad(80);

        /*                    Posibilité d'amélioration du jeu
        for (int i = 0; i < nbChest; i++)
        {
            GenerateChest();
        }
        yield return new WaitForSeconds(10);

        for (int i = 0; i < nbPenguin; i++)
        {
            GeneratePenguin();
        }
        yield return new WaitForSeconds(10);
        */

        gizmoState = GizmoState.ENEMY;

        yield return null;
        gizmoState = GizmoState.MAPNAV;
        // Création des nodes pour le pathfinding
        GameManager.Instance.MapNav.Initialize(mapOfCells, roomList);
        yield return null;
        GameManager.Instance.UiManagerScript.DisplayLoad(100);

        // Lancement du jeu
        GameManager.Instance.MapLoaded();
    }

    //Initialisation du tableau
    void Init()
    {
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                mapOfCells[x, y] = new Cell();

                mapOfCells[x, y].region = -1;
                mapOfCells[x, y].room = -1;
                mapOfCells[x, y].position = new Vector2Int(x, y);

                float isAlive = Random.Range(0f, 1f);

                mapOfCells[x, y].isAlive = isAlive < 0.5f;
            }
        }
    }


    // Generation du Cellular Automata
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



    // Definition des régions
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


    // Trouver les régions adjacentent
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
            if (regionList[indexRegionA].cells.Count < minSizeOfRoom) continue;
            for (int indexRegionB = 0; indexRegionB < regionList.Count; indexRegionB++)
            {
                if (regionList[indexRegionB].cells.Count < minSizeOfRoom) continue;
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

    // Generation de chemins entre les régions adjacents
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
                    mapOfCells[currentPosition.x + b.x, currentPosition.y + b.y].bridge = true;
                    mapOfCells[currentPosition.x + b.x, currentPosition.y + b.y].room = -1;
                }
            }
        }

        regionList[bestRegionA].connectedRegions.Add(regionList[bestRegionB]);
        regionList[bestRegionB].connectedRegions.Add(regionList[bestRegionA]);
    }


    // Définition des salles
    void GenerateRoom()
    {
        // Gestion des salles isolées
        for (int indexRegion = 0; indexRegion < regionList.Count; indexRegion++)
        {
            if ((regionList[indexRegion].cells.Count < minSizeOfRoom || regionList[indexRegion].cells.Count > maxSizeOfRoom)|| regionList[indexRegion].connectedRegions.Count != 1) continue;

            roomList.Add(new Room());
            for (int cellIndex = 0; cellIndex < regionList[indexRegion].cells.Count; cellIndex++)
            {
                Cell newCell = regionList[indexRegion].cells[cellIndex];
                newCell.room = currentRoom;
                mapOfCells[newCell.position.x, newCell.position.y] = newCell;
                roomList[currentRoom].cells.Add(newCell);
                
            }
            roomList[currentRoom].island = true;
            Debug.Log(GetSpawn(roomList[currentRoom]));
            if (GetSpawn(roomList[currentRoom]) == Vector2Int.zero)
            {
                for (int indexcell = 0; indexcell < roomList[currentRoom].cells.Count; indexcell++)
                {
                    Cell cell = roomList[currentRoom].cells[indexcell];
                    int neighboursAlive = 0;
                    BoundsInt newBounds = new BoundsInt(-1, -1, 0, 3, 3, 1);
                    foreach (Vector2Int b in newBounds.allPositionsWithin)
                    {
                        if (cell.position.x + b.x < 0 || cell.position.x + b.x >= mapSize) break;
                        if (cell.position.y + b.y < 0 || cell.position.y + b.y >= mapSize) break;
                        if (mapOfCells[cell.position.x + b.x, cell.position.y + b.y].region != cell.region && mapOfCells[cell.position.x + b.x, cell.position.y + b.y].region >= 0) break;
                        if (!mapOfCells[cell.position.x + b.x, cell.position.y + b.y].isAlive || mapOfCells[cell.position.x + b.x, cell.position.y + b.y].region < 0)
                        {
                            neighboursAlive++;
                        }
                        else if (regionList[mapOfCells[cell.position.x + b.x, cell.position.y + b.y].region].cells.Count >= minSizeOfRoom)
                        {
                            neighboursAlive++;
                        }
                    }

                    Debug.Log(neighboursAlive);
                    if (neighboursAlive == 9)
                    {
                        BoundsInt createBounds = new BoundsInt(-1, -1, 0, 3, 3, 1);
                        foreach (Vector2Int b in createBounds.allPositionsWithin)
                        {
                            if (mapOfCells[cell.position.x + b.x, cell.position.y + b.y].region != cell.region || !mapOfCells[cell.position.x + b.x, cell.position.y + b.y].isAlive)
                            {
                                mapOfCells[cell.position.x + b.x, cell.position.y + b.y].isAlive = true;
                                mapOfCells[cell.position.x + b.x, cell.position.y + b.y].room = currentRoom;
                                mapOfCells[cell.position.x + b.x, cell.position.y + b.y].position = new Vector2Int(cell.position.x + b.x, cell.position.y + b.y);
                                mapOfCells[cell.position.x + b.x, cell.position.y + b.y].region = mapOfCells[cell.position.x, cell.position.y].region;
                                roomList[currentRoom].cells.Add(mapOfCells[cell.position.x + b.x, cell.position.y + b.y]);
                                regionList[indexRegion].cells.Add(mapOfCells[cell.position.x + b.x, cell.position.y + b.y]);
                                break;
                            }
                        }
                    }

                    if (GetSpawn(roomList[currentRoom]) != Vector2Int.zero)
                    {

                        break;
                    }
                }

                if (GetSpawn(roomList[currentRoom]) != Vector2Int.zero)
                {
                    break;
                }
            }

            currentRoom++;
        }


        // FlowField des grandes régions
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);
        roomList.Add(new Room());
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                if (!mapOfCells[x, y].isAlive) continue;
                if (mapOfCells[x, y].region >= 0)
                {
                   
                    if (regionList[mapOfCells[x, y].region].cells.Count < minSizeOfRoom)
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



        // Suppression des salles trop petites et définitions des voisins
        OrganizeRoom();

        // Calcule des bordures
        FindEdge();

        // Organisations des salles par rapport à leurs isolations
        PriorityCalculation();
    }
    
    // Suppression des salles trop petites et définitions des voisins
    void OrganizeRoom()
    {
        // suppresion des salles trop petites
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

                if (newRoom >= 0)
                {
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

        //Definition des voisins

        foreach (Room room in roomList)
        {
            foreach (Cell cell in room.cells)
            {
                BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

                foreach (Vector3Int b in bounds.allPositionsWithin)
                {
                    if (cell.position.x + b.x < 0 || cell.position.x + b.x >= mapSize) continue;
                    if (cell.position.y + b.y < 0 || cell.position.y + b.y >= mapSize) continue;
                    if (!mapOfCells[cell.position.x + b.x, cell.position.y + b.y].isAlive) continue;
                    if (mapOfCells[cell.position.x + b.x, cell.position.y + b.y].room >= 0)
                    if (mapOfCells[cell.position.x + b.x, cell.position.y + b.y].room != cell.room && !room.neighboursRooms.Contains(roomList[mapOfCells[cell.position.x + b.x, cell.position.y + b.y].room]))
                    {
                        room.neighboursRooms.Add(roomList[mapOfCells[cell.position.x + b.x, cell.position.y + b.y].room]);
                    }
                }
            }
        }
        
    }


    // Organisations des salles par rapport à leurs isolations
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
                            if (room.neighboursRooms.Count < priorityRoomList[i].neighboursRooms.Count)
                            {
                                priorityRoomList.Insert(i, room);
                                inserted = true;
                                break;
                            } else if (room.neighboursRooms.Count == priorityRoomList[i].neighboursRooms.Count && room.edgeCells.Count > priorityRoomList[i].edgeCells.Count)
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
    }

    // Affichage du terrain
    void GenerateCube()
    {
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                if (mapOfCells[x, y].isAlive)
                {
                    tilemapIce.SetTile(new Vector3Int(x, y, 0), tileIce);
                }
                else
                {
                    tilemapWater.SetTile(new Vector3Int(x, y, 0), tileWater);
                }
            }
        }
    }

    public void SetPlayerRoom(int indexRoom)
    {
        roomList[indexRoom].occuped = true;
    }

    // Générations des ours polaires
    void GenerateBear()
    {
        Room spawningRoom = new Room();

        for (int i = 0; i < priorityRoomList.Count; i++)
        {
            if (!priorityRoomList[i].occuped)
            {
                bool occuped = false;
                foreach (Room neighbours in priorityRoomList[i].neighboursRooms)
                {
                    if (neighbours.occuped)
                    {
                        occuped = true;
                    }
                }

                if (!occuped)
                {
                    spawningRoom = priorityRoomList[i];
                    break;
                }
            }
        }
        
        if (spawningRoom.cells.Count > 0)
        {
            Vector2Int newPosition = GetSpawn(spawningRoom);
            if (newPosition != Vector2Int.zero)
            {
                GameObject penguin = Instantiate(penguinPrefab, new Vector3(newPosition.x + 0.5f, newPosition.y + 0.5f, 0), Quaternion.identity);
                penguinList.Add(penguin);
                if (nbPenguin > 1)
                {
                    for (int i = 1; i < nbPenguin; i++)
                    {
                        Instantiate(penguinPrefab, new Vector3(newPosition.x + 0.5f, newPosition.y + 0.5f, 0), Quaternion.identity);
                    }
                }

                GameObject bear = enemies[1].Instantiate(new Vector2(newPosition.x + 1 + 0.5f, newPosition.y + 0.5f), GetRegion(newPosition));
                bear.GetComponent<Bear>().ConnectedPenguin = penguin.GetComponent<Penguin>();
                bearList.Add(bear);
            }
            mapOfCells[newPosition.x, newPosition.y].occuped = true;
            spawningRoom.occuped = true;
        }
    }

    // Generations des morses
    void GenerateWalrus()
    {
        Room spawningRoom = new Room();

        for (int i = 0; i < priorityRoomList.Count; i++)
        {
            if (!priorityRoomList[i].occuped)
            {
                spawningRoom = priorityRoomList[i];
                break;
            }
        }

        if (spawningRoom.cells.Count > 0)
        {
            Vector2Int newPosition = GetSpawn(spawningRoom);
            if (newPosition != Vector2Int.zero)
            {
                GameObject walrus = enemies[0].Instantiate(new Vector2(newPosition.x + 0.5f, newPosition.y + 0.5f), GetRegion(newPosition));
                walrusList.Add(walrus);
            }

            spawningRoom.occuped = true;
        }
    }
    /*                                                                                                    Posibilité d'amélioration du jeu
    void GenerateChest()
    {
        
        BoundsInt boundsChest = new BoundsInt(-2, -2, 0, 5, 5, 1);
        for (int i = 0; i < 1000; i++)
        {
            Vector2Int newPosition = Vector2Int.zero;// GetSpawn();
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
            Vector2Int newPosition = Vector2Int.zero;//GetSpawn();
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
    */


    // Trouve un emplacement libre dans une salle
    public Vector2Int GetSpawn(Room room)
    {
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        for (int i = 0; i < 1000; i++)
        {
            int neighboursAlive = 0;
            Vector2Int newPosition = room.cells[Random.Range(0, room.cells.Count)].position;
            //Check Neighbours
            foreach (Vector3Int b in bounds.allPositionsWithin)
            {
                if (newPosition.x + b.x < 0 || newPosition.x + b.x >= mapSize) continue;
                if (newPosition.y + b.y < 0 || newPosition.y + b.y >= mapSize) continue;
                if (mapOfCells[newPosition.x + b.x, newPosition.y + b.y].room != mapOfCells[newPosition.x, newPosition.y].room) continue;

                if (mapOfCells[newPosition.x + b.x, newPosition.y + b.y].isAlive && !mapOfCells[newPosition.x + b.x, newPosition.y + b.y].occuped)
                {
                    neighboursAlive++;
                }
            }

            if (neighboursAlive == 9)
            {
                return newPosition;
            }

        }
        Debug.LogError("Non spawn found" + room.cells[0].position);
        return Vector2Int.zero;
    }

    // trouve uneb bordurelibre pour le joueur
    public Vector2Int GetPlayerSpawn()
    {
        roomList[0].occuped = true;
        Vector2 boatPosition = new Vector2();
        Vector2Int playerPosition = new Vector2Int();
        foreach (var room in roomList)
        {
            foreach (Cell edgeCell in room.edgeCells)
            {
                BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

                int consecutiveEdge = 0;
                foreach (Vector3Int b in bounds.allPositionsWithin)
                {
                    if ((edgeCell.position.x + b.x < 0 || edgeCell.position.x + b.x >= mapSize) || (edgeCell.position.y + b.y < 0 || edgeCell.position.y + b.y >= mapSize))
                    {
                        consecutiveEdge++;
                        
                    }
                    else if (!mapOfCells[edgeCell.position.x + b.x, edgeCell.position.y + b.y].isAlive)
                    {
                        consecutiveEdge++;
                    }
                    else
                    {
                        consecutiveEdge = 0;
                    }
                    if (consecutiveEdge >= 2 && (b.x == 0 || b.y == 0))
                    {
                        boatPosition = new Vector2(edgeCell.position.x + b.x +0.5f, edgeCell.position.y + b.y + 0.5f);
                    }
                    if (consecutiveEdge >= 3)
                    {
                        playerPosition = edgeCell.position;
                        Instantiate(boatPrefab, boatPosition, Quaternion.identity);
                        return playerPosition;
                    }
                }
            }
        }
        return Vector2Int.zero;
    }


    public int GetRegion(Vector2Int position)
    {
        return mapOfCells[position.x, position.y].region;
    }

    public int GetRoomIndex(Vector2Int position)
    {
        return mapOfCells[position.x, position.y].room;
    }

    public Room GetRoom(Vector2Int position)
    {
        if (GetRoomIndex(position) >= 0)
        {
            return roomList[mapOfCells[position.x, position.y].room];
        }
        else
        {
            Debug.Log("Room not found  " + GetRoomIndex(position) + "; " + position);
            return null;
        }
    }



    void OnDrawGizmos()
    {
        //for (int x = 0; x < mapSize; x++)
        //{
        //    for (int y = 0; y < mapSize; y++)
        //    {
        //        if (mapOfCells[x, y].room > -1)
        //        {

        //            Gizmos.color = mapOfCells[x, y].room < 0 ? Color.clear : colorsDebug[mapOfCells[x, y].room % colors.Count];
        //            Gizmos.DrawCube(new Vector3(x + 0.5f, y + 0.5f, 0), Vector2.one);
        //        }
        //    }
        //}
        return;
        if (!isRunning) return;

        switch (gizmoState)
        {
            case GizmoState.CELLULES:
                for (int x = 0; x < mapSize; x++)
                {
                    for (int y = 0; y < mapSize; y++)
                    {
                        if (!mapOfCells[x, y].isAlive)
                        {

                            Gizmos.color = Color.black;
                        }
                        else
                        {
                            Gizmos.color= Color.white;
                        }

                        Gizmos.DrawCube(new Vector3(x + 0.5f, y + 0.5f, 0), Vector2.one);
                    }
                }
                break;
            case GizmoState.REGION:

                for (int x = 0; x < mapSize; x++)
                {
                    for (int y = 0; y < mapSize; y++)
                    {
                        if (mapOfCells[x, y].region > -1)
                        {

                            Gizmos.color = mapOfCells[x, y].region < 0 ? Color.clear : colors[mapOfCells[x, y].region % colors.Count];
                            Gizmos.DrawCube(new Vector3(x + 0.5f, y + 0.5f, 0), Vector2.one);
                        }
                    }
                }
                break;
            case GizmoState.BRIDGES:
                for (int x = 0; x < mapSize; x++)
                {
                    for (int y = 0; y < mapSize; y++)
                    {
                        if (mapOfCells[x, y].bridge)
                        {

                            Gizmos.color = Color.red;
                            Gizmos.DrawCube(new Vector3(x + 0.5f, y + 0.5f, 0), Vector2.one);
                        }
                        else if (mapOfCells[x, y].edge)
                        {
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawCube(new Vector3(x + 0.5f, y + 0.5f, 0), Vector2.one);
                        }
                    }
                }
                break;
            case GizmoState.ROOM:
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
                break;
            case GizmoState.PRIORITYROOM:
                for (float i = 0; i < priorityRoomList.Count; i++)
                {
                    foreach (Cell cell in priorityRoomList[(int)i].cells)
                    {
                        Gizmos.color = Color.black * ((priorityRoomList.Count - i) / priorityRoomList.Count);
                        Gizmos.DrawCube(new Vector3(cell.position.x + 0.5f, cell.position.y + 0.5f, 0), Vector2.one);
                    }
                }
                break;
            case GizmoState.ENEMY:
                for (float i = 0; i < priorityRoomList.Count; i++)
                {
                    foreach (Cell cell in priorityRoomList[(int)i].cells)
                    {
                        Gizmos.color = Color.black*((priorityRoomList.Count-i)/priorityRoomList.Count);
                        Gizmos.DrawCube(new Vector3(cell.position.x + 0.5f, cell.position.y + 0.5f, 0), Vector2.one);
                    }
                }
                foreach (GameObject bear in bearList)
                {
                    Gizmos.DrawIcon(new Vector3(bear.transform.position.x, bear.transform.position.y, -5), "BearGizmos.png", false);
                        
                }
                foreach (GameObject walrus in walrusList)
                {
                    Gizmos.DrawIcon(new Vector3(walrus.transform.position.x, walrus.transform.position.y, -1), "WalrusGizmos.png", false);

                }
                foreach (GameObject penguin in penguinList)
                {
                    Gizmos.DrawIcon(new Vector3(penguin.transform.position.x, penguin.transform.position.y, -1), "PenguinGizmos.png",false);

                }
                break;
            case GizmoState.MAPNAV:
                break;
        }
        
    }
}