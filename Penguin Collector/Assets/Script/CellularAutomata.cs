using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CellularAutomata : MonoBehaviour
{
    [SerializeField] [Range(0, 250)] int size = 50;
    [SerializeField] [Range(0, 100)] int iteration = 10;
    [SerializeField] private TileBase tile;
    [SerializeField] private Tilemap tilemap;
    


    struct Cell
    {
        public bool isAlive;
        public bool futureState;
        public int region;
    }

    Cell[,] cells;

    bool isRunning;

    public void OnDrawGizmos()
    {
        if (!isRunning) { return; }
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (cells[x, y].isAlive)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawCube(new Vector3(x, y), Vector3.one);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Create array
        cells = new Cell[size, size];

        //Fille array by random
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                 float f = Random.Range(0f, 1f);
                 cells[x, y].isAlive = f > 0.5f;
            }
        }
        StartCoroutine(WorldGeneration());

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

    IEnumerator WorldGeneration()
    {
        //isRunning = true;

        for (int i = 0; i < iteration; i++)
        {
            Cellular();
            
            yield return new WaitForSeconds(0.1f);
        }
        

        //Cut Cube
        //CutCube();

        //Generate cube
        GenerateCube();

        GameManager.Instance.MapLoaded();
    }

    void GenerateCube()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (cells[x, y].isAlive)
                {

                    tilemap.SetTile(new Vector3Int(x,y,0), tile );
                     
                }
            }
        }
    }

    void CutCube()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = size - 5; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    cells[x, y].isAlive = true;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Vector2 GetSpawn()
    {
        while (true)
        {
            int newX = Random.Range(0, size);
            int newY = Random.Range(0, size);
            if (cells[newX, newY].isAlive)
            {
                Debug.Log(newX +" "+ newY);
                return new Vector2(newX, newY);
            }
        }
    }
}
