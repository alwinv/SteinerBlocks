using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;

public class BlocksParentBehaviors : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnHide()
    {
        MeshRenderer[] meshRenderers = this.transform.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in meshRenderers)
        {
            if (mr != null)
                mr.enabled = false;
        }
    }

    public void OnShow()
    {
        MeshRenderer[] meshRenderers = this.transform.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in meshRenderers)
        {
            if (mr != null)
                mr.enabled = true;
        }
    }

}
