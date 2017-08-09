using HoloToolkit.Sharing;
using UnityEngine;
using UnityEngine.VR.WSA.Input;
using HoloToolkit.Unity.InputModule;

public class TappedHandler : MonoBehaviour, ISpeechHandler
{
    public GameObject sharedBlocks_Grid;

    GestureRecognizer recognizer;

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
            if (Globals.CurrentlyNavigatingDirection == Vector3.zero)
            {
                // try locking navigation direction if not yet locked
                if (TryLockNavigationDirection(normalizedOffset))
                {
                    SendMessageToSelectedBlocks("OnRotateRelativeInit");
                }
            }
            else
            {
                // rotate block relative to navigation offset
                SendMessageToSelectedBlocks("OnRotateRelative", Vector3.Scale(normalizedOffset, Globals.CurrentlyNavigatingDirection));
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
                Globals.CurrentlyNavigatingDirection = Vector3.left; // was (1, 0, 0)
            }
            else if (Mathf.Abs(relativePosition.y) >= Mathf.Abs(relativePosition.x) 
                && Mathf.Abs(relativePosition.y) >= Mathf.Abs(relativePosition.z))
            {
                Globals.CurrentlyNavigatingDirection = Vector3.up; // was (0, 1, 0)
            }
            else
            {
                Globals.CurrentlyNavigatingDirection = Vector3.forward; // was (0, 0, 1)
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
        Globals.CurrentlyNavigatingDirection = Vector3.zero;

        // Send a message to the selected object and its ancestors
        // to finish the rotation
        SendMessageToSelectedBlocks("OnRotateRelativeSnap", relativePosition);
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
        string keyWords = eventData.RecognizedText.ToLower();
        var blockIOscript = (BlockIO)this.gameObject.GetComponent(typeof(BlockIO));

        if (Globals.Instance.SelectedBlock != null)
        {
            switch (keyWords)
            {
                case "left":
                    SendMessageToSelectedBlocks("OnTurnLeft");
                    break;
                case "right":
                    SendMessageToSelectedBlocks("OnTurnRight");
                    break;
                case "up":
                    SendMessageToSelectedBlocks("OnTurnUp");
                   break;
                case "down":
                    SendMessageToSelectedBlocks("OnTurnDown");
                    break;
                case "turn":
                    SendMessageToSelectedBlocks("OnRotate");
                    break;
                case "flip":
                    SendMessageToSelectedBlocks("OnFlip");
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
            switch (keyWords)
            {
                case "select":
                    Globals.Instance.FocusedObject.SendMessage("OnSelect", Globals.SelectionType.Block);
                    break;
                case "select column":
                    Globals.Instance.FocusedObject.SendMessage("OnSelect", Globals.SelectionType.Column);
                    break;
                case "select row":
                    Globals.Instance.FocusedObject.SendMessage("OnSelect", Globals.SelectionType.Row);
                    break;
                case "select all":
                    Globals.Instance.FocusedObject.SendMessage("OnSelect", Globals.SelectionType.All);
                    break;
                default:
                    break;
            }
        }
        switch (keyWords)
        {
            case "show examples":
                Globals.Instance.SlideShowBlocks_Parent.SendMessage("OnShow");
                Globals.Instance.SlideShowRunning = true;
                break;
            case "hide examples":
                Globals.Instance.SlideShowBlocks_Parent.SendMessage("OnHide");
                Globals.Instance.SlideShowRunning = false;
                break;
            case "pause":
                Globals.Instance.SlideShowRunning = false;
                break;
            case "resume":
                Globals.Instance.SlideShowRunning = true;
                break;
            case "show my blocks":
                Globals.Instance.LocalBlocks_Parent.SendMessage("OnShow");
                this.SendMessageUpwards("OnIntroToEditing");
                break;
            case "hide my blocks":
                Globals.Instance.LocalBlocks_Parent.SendMessage("OnHide");
                break;
            case "show shared blocks":
                Globals.Instance.SharedBlocks_Parent.SendMessage("OnShow");
                break;
            case "hide shared blocks":
                Globals.Instance.SharedBlocks_Parent.SendMessage("OnHide");
                break;
            default:
                break;
        }
    }

    private void SendMessageToSelectedBlocks(string Message)
    {
        SendMessageToSelectedBlocks(Message, null);
    }

    private void SendMessageToSelectedBlocks(string Message, object Param)
    {
        if (Globals.Instance.SelectionMode == Globals.SelectionType.Block)
        {
            Globals.Instance.SelectedBlock.SendMessage(Message, Param);
        }
        else
        {
            // find out index # of the selected block
            int selectedBlockIndex = getBlockIndex(Globals.Instance.SelectedBlock);

            Transform gridTransform = Globals.Instance.SelectedBlock.transform.parent;

            // cache grid width/height
            var blockIOscript = (BlockIO)gridTransform.parent.gameObject.GetComponent(typeof(BlockIO));
            int gridWidth = 0;
            int gridHeight = 0;

#if !UNITY_EDITOR
            gridWidth = blockIOscript.GetBlockGridSize().width;
            gridHeight = blockIOscript.GetBlockGridSize().height;
#endif

            if (selectedBlockIndex > 0 && gridHeight > 0 && gridWidth > 0)
            {
                // calculate index range for the full list of selected objects

                switch (Globals.Instance.SelectionMode)
                {
                    case Globals.SelectionType.Column:
                        int selectedColumnIndex = Mathf.FloorToInt(selectedBlockIndex / gridHeight);
                        for (int i = selectedColumnIndex * gridHeight; i < (selectedColumnIndex + 1) * gridHeight; i++)
                        {
                            gridTransform.GetChild(i).SendMessage(Message, Param);
                        }
                        break;
                    case Globals.SelectionType.Row:
                        int selectedRowIndex = Mathf.FloorToInt(selectedBlockIndex % gridHeight);
                        for (int j = 0; j < gridWidth; j++)
                        {
                            gridTransform.GetChild(selectedRowIndex + j * gridHeight).SendMessage(Message, Param);
                        }
                        break;
                    case Globals.SelectionType.All:
                        for (int i = 0; i < gridHeight * gridWidth; i++)
                        {
                            gridTransform.GetChild(i).SendMessage(Message, Param);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private int getBlockIndex(GameObject block)
    {
        Transform gridTransform = Globals.Instance.SelectedBlock.transform.parent;
        if (gridTransform != null)
        {
            for (int i = 0; i < gridTransform.childCount; i++)
            {
                if (gridTransform.GetChild(i).gameObject == Globals.Instance.SelectedBlock)
                {
                    return i;
                }
            }
        }
        return -1;
    }

}