using System.Collections;
using UnityEngine;
using SteinerBlocks.Persistence;

namespace SteinerBlocks.Game
{
    /// <summary>
    /// Central game manager. Replaces the old Globals.cs.
    /// Manages app state, slideshow, and references to key objects.
    /// No platform-specific dependencies (no WSA, no WorldAnchors).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Grid References")]
        public BlockGridController slideshowGrid;
        public BlockGridController localGrid;

        [Header("Selection")]
        public SelectionManager selectionManager;
        public SelectionHighlight selectionHighlight;

        [Header("Slideshow Settings")]
        [SerializeField] float slideDuration = 5.0f;

        [Header("Block Settings")]
        public static float BlockSpacing = 0.0195f;
        public static float SelectedBlockScale = 1.25f;

        // Slideshow state
        public bool SlideshowRunning { get; set; }
        float timeSinceLastSlide;

        // Random number generator for textures etc.
        public System.Random Rng { get; private set; }

        // Bundled pattern file names for slideshow
        static readonly string[] PatternFileNames = {
            "001.blocks", "002.blocks", "003.blocks", "004.blocks",
            "005.blocks", "006.blocks",
            "013.blocks", "014.blocks", "015.blocks", "016.blocks",
            "017.blocks", "018.blocks",
            "081.blocks",
            "084.blocks", "085.blocks", "086.blocks", "087.blocks",
            "091.blocks"
        };

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Rng = new System.Random(System.DateTime.Now.Millisecond);
        }

        IEnumerator Start()
        {
            // Load slideshow grid (hidden initially)
            slideshowGrid.LoadPatternFiles(PatternFileNames);
            slideshowGrid.SetVisible(false);
            SlideshowRunning = false;

            // Load local/user grid (hidden initially)
            localGrid.LoadFile("my.blocks");
            localGrid.SetVisible(false);

            // Start intro sequence
            yield return StartCoroutine(IntroSequence());
        }

        void Update()
        {
            if (SlideshowRunning)
            {
                timeSinceLastSlide += Time.deltaTime;
                if (timeSinceLastSlide >= slideDuration)
                {
                    timeSinceLastSlide = 0f;
                    slideshowGrid.ShowNextPattern();
                }
            }
        }

        #region Public Commands

        public void ShowSlideshow()
        {
            slideshowGrid.SetVisible(true);
            SlideshowRunning = true;
            timeSinceLastSlide = 0f;
        }

        public void HideSlideshow()
        {
            slideshowGrid.SetVisible(false);
            SlideshowRunning = false;
        }

        public void PauseSlideshow()
        {
            SlideshowRunning = false;
        }

        public void ResumeSlideshow()
        {
            SlideshowRunning = true;
        }

        public void ShowLocalBlocks()
        {
            localGrid.SetVisible(true);
        }

        public void HideLocalBlocks()
        {
            localGrid.SetVisible(false);
        }

        public void SaveLocalBlocks()
        {
            localGrid.SaveToFile();
        }

        #endregion

        #region Intro Sequence

        IEnumerator IntroSequence()
        {
            // Brief delay before starting
            yield return new WaitForSeconds(3f);

            // Show slideshow of examples
            ShowSlideshow();
            yield return new WaitForSeconds(30f);

            // After slideshow intro, show local blocks for editing
            ShowLocalBlocks();
        }

        #endregion
    }
}
