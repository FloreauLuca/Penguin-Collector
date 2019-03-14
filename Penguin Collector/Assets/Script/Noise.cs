using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise : MonoBehaviour
{
    private struct Cell
    {
        public int x;
        public int y;
        public Color color;
        public bool land;
    }

    


    // Start is called before the first frame update
    void Start()
    {
        Cell[,] grid = new Cell[10, 10];
        foreach (var cell in grid)
        {
            //cell.color = Color.Lerp(Color.white, Color.black, Convert.ToSingle(cell.y) / grid.Rank);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
