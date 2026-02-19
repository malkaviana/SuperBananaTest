using UnityEngine;
using UnityEngine.SceneManagement;

namespace SuperBanana
{
    /// <summary>
    /// Shows level finish UI when all elements are gone (level cleared).
    /// Assign levelFinishUI (CanvasLevelFinish root) and placer or shelves to count elements.
    /// </summary>
    public class LevelFinishController : MonoBehaviour
    {
        public static LevelFinishController Instance { get; private set; }

        [SerializeField] private GameObject levelFinishUI;
        [SerializeField] private RandomShelfPlacer placer;
        [SerializeField] [Tooltip("If placer is not set, use these shelves to count elements")]
        private ShelfController[] shelves;

        private const string LevelFinishCanvasName = "CanvasLevelFinish";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // Do not destroy if this is the Level Finish canvas — otherwise we'd destroy the UI we need to show
                if (gameObject.name == LevelFinishCanvasName)
                    return;
                StartCoroutine(DestroyDuplicateNextFrame());
                return;
            }
            Instance = this;

            if (levelFinishUI == null)
                levelFinishUI = FindLevelFinishCanvas();

            if (levelFinishUI != null)
                levelFinishUI.SetActive(false);
            else
                Debug.LogWarning("LevelFinishController: Level Finish UI not assigned and no GameObject named '" + LevelFinishCanvasName + "' found in scene. Assign it in the Inspector or add the canvas to the scene.");

            if (placer == null)
                placer = FindFirstObjectByType<RandomShelfPlacer>();
        }

        private static GameObject FindLevelFinishCanvas()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                if (root.name == LevelFinishCanvasName)
                    return root;
                Transform found = FindInChildren(root.transform, LevelFinishCanvasName);
                if (found != null)
                    return found.gameObject;
            }
            return null;
        }

        private static Transform FindInChildren(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Transform t = FindInChildren(child, name);
                if (t != null) return t;
            }
            return null;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private System.Collections.IEnumerator DestroyDuplicateNextFrame()
        {
            yield return null;
            if (this != null && gameObject != null)
                Destroy(gameObject);
        }

        /// <summary>Call from ShelfController/ElementController. Finds controller if Instance is null (e.g. inactive).</summary>
        public static void TryCheckLevelComplete()
        {
            LevelFinishController c = Instance ?? Object.FindFirstObjectByType<LevelFinishController>(FindObjectsInactive.Include);
            if (c != null)
                c.CheckLevelComplete();
        }

        /// <summary>Call after match resolution or element destroyed. If no elements left, shows UI and notifies game over.</summary>
        public void CheckLevelComplete()
        {
            if (GameManager.Instance == null || GameManager.Instance.IsGameOver) return;

            if (levelFinishUI == null)
                levelFinishUI = FindLevelFinishCanvas();

            int count = CountElements();
            if (count < 0)
            {
                Debug.LogWarning("LevelFinishController: Placer and Shelves not set — cannot count elements. Assign RandomShelfPlacer or Shelves in Inspector.");
                return;
            }
            if (count > 0) return;

            GameManager.Instance.NotifyLevelComplete();
            if (levelFinishUI != null)
            {
                levelFinishUI.SetActive(true);
                Animator anim = levelFinishUI.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.updateMode = AnimatorUpdateMode.UnscaledTime;
                    anim.SetBool(Animator.StringToHash("IsOpen"), true);
                }
                Debug.Log("LevelFinishController: Level complete — showing Level Finish UI.");
            }
            else
            {
                Debug.LogWarning("LevelFinishController: Level complete but no UI to show. Assign Level Finish UI in Inspector or add a GameObject named '" + LevelFinishCanvasName + "' to the scene.");
            }
        }

        /// <returns>Number of elements, or -1 if shelves not configured (cannot count).</returns>
        private int CountElements()
        {
            int count = 0;
            if (placer != null && placer.Shelves != null)
            {
                foreach (ShelfController shelf in placer.Shelves)
                {
                    if (shelf == null) continue;
                    foreach (var slot in shelf.Slots)
                        if (slot?.CurrentElement != null) count++;
                }
                return count;
            }
            if (shelves != null && shelves.Length > 0)
            {
                foreach (var shelf in shelves)
                {
                    if (shelf == null) continue;
                    foreach (var slot in shelf.Slots)
                        if (slot?.CurrentElement != null) count++;
                }
                return count;
            }
            return -1;
        }
    }
}
