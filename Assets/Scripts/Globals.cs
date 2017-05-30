using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Globals : MonoBehaviour {
    // instance variable so its globals can be referenced from other scripts
    public static Globals Instance { get; private set; }

    // The cube that is currently being edited
    public GameObject SelectedBlock { get; set; }

    public static float BlockSpacing = 0.02f;

    // Use this for initialization
    void Start () {
        Instance = this;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
