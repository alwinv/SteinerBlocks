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
            // load blocks into the block grid
            this.SendMessage("OnLoadFile", "ms-appx:///Blocks/1.blocks");
            this.recognizer.TappedEvent -= OnTapped;

      //      // Make a new cube that is 2m away in direction of gaze but then get that position
      //      // relative to the object that we are attached to (which is world anchor'd across
      //      // our devices).
      //      var newCubePosition =
      //this.gameObject.transform.InverseTransformPoint(
      //(GazeManager.Instance.GazeOrigin + GazeManager.Instance.GazeNormal * 2.0f));

            //      // Use the spawn manager to spawn a 'SyncSpawnedObject' at that position with
            //      // some random rotation, parent it off our gameObject, give it a base name (MyCube)
            //      // and do not claim ownership of it so it stays behind in the scene even if our
            //      // device leaves the session.
            //      this.spawnManager.Spawn(
            //new SyncSpawnedObject(),
            //newCubePosition,
            //Random.rotation,
            //blocksGrid,
            //"MyCube",
            //false);
        }
    }
    GestureRecognizer recognizer;
}