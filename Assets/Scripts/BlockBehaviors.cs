﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

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

    private bool IsNotSelectedBySomeoneElse()
    {
        if (this.transform.localPosition.y != (originalLocalPosition.y + 4 * Globals.BlockSpacing))
            return true;
        else
            return false;
    }

    public void OnFocusEnter()
    {
        if(Globals.Instance.SelectedBlock != this.gameObject && IsNotSelectedBySomeoneElse())
        {
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
        if (Globals.Instance.SelectedBlock != this.gameObject && IsNotSelectedBySomeoneElse())
        {
            // move the block up back down to the matrix plane
            AnimationTargetPosition = originalLocalPosition;
            movementSpeed = 0.2f;
            animState = AnimationStates.on;
        }
    }

    public void OnSelect()
    {
        Globals.Instance.SelectedBlock = this.gameObject;

        // move the block up off the matrix plane
        AnimationTargetPosition = new Vector3(
            this.transform.localPosition.x,
            this.transform.localPosition.y + 4 * Globals.BlockSpacing,
            this.transform.localPosition.z);
        AnimationTargetScale = new Vector3(
            originalLocalScale.x * 2,
            originalLocalScale.y * 2,
            originalLocalScale.z * 2);
        animState = AnimationStates.on;
        movementSpeed = 2f;
    }

    public void OnUnselect()
    {
        Globals.Instance.SelectedBlock = null;

        // move the block back to the original position in the matrix plane
        AnimationTargetPosition = originalLocalPosition;
        AnimationTargetScale = originalLocalScale;
        animState = AnimationStates.on;
        movementSpeed = 2f;
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
            this.OnSelect();
        }
    }
}
