﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;

public class BlockBehaviors : MonoBehaviour, IFocusable, IInputClickHandler
{
    Vector3 originalLocalPosition;
    Vector3 AnimationTargetPosition;
    Vector3 AnimationTargetScale;
    Quaternion originalLocalRotation;
    Quaternion AnimationTargetRotation;
    Vector3 originalLocalScale;

    // navigation reference variables
    Vector3 navStart_headPosition;
    Quaternion navStart_gazeRotation;
    Transform navStart_axes_transform;
    Quaternion navStart_block_rotation;

    enum AnimationStates
    {
        off = 0,
        on = 1
    }
    AnimationStates animState = AnimationStates.off;

    // for cube animations
    float movementSpeed = 0.2f;
    float scaleSpeed = 100f;
    float rotationSpeed = 360;

    // Use this for initialization
    void Start () {
        // Grab the original local position, rotation and scale of the cube when the app starts.
        originalLocalPosition = this.transform.localPosition;
        originalLocalRotation = this.transform.localRotation;
        originalLocalScale = this.transform.localScale;

        AnimationTargetPosition = originalLocalPosition;
        AnimationTargetRotation = originalLocalRotation;
        AnimationTargetScale = originalLocalScale;

        // initialize the axes transform to be children of the block world
        navStart_axes_transform = new GameObject().transform;
        navStart_axes_transform.SetParent(this.transform.parent);

        // assign a random texture to this block
        Renderer renderer = this.GetComponent<Renderer>();
        int rndNum = Globals.Instance.rnd1.Next(26);
        Texture newTexture = Resources.Load<Texture>("Textures/block_" + rndNum.ToString("D3"));
        Texture newBump = Resources.Load<Texture>("Textures/block_" + rndNum.ToString("D3") + "_gray");
        renderer.material.SetTexture("_MainTex", newTexture);
        renderer.material.SetTexture("_BumpMap", newBump);
    }

    // Update is called once per frame
    void Update () {
        if (animState == AnimationStates.on)
        {
            // animate movement
            var stepToMove = Time.deltaTime * movementSpeed;
            this.transform.localPosition = Vector3.MoveTowards(transform.localPosition, AnimationTargetPosition, stepToMove);

            // animate rotation
            var stepToRotate = Time.deltaTime * rotationSpeed;
            this.transform.localRotation = Quaternion.RotateTowards(transform.localRotation, AnimationTargetRotation, stepToRotate);

            // animate scale
            var stepToScale = Time.deltaTime * scaleSpeed;
            this.transform.localScale = Vector3.MoveTowards(transform.localScale, AnimationTargetScale, stepToScale);

            // turn animation off when it reaches the target
            if (this.transform.localRotation == AnimationTargetRotation 
                && this.transform.localPosition == AnimationTargetPosition
                && this.transform.localScale == AnimationTargetScale)
            {
                animState = AnimationStates.off;
            }
        }
    }

    private bool ImEditable()
    {
        var blockparentscript = (BlocksParentBehaviors) this.transform.parent.parent.gameObject.GetComponent(typeof(BlocksParentBehaviors));
        if (blockparentscript.IsEditable)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsNotSelectedBySomeoneElse()
    {
        if (this.transform.localPosition.y != (originalLocalPosition.y + 4 * Globals.BlockSpacing))
            return true;
        else
            return false;
    }

    #region userinput
    // 
    // handlers for user input
    // 

    public void OnFocusEnter()
    {
        // set stabilization plane to 0,0 of parent
        Transform gridPlane = this.transform.parent;
        UnityEngine.VR.WSA.HolographicSettings.SetFocusPointForFrame(gridPlane.position, gridPlane.up);

        if (!Globals.CurrentlyNavigating 
            && Globals.Instance.SelectedBlock == null
            && IsNotSelectedBySomeoneElse()
            && ImEditable())
        {
            // show selection highlight around the block
            Globals.Instance.SelectionHighlight.SendMessage("OnFocus", this.gameObject);
            Globals.Instance.FocusedObject = this.gameObject;

            // move the block up off the matrix plane
            AnimationTargetPosition = new Vector3(
                this.transform.localPosition.x,
                originalLocalPosition.y + 0.2f * Globals.BlockSpacing,
                this.transform.localPosition.z);
            movementSpeed = 2f;
            animState = AnimationStates.on;
        }
    }

    public void OnFocusExit()
    {
        if(!Globals.CurrentlyNavigating
            && Globals.Instance.SelectedBlock == null
            && IsNotSelectedBySomeoneElse()
            && ImEditable())
        {
            // hide selection highlight
            Globals.Instance.SelectionHighlight.SendMessage("OnFocusLost", this.gameObject);
            Globals.Instance.FocusedObject = null;

            // move the block up back down to the matrix plane
            AnimationTargetPosition = originalLocalPosition;
            movementSpeed = 0.2f;
            animState = AnimationStates.on;
        }
    }

    public void OnSelect(Globals.SelectionType SelectionMode)
    {
        // todo: does this need to be conditioned to only happen when not dragging?
        // !this.gameObject.transform.parent.parent.gameObject.GetComponent<HandDraggable>().IsDraggingEnabled

        if (ImEditable())
        {
            // disable parent's hand draggable
            this.gameObject.transform.parent.parent.gameObject.GetComponent<HandDraggable>().enabled = false;

            Globals.Instance.SelectedBlock = this.gameObject;
            Globals.Instance.SelectionMode = SelectionMode;

            // move the block up off the matrix plane
            AnimationTargetPosition = new Vector3(
                this.transform.localPosition.x,
                this.transform.localPosition.y + 4 * Globals.BlockSpacing,
                this.transform.localPosition.z);
            AnimationTargetScale = new Vector3(
                originalLocalScale.x * Globals.selectedBlockScale,
                originalLocalScale.y * Globals.selectedBlockScale,
                originalLocalScale.z * Globals.selectedBlockScale);
            animState = AnimationStates.on;
            movementSpeed = 2f;

            // show selection highlight around the block
            Globals.Instance.SelectionHighlight.SendMessage("OnSelect");

            // play sound effect
            this.GetComponents<AudioSource>()[0].Play();
        }
    }

    public void OnUnselect()
    {
        Globals.Instance.SelectedBlock = null;

        // move the block back to the original position in the matrix plane
        AnimationTargetPosition = originalLocalPosition;
        AnimationTargetScale = originalLocalScale;
        animState = AnimationStates.on;
        movementSpeed = 2f;

        // update file
        this.SendMessageUpwards("OnSaveFile_ForLocal");

        // hide selection highlight
        Globals.Instance.SelectionHighlight.SendMessage("OnUnSelect");

        // play sound effect
        this.GetComponents<AudioSource>()[0].Play();

        // re-enable hand draggable
        // todo: undo hack below - make it so dragging doesn't start on parent until a motion delta - to prevent conflicting with block selection
//        this.gameObject.transform.parent.parent.gameObject.GetComponent<HandDraggable>().enabled = true;
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if(Globals.Instance.SelectedBlock == this.gameObject)
        {
            this.OnUnselect();
        }
        else
        {
            if(Globals.Instance.SelectedBlock != null)
                Globals.Instance.SelectedBlock.SendMessage("OnUnselect");
            else
               this.OnSelect(Globals.SelectionType.Block);
        }
    }

    #endregion

    #region navigation
    //
    // handlers for navigation 
    //

    void OnRotateRelativeInit()
    {
        // calculate and store transform for "front" of block relative to user's head position
        initFrontFace_transform();
    }

    void initFrontFace_transform()
    {
        // initialize the frame of reference for the rotations 
        // (e.g. how does up/down/etc navigation translate to block rotation)
        navStart_headPosition = Camera.main.transform.position;
        navStart_gazeRotation = Camera.main.transform.rotation;

        // clone the camera, as a child of the block world transform
        var tempTransform = new GameObject().transform;
        tempTransform.SetParent(Globals.Instance.SelectedBlock.transform.parent);
        tempTransform.rotation = navStart_gazeRotation;
        tempTransform.position = navStart_headPosition;

        //// rotate cloned camera to look at the selected block
        tempTransform.LookAt(Globals.Instance.SelectedBlock.transform, Camera.main.transform.up);

        // find closest 90 deg local rotation angles for the cloned camera 
        Vector3 SnapAngles;
        SnapAngles.x = Mathf.RoundToInt(tempTransform.localEulerAngles.x / 90) * 90;
        SnapAngles.y = Mathf.RoundToInt(tempTransform.localEulerAngles.y / 90) * 90;
        SnapAngles.z = Mathf.RoundToInt(tempTransform.localEulerAngles.z / 90) * 90;

        // rotate the transform to the nearest 90 deg snap angles
        tempTransform.localEulerAngles = SnapAngles;

        // save off the resulting transform which defines the up, right, forward 
        // directions to use in block rotation navigation
        navStart_axes_transform.rotation = tempTransform.rotation;
        navStart_axes_transform.position = tempTransform.position;

        // save off the current block's starting rotation
        // to use in absolute rotations
        navStart_block_rotation = this.transform.rotation;
    }

    void OnRotateRelative(Vector3 relativeAmount)
    {
        rotateRelative(relativeAmount);
    }

    void OnRotateRelativeSnap(Vector3 relativeAmount)
    {
        // find closest 90 deg local rotation angles for this block
        Vector3 SnapAngles = new Vector3(0, 0, 0);
        SnapAngles.x = Mathf.RoundToInt(this.transform.localEulerAngles.x / 90) * 90;
        SnapAngles.y = Mathf.RoundToInt(this.transform.localEulerAngles.y / 90) * 90;
        SnapAngles.z = Mathf.RoundToInt(this.transform.localEulerAngles.z / 90) * 90;

        // save the quaternion for animations
        this.AnimationTargetRotation.eulerAngles = SnapAngles;
        animState = AnimationStates.on;
    }

    void rotateRelative(Vector3 relativeAmount)
    {
        animState = AnimationStates.off;
        // rotate the cube around the world x, y and z axes
        // amount to rotate is based on the relative amount vector against the original rotation of the block
        // x,y,z navitagion is scaled to rotations by NavigationToRotationFactor
        // x,y,z navigation is constrained to 1 axis

        this.transform.rotation = navStart_block_rotation;
        if (Mathf.Abs(relativeAmount.y) > 0)
        {
            this.transform.Rotate(
                navStart_axes_transform.right,
                relativeAmount.y * Globals.NavigationToRotationFactor,
                Space.World);
        }
        if (Mathf.Abs(relativeAmount.x) > 0)
        {
            this.transform.Rotate(
                navStart_axes_transform.up,
                relativeAmount.x * Globals.NavigationToRotationFactor,
                Space.World);
        }
        if (Mathf.Abs(relativeAmount.z) > 0)
        {
            this.transform.Rotate(
                navStart_axes_transform.forward,
                -relativeAmount.z * Globals.NavigationToRotationFactor,
                Space.World);
        }

    }

    void OnRotateAbsolute(Quaternion localRotation)
    {
        // save the quaternion for animations
        this.AnimationTargetRotation = localRotation;
        animState = AnimationStates.on;
    }
    #endregion

    #region voice
    // Called by SpeechManager when the user says the "Rotate" command
    void OnRotate()
    {
        // calculate and store transform for "front" of block
        initFrontFace_transform();

        // rotate the cube around the Block World's y axis by 90 degrees.
        AnimationTargetRotation = newRotation(this.transform, navStart_axes_transform.forward, -90);
        animState = AnimationStates.on;
    }

    // Called by SpeechManager when the user says the "Rotate" command
    void OnFlip()
    {
        // calculate and store transform for "front" of block
        initFrontFace_transform();

        // rotate the cube around the Block World's y axis by 90 degrees.
        AnimationTargetRotation = newRotation(this.transform, navStart_axes_transform.forward, 180);
        animState = AnimationStates.on;
    }

    // Called by SpeechManager when the user says the "Turn Left" command
    void OnTurnLeft()
    {
        // calculate and store transform for "front" of block
        initFrontFace_transform();

        // rotate the cube around the Block World's y axis by 90 degrees.
        AnimationTargetRotation = newRotation(this.transform, navStart_axes_transform.up, 90);
        animState = AnimationStates.on;
    }

    // Called by SpeechManager when the user says the "Turn Right" command
    void OnTurnRight()
    {
        // calculate and store transform for "front" of block
        initFrontFace_transform();

        // rotate the cube around the Block World's y axis by -90 degrees.
        AnimationTargetRotation = newRotation(this.transform, navStart_axes_transform.up, -90);
        animState = AnimationStates.on;
    }

    // Called by SpeechManager when the user says the "Turn Up" command
    void OnTurnUp()
    {
        // calculate and store transform for "front" of block
        initFrontFace_transform();

        // rotate the cube around the Block World's x axis by 90 degrees.
        AnimationTargetRotation = newRotation(this.transform, navStart_axes_transform.right, 90);
        animState = AnimationStates.on;
    }

    // Called by SpeechManager when the user says the "Turn Down" command
    void OnTurnDown()
    {
        // calculate and store transform for "front" of block
        initFrontFace_transform();

        // rotate the cube around the Block World's x axis by 90 degrees.
        AnimationTargetRotation = newRotation(this.transform, navStart_axes_transform.right, -90);
        animState = AnimationStates.on;
    }

    Quaternion newRotation(Transform initialTransform, Vector3 axis, float angle)
    {
        // find the transform rotation in world coordinates
        var tempTransform = new GameObject().transform;
        tempTransform.parent = initialTransform.parent;
        tempTransform.position.Set(
            initialTransform.position.x,
            initialTransform.position.y,
            initialTransform.position.z);
        tempTransform.rotation = Quaternion.Euler(initialTransform.rotation.eulerAngles.x,
            initialTransform.rotation.eulerAngles.y,
            initialTransform.rotation.eulerAngles.z);
        tempTransform.RotateAround(tempTransform.position, axis, angle);
        return tempTransform.localRotation;
    }
    #endregion
}