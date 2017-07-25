using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Globals : MonoBehaviour {
    // instance variable so its globals can be referenced from other scripts
    public static Globals Instance { get; private set; }

    public GameObject SlideShowBlocks_Parent;
    public GameObject LocalBlocks_Parent;
    public GameObject SharedBlocks_Parent;
    public GameObject SelectionHighlight;

    // The cube that is currently being edited
    public GameObject SelectedBlock { get; set; }
    public GameObject FocusedObject { get; set; }
    public enum SelectionType
    {
        Block, Row, Column, All, Custom
    }
    public SelectionType SelectionMode { get; set; }
    public struct size
    {
        public int width, height;
        public size(int Width, int Height)
        {
            width = Width;
            height = Height;
        }
    };

    // for cube interactions
    public static float selectedBlockScale = 1.25f;

    // The modal state of the interface
    public static bool CurrentlyNavigating = false;
    public static Vector3 CurrentlyNavigatingDirection = Vector3.zero;
    public static bool CurrentlyPositioning = false;

    public static float BlockSpacing = 0.0195f;
    public static int NavigationSpeed = 5;
    public static int NavigationToRotationFactor = 180; // one full rotation per 1F of distance navigated

    private float SlideDuration = 5.0f; // how many seconds to show each block arrangement
    private float timeSinceLastSlide = 0.0f;
    public bool SlideShowRunning = false;

    // use this for random #s elsewhere in the app
    public System.Random rnd1 = new System.Random(System.DateTime.Now.Millisecond);

    // Use this for initialization
    IEnumerator Start () {
        Instance = this;

        // load slide show grid hidden & not running
        OnLoadSlideShow();
        SlideShowBlocks_Parent.SendMessage("OnHide");
        SlideShowRunning = false;

        // load local block grid
        LocalBlocks_Parent.SendMessage("OnLoadFile_ForLocal", "my.blocks");
        LocalBlocks_Parent.SendMessage("OnHide");

        // start intro sequence
        yield return StartCoroutine("IntroSequence");
    }

    // Update is called once per frame
    void Update () {
        if(SlideShowRunning)
        {
            if (timeSinceLastSlide + Time.deltaTime >= SlideDuration)
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

    IEnumerator IntroSequence()
    {
        GameObject introGameObject = GameObject.Find("Intro");
        AudioSource[] audioSources = introGameObject.GetComponents<AudioSource>();
        if (introGameObject != null)
        {
            yield return new WaitForSeconds(3);
            // welcome
            if (audioSources != null && audioSources.Length > 1)
            {
                introGameObject.GetComponents<AudioSource>()[0].volume = 0.75f;
                introGameObject.GetComponents<AudioSource>()[1].Play();
                yield return new WaitForSeconds(12);
            }

            // slideshow of examples
            SlideShowBlocks_Parent.SendMessage("OnShow");
            SlideShowRunning = true;
            TextMesh promptText = GameObject.Find("Intro/PromptText").GetComponent<TextMesh>();
            if (promptText != null)
            {
                promptText.text = "These uniquely patterned blocks " +
                    "\ncan be used as artistic pixels to" +
                    "\ncreate patterns, or works of art. " +
                    "\nTake a look at some of the 100 " +
                    "\ncreated by Curtis as part of the" +
                    "\n1,000 Blocks exhibit.";
            }
            if (audioSources != null && audioSources.Length > 2)
            {
                introGameObject.GetComponents<AudioSource>()[2].Play();
                yield return new WaitForSeconds(30);
            }

            // invite them to create their own
            promptText = GameObject.Find("Intro/PromptText").GetComponent<TextMesh>();
            if (promptText != null)
            {
                promptText.text = "Would you like to try it out for " +
                    "\nyourself? Say \"Show My Blocks\", " +
                    "\nto begin making your own " +
                    "\npatterns.";
            }
            if (audioSources != null && audioSources.Length > 3)
            {
                introGameObject.GetComponents<AudioSource>()[3].Play();
                yield return new WaitForSeconds(10);

                // return volume of looping audio to normal
                introGameObject.GetComponents<AudioSource>()[0].volume = 1.0f;
            }
        }
    }

    public IEnumerator OnIntroToEditing()
    {
        yield return StartCoroutine("IntroToEditing");
    }

    private IEnumerator IntroToEditing()
    {
        GameObject introGameObject = GameObject.Find("Intro");
        AudioSource[] audioSources = introGameObject.GetComponents<AudioSource>();
        if (introGameObject != null)
        {
            // explain how to edit a block
            TextMesh promptText = GameObject.Find("Intro/PromptText").GetComponent<TextMesh>();
            if (promptText != null)
            {
                promptText.text = "Air tap, or say \"Select\", to " +
                    "\nedit the block you're gazing at." +
                    "\n\nRotate the block by doing a Pinch " +
                    "\n& Drag gesture." +
                    "\n\nYou can also rotate the block with " +
                    "\nvoice commands. Say \"left\", " +
                    "\n\"right\", \"up\", \"down\", " +
                    "\n\"turn\", or \"flip\".";
            }
            if (audioSources != null && audioSources.Length > 4)
            {
                introGameObject.GetComponents<AudioSource>()[0].volume = 0.75f;
                introGameObject.GetComponents<AudioSource>()[4].Play();
                yield return new WaitForSeconds(65);
                introGameObject.GetComponents<AudioSource>()[0].volume = 1.0f;
            }
        }
    }

    public void OnLoadSlideShow()
    {
        // load list of .blocks files from file
        string[] fileNames = {
                "001.blocks","002.blocks","003.blocks","004.blocks",
                "005.blocks",
                "006.blocks",
                "013.blocks","014.blocks","015.blocks","016.blocks","017.blocks","018.blocks",
                "081.blocks",
                "084.blocks","085.blocks","086.blocks","087.blocks",
                "091.blocks"};
        SlideShowBlocks_Parent.SendMessage("OnLoadFiles_ForSlideShow", fileNames);
        timeSinceLastSlide = 0.0f;
    }
}
