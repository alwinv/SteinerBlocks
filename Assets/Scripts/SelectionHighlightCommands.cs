using UnityEngine;
using System.Collections;

public class SelectionHighlightCommands : MonoBehaviour {
    Vector3 originalPosition;
    private MeshRenderer[] meshRenderers_focus;
    private MeshRenderer[] meshRenderers_selection;
    Vector3 originalLocalScale;
    Quaternion originalLocalRotation;
    bool selectedState = false;

    // Use this for initialization
    void Start () {
        // Save the original local position of the cube when the app starts.
        originalPosition = this.transform.localPosition;
        originalLocalScale = this.transform.localScale;
        originalLocalRotation = this.transform.localRotation;
        meshRenderers_focus = this.transform.FindChild("FocusHighlight").GetComponentsInChildren<MeshRenderer>();
        meshRenderers_selection = this.transform.FindChild("SelectionHighlight").GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in meshRenderers_focus)
        {
            if(mr != null)
                mr.enabled = false;
        }
        foreach (MeshRenderer mr in meshRenderers_selection)
        {
            if (mr != null)
                mr.enabled = false;
        }
    }

    // Called by GazeGestureManager to pop the focus block out
    void OnFocus(GameObject FocusedBlock)
    {
        // show the hilight 
        foreach (MeshRenderer mr in meshRenderers_focus)
        {
            if (mr != null)
                mr.enabled = true;
        }

        //move up off the matrix plane and to the location of the selected block
        if (FocusedBlock != null)
        {
            // place the highlight in the context of the focused block grid (parent, scaling)
            this.transform.parent = FocusedBlock.transform.parent;
            this.transform.localScale = this.originalLocalScale;
            this.transform.localRotation = Quaternion.Euler(0, 0, 0);
            this.transform.localPosition = new Vector3(
                FocusedBlock.transform.localPosition.x,
                FocusedBlock.transform.localPosition.y + 0.5f * Globals.BlockSpacing,
                FocusedBlock.transform.localPosition.z);
        }
    }

    // Called by GazeGestureManager to pop the focus block back in
    void OnFocusLost()
    {
        // hide the highlight
        if (meshRenderers_focus != null)
        {
            foreach (MeshRenderer mr in meshRenderers_focus)
            {
                if (mr != null)
                    mr.enabled = false;
            }
        }
        this.transform.localPosition = originalPosition;
    }

    void OnSelect()
    {
        // hide the focus
        if (meshRenderers_focus != null)
        {
            foreach (MeshRenderer mr in meshRenderers_focus)
            {
                if (mr != null)
                    mr.enabled = false;
            }
        }
        // show the selection
        if (meshRenderers_selection != null)
        {
            foreach (MeshRenderer mr in meshRenderers_selection)
            {
                if (mr != null)
                    mr.enabled = true;
            }
        }
        originalPosition = this.transform.localPosition;
        this.transform.localScale = new Vector3(
            originalLocalScale.x * 2,
            originalLocalScale.y * 2,
            originalLocalScale.z * 2);
        this.transform.localRotation = Quaternion.Euler(0, 45, 0);
        selectedState = true;
    }

    void OnUnSelect()
    {
        // show the focus
        if (meshRenderers_focus != null)
        {
            foreach (MeshRenderer mr in meshRenderers_focus)
            {
                if (mr != null)
                    mr.enabled = true;
            }
        }
        // hide the selection
        if (meshRenderers_selection != null)
        {
            foreach (MeshRenderer mr in meshRenderers_selection)
            {
                if (mr != null)
                    mr.enabled = false;
            }
        }
        this.transform.localRotation = Quaternion.Euler(0, 0, 0);
        this.transform.localScale = originalLocalScale;
        this.transform.localPosition = originalPosition;
        selectedState = false;
    }

    // Update is called once per frame
    void Update () {
        if(selectedState)
        {
            this.transform.localPosition = new Vector3(
                Globals.Instance.SelectedBlock.transform.localPosition.x,
                Globals.Instance.SelectedBlock.transform.localPosition.y,
                Globals.Instance.SelectedBlock.transform.localPosition.z);

            // clone the camera, as a child of the block world transform
            var tempTransform = new GameObject().transform;
            tempTransform.SetParent(Globals.Instance.SelectedBlock.transform.parent);
            tempTransform.rotation = Camera.main.transform.rotation;
            tempTransform.position = Camera.main.transform.position;

            //// rotate cloned camera to look at the selected block
            tempTransform.LookAt(Globals.Instance.SelectedBlock.transform, Camera.main.transform.up);

            // find closest 90 deg local rotation angles for the cloned camera 
            Vector3 SnapAngles = new Vector3(0, 0, 0);
            SnapAngles.x = Mathf.RoundToInt(tempTransform.localEulerAngles.x / 90) * 90;
            SnapAngles.y = Mathf.RoundToInt(tempTransform.localEulerAngles.y / 90) * 90;
            SnapAngles.z = Mathf.RoundToInt(tempTransform.localEulerAngles.z / 90) * 90;

            // rotate the transform to the nearest 90 deg snap angles in local coordinates
            tempTransform.localEulerAngles = SnapAngles;

            this.transform.rotation = tempTransform.rotation;
            this.transform.Rotate(tempTransform.right, -90, Space.World);
            this.transform.position = Globals.Instance.SelectedBlock.transform.position;
            this.transform.Translate(0, 0, -2f * Globals.BlockSpacing, tempTransform);
        }
    }
}
