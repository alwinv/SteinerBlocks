using UnityEngine;
using SteinerBlocks.Game;

// Uncomment when XR Interaction Toolkit is imported:
// using UnityEngine.XR.Interaction.Toolkit;
// using UnityEngine.XR.Interaction.Toolkit.Interactables;
// using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SteinerBlocks.Input
{
    /// <summary>
    /// XR input handler using XR Interaction Toolkit (XRI).
    /// Handles hand tracking, controller input, and gaze interaction
    /// across Quest, Vision Pro, and Android XR.
    ///
    /// PHASE 2 IMPLEMENTATION — This is a structural stub.
    /// The XRI components (XRGrabInteractable, XRRayInteractor, etc.)
    /// will be configured in the Unity Editor on the block prefab
    /// and XR Origin rig.
    ///
    /// Architecture:
    /// - Each block prefab gets an XRGrabInteractable component
    /// - The XR Origin rig has XRRayInteractor (controllers) and
    ///   XRGazeInteractor (eye tracking for Vision Pro)
    /// - This script bridges XRI events to SelectionManager
    ///
    /// Input mapping per platform:
    /// ┌──────────────┬──────────────┬──────────────┬───────────────┐
    /// │ Action       │ Quest        │ Vision Pro   │ Android XR    │
    /// ├──────────────┼──────────────┼──────────────┼───────────────┤
    /// │ Focus        │ Ray hover    │ Gaze         │ Ray/gaze      │
    /// │ Select       │ Trigger/pinch│ Pinch        │ Pinch/tap     │
    /// │ Rotate       │ Thumbstick   │ Hand drag    │ Hand drag     │
    /// │ Deselect     │ Trigger/pinch│ Pinch        │ Pinch/tap     │
    /// └──────────────┴──────────────┴──────────────┴───────────────┘
    /// </summary>
    public class XRInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] GameManager gameManager;

        // TODO Phase 2: Add XR Interaction Toolkit references
        // [SerializeField] XRRayInteractor leftRayInteractor;
        // [SerializeField] XRRayInteractor rightRayInteractor;
        // [SerializeField] XRGazeInteractor gazeInteractor;

        void OnEnable()
        {
            // TODO Phase 2: Subscribe to XRI interactor events
            // Example:
            // rightRayInteractor.hoverEntered.AddListener(OnHoverEntered);
            // rightRayInteractor.hoverExited.AddListener(OnHoverExited);
            // rightRayInteractor.selectEntered.AddListener(OnSelectEntered);
        }

        void OnDisable()
        {
            // TODO Phase 2: Unsubscribe from XRI interactor events
        }

        // TODO Phase 2: XRI event handlers
        //
        // void OnHoverEntered(HoverEnterEventArgs args)
        // {
        //     var block = args.interactableObject.transform.GetComponent<BlockController>();
        //     if (block != null)
        //         selectionManager.FocusBlock(block);
        // }
        //
        // void OnHoverExited(HoverExitEventArgs args)
        // {
        //     selectionManager.UnfocusBlock();
        // }
        //
        // void OnSelectEntered(SelectEnterEventArgs args)
        // {
        //     var block = args.interactableObject.transform.GetComponent<BlockController>();
        //     if (block != null)
        //         selectionManager.ToggleSelection();
        // }
        //
        // For rotation via hand/controller movement:
        // Track the interactor's position delta during a grab and
        // call selectionManager.BeginNavigation() / UpdateNavigation() / EndNavigation()
    }
}
