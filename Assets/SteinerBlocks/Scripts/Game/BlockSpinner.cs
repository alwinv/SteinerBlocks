using UnityEngine;

namespace SteinerBlocks.Game
{
    /// <summary>
    /// Continuously rotates a block around the Y axis.
    /// Used for decorative/preview blocks. Replaces SpinBlock.cs.
    /// </summary>
    public class BlockSpinner : MonoBehaviour
    {
        [SerializeField] float speed = 50f;

        void Update()
        {
            transform.Rotate(Vector3.up, speed * Time.deltaTime, Space.World);
        }
    }
}
