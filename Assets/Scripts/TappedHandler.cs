using HoloToolkit.Sharing;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

public class TappedHandler : MonoBehaviour
{
    void Start()
    {
        this.recognizer = new GestureRecognizer();
        this.recognizer.TappedEvent += OnTapped;
        this.recognizer.StartCapturingGestures();
    }
    void OnTapped(InteractionSourceKind source, int tapCount, Ray headRay)
    {
        // If we're networking...
        if (SharingStage.Instance.IsConnected)
        {
            if(SharingStage.Instance.Root.InstantiatedPrefabs.GetDataArray().GetLength(0) == 0)
            {
                // load blocks into the block grid
                this.BroadcastMessage("OnLoadFile_ForSharing", "ms-appx:///Blocks/1.blocks");
            }
            this.recognizer.TappedEvent -= OnTapped;
        }
    }
    GestureRecognizer recognizer;
}