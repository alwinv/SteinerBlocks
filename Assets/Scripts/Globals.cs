using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Globals : MonoBehaviour {
    // instance variable so its globals can be referenced from other scripts
    public static Globals Instance { get; private set; }

    // The cube that is currently being edited
    public GameObject SelectedBlock { get; set; }

    // The modal state of the interface
    public bool IsNavigating = false;


    // Use this for initialization
    void Start () {
        Instance = this;

        // load list of .blocks files from file
        fileNames[0] = "ms-appx:///Blocks/CS_1.blocks";
        fileNames[1] = "ms-appx:///Blocks/CS_2.blocks";
        fileNames[2] = "ms-appx:///Blocks/CS_3.blocks";
        fileNames[3] = "ms-appx:///Blocks/CS_4.blocks";
    }

    // Update is called once per frame
    void Update () {
		
	}
}
