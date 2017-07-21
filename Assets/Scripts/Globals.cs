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

    // for cube interactions
    public static float selectedBlockScale = 1.25f;

    // The modal state of the interface
    public static bool CurrentlyNavigating = false;
    public static bool CurrentlyPositioning = false;

    public static float BlockSpacing = 0.0195f;
    public static int NavigationSpeed = 5;
    public static int NavigationToRotationFactor = 180; // one full rotation per 1F of distance navigated

    private float SlideDuration = 5.0f; // how many seconds to show each block arrangement
    private float timeSinceLastSlide = 0.0f;

    // use this for random #s elsewhere in the app
    public System.Random rnd1 = new System.Random(System.DateTime.Now.Millisecond);

    // Use this for initialization
    void Start () {
        Instance = this;

        // load slide show grid
        OnLoadSlideShow();

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

    public void OnLoadSlideShow()
    {
        // load list of .blocks files from file
        string[] fileNames = {
                "001.blocks","002.blocks","003.blocks","004.blocks","005.blocks",
                "013.blocks","014.blocks","015.blocks","016.blocks","017.blocks","018.blocks",
                "081.blocks",
                "084.blocks","085.blocks","086.blocks","087.blocks",
                "091.blocks"};
        SlideShowBlocks_Parent.SendMessage("OnLoadFiles_ForSlideShow", fileNames);
        timeSinceLastSlide = 0.0f;

        //// hide other blocks
        //SlideShowBlocks_Parent.SendMessage("OnHide");
    }
}
