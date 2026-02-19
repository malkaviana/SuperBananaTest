using UnityEngine;
using UnityEngine.UI;

namespace SuperBanana
{
    /// <summary>
    /// Pause popup: drives Animator IsOpen (true = opening animation, false = closing).
    /// Wire Close (X), Continue, Quit buttons. Pauses time when open.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        public static PauseMenuController Instance { get; private set; }

        [SerializeField] private Animator animator;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button quitButton;

        private static readonly int IsOpenId = Animator.StringToHash("IsOpen");
        private bool _isOpen;

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (animator == null)
                animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);

            if (animator != null)
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
            if (continueButton != null)
                continueButton.onClick.AddListener(Close);
            if (quitButton != null)
                quitButton.onClick.AddListener(Quit);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Call from Pause button: open popup and play opening animation.</summary>
        public void Open()
        {
            gameObject.SetActive(true);
            if (animator != null)
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            _isOpen = true;
            if (animator != null)
                animator.SetBool(IsOpenId, true);
            Time.timeScale = 0f;
        }

        /// <summary>Close popup and play closing animation.</summary>
        public void Close()
        {
            _isOpen = false;
            if (animator != null)
                animator.SetBool(IsOpenId, false);
            Time.timeScale = 1f;
        }

        private void Quit()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
