using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Globals : MonoBehaviour {
    // instance variable so its globals can be referenced from other scripts
    public static Globals Instance { get; private set; }

    public GameObject SlideShowBlocks_Grid;

    // The cube that is currently being edited
    public GameObject SelectedBlock { get; set; }

    // The modal state of the interface
    public static bool CurrentlyNavigating = false;

    public static float BlockSpacing = 0.02f;
    public static int NavigationSpeed = 5;
    public static int NavigationToRotationFactor = 180; // one full rotation per 1F of distance navigated

    // Use this for initialization
    void Start () {
        Instance = this;

        // load list of .blocks files from file
        var fileNames = new string[4];
        fileNames[0] = "ms-appx:///Blocks/CS_1.blocks";
        fileNames[1] = "ms-appx:///Blocks/CS_2.blocks";
        fileNames[2] = "ms-appx:///Blocks/CS_3.blocks";
        fileNames[3] = "ms-appx:///Blocks/CS_4.blocks";
        SlideShowBlocks_Grid.SendMessage("OnLoadFiles_ForSlideShow", fileNames);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
