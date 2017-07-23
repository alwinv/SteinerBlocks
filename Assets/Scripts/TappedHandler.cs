using HoloToolkit.Sharing;
using UnityEngine;
using UnityEngine.VR.WSA.Input;
using HoloToolkit.Unity.InputModule;

public class TappedHandler : MonoBehaviour, ISpeechHandler
{
    public GameObject sharedBlocks_Grid;

    GestureRecognizer recognizer;
    Vector3 currentlyNavigatingDirection = Vector3.zero;

    void Start()
    {
        this.recognizer = new GestureRecognizer();
        recognizer.SetRecognizableGestures(
            GestureSettings.Tap |
            GestureSettings.NavigationX |
            GestureSettings.NavigationY |
            GestureSettings.NavigationZ);
        this.recognizer.TappedEvent += OnTapped;

        // block rotation input handlers
        this.recognizer.NavigationStartedEvent += Recognizer_NavigationStartedEvent;
        this.recognizer.NavigationUpdatedEvent += Recognizer_NavigationUpdatedEvent;
        this.recognizer.NavigationCanceledEvent += Recognizer_NavigationCanceledEvent;
        this.recognizer.NavigationCompletedEvent += Recognizer_NavigationCompletedEvent;

        // start recognition
        this.recognizer.StartCapturingGestures();

        SharingStage.Instance.SharingManagerConnected += Instance_SharingManagerConnected;
    }

    private void Instance_SharingManagerConnected(object sender, System.EventArgs e)
    {
    }

    private void Recognizer_NavigationStartedEvent(InteractionSourceKind source, Vector3 normalizedOffset, Ray headRay)
    {
        if(Globals.Instance.SelectedBlock != null)
        {
            Globals.CurrentlyNavigating = true;
        }
    }

    private void Recognizer_NavigationUpdatedEvent(InteractionSourceKind source, Vector3 normalizedOffset, Ray headRay)
    {
        if (Globals.Instance.SelectedBlock != null)
        {
            if (currentlyNavigatingDirection == Vector3.zero)
            {
                // try locking navigation direction if not yet locked
                if (TryLockNavigationDirection(normalizedOffset))
                {
                    Globals.Instance.SelectedBlock.SendMessageUpwards("OnRotateRelativeInit", currentlyNavigatingDirection);
                }
            }
            else
            {
                // rotate block relative to navigation offset
                Globals.Instance.SelectedBlock.SendMessageUpwards("OnRotateRelative", Vector3.Scale(normalizedOffset, currentlyNavigatingDirection));
            }
        }
    }

    // Lock navigation direction to one axis if user moves > n units in one axis
    bool TryLockNavigationDirection(Vector3 relativePosition)
    {
        // define the smallest movement that can trigger lock of a directional navigation
        // helps make sure user is actually intending to start in that direction
        float triggerDelta = 0.1f;

        // lock rotation to one axis 
        if (Mathf.Abs(relativePosition.x) > triggerDelta
            | Mathf.Abs(relativePosition.y) > triggerDelta
            | Mathf.Abs(relativePosition.z) > triggerDelta)
        {
            if (Mathf.Abs(relativePosition.x) >= Mathf.Abs(relativePosition.y)
                && Mathf.Abs(relativePosition.x) >= Mathf.Abs(relativePosition.z))
            {
                currentlyNavigatingDirection = Vector3.left; // was (1, 0, 0)
            }
            else if (Mathf.Abs(relativePosition.y) >= Mathf.Abs(relativePosition.x) 
                && Mathf.Abs(relativePosition.y) >= Mathf.Abs(relativePosition.z))
            {
                currentlyNavigatingDirection = Vector3.up; // was (0, 1, 0)
            }
            else
            {
                currentlyNavigatingDirection = Vector3.forward; // was (0, 0, 1)
            }
            return true;
        }
        else
            return false;
    }

    private void Recognizer_NavigationCompletedEvent(InteractionSourceKind source, Vector3 normalizedOffset, Ray headRay)
    {
        if (Globals.Instance.SelectedBlock != null)
        {
            EndNavigation(normalizedOffset);
        }
    }

    private void Recognizer_NavigationCanceledEvent(InteractionSourceKind source, Vector3 normalizedOffset, Ray headRay)
    {
        if (Globals.Instance.SelectedBlock != null)
        {
            EndNavigation(normalizedOffset);
        }
    }

    void EndNavigation(Vector3 relativePosition)
    {
        Globals.CurrentlyNavigating = false;
        currentlyNavigatingDirection = Vector3.zero;

        // Send a message to the selected object and its ancestors
        // to finish the rotation
        Globals.Instance.SelectedBlock.SendMessageUpwards("OnRotateRelativeSnap", relativePosition);
    }

    void OnTapped(InteractionSourceKind source, int tapCount, Ray headRay)
    {
        // If we're networking...
        if (SharingStage.Instance.IsConnected)
        {
            // if there are no shared blocks yet, create them
            if (SharingStage.Instance.Root.InstantiatedPrefabs.GetDataArray().GetLength(0) == 0)
            {
                // load blocks into the block grid
                sharedBlocks_Grid.SendMessage("OnLoadFile_ForSharing", "shared.blocks");
            }
            this.recognizer.TappedEvent -= OnTapped;
        }
    }

    public void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
    {
        if (Globals.Instance.SelectedBlock != null)
        {
            string keyWords = eventData.RecognizedText.ToLower();
            switch (keyWords)
            {
                case "left":
                    Globals.Instance.SelectedBlock.SendMessage("OnTurnLeft");
                    break;
                case "right":
                    Globals.Instance.SelectedBlock.SendMessage("OnTurnRight");
                    break;
                case "up":
                    Globals.Instance.SelectedBlock.SendMessage("OnTurnUp");
                   break;
                case "down":
                    Globals.Instance.SelectedBlock.SendMessage("OnTurnDown");
                    break;
                case "turn":
                    Globals.Instance.SelectedBlock.SendMessage("OnRotate");
                    break;
                case "flip":
                    Globals.Instance.SelectedBlock.SendMessage("OnFlip");
                    break;
                case "release":
                    Globals.Instance.SelectedBlock.SendMessage("OnUnselect");
                    break;
                default:
                    break;
            }
        }
        else if (Globals.Instance.FocusedObject != null)
        {
            string keyWords = eventData.RecognizedText.ToLower();
            switch (keyWords)
            {
                case "select":
                    Globals.Instance.FocusedObject.SendMessage("OnSelect");
                    break;
                default:
                    break;
            }
        }

    }

}