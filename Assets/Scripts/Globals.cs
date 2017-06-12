using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Globals : MonoBehaviour {
    // instance variable so its globals can be referenced from other scripts
    public static Globals Instance { get; private set; }

    public GameObject SlideShowBlocks_Parent;
    public GameObject LocalBlocks_Parent;
    public GameObject SelectionHighlight;

    // The cube that is currently being edited
    public GameObject SelectedBlock { get; set; }
    public GameObject FocusedObject { get; set; }

    // The modal state of the interface
    public static bool CurrentlyNavigating = false;

    public static float BlockSpacing = 0.02f;
    public static int NavigationSpeed = 5;
    public static int NavigationToRotationFactor = 180; // one full rotation per 1F of distance navigated

    private float SlideDuration = 5.0f; // how many seconds to show each block arrangement
    private float timeSinceLastSlide = 0.0f;

    // Use this for initialization
    void Start () {
        Instance = this;

        // load list of .blocks files from file
        var fileNames = new string[4];
        fileNames[0] = "CS_1.blocks";
        fileNames[1] = "CS_2.blocks";
        fileNames[2] = "CS_3.blocks";
        fileNames[3] = "CS_4.blocks";
        SlideShowBlocks_Parent.SendMessage("OnLoadFiles_ForSlideShow", fileNames);
        timeSinceLastSlide = 0.0f;

        // load local block grid
        LocalBlocks_Parent.SendMessage("OnLoadFile_ForLocal", "my.blocks");
    }

    // Update is called once per frame
    void Update () {
        if(timeSinceLastSlide + Time.deltaTime >= SlideDuration)
        {
            timeSinceLastSlide = 0.0f;
            SlideShowBlocks_Parent.SendMessage("OnLoadNextBlocks_ForSlideShow");
        }
        else
        {
            timeSinceLastSlide += Time.deltaTime;
        }
    }
}
