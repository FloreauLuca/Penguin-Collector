﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bear : Enemy
{
    private Penguin connectedPenguin;

    public Penguin ConnectedPenguin
    {
        get { return connectedPenguin; }
        set { connectedPenguin = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void StandardMove()
    {
        
    }
}
