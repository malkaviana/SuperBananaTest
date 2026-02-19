using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SuperBanana
{
    /// <summary>
    /// Combo UI: handle fill (from ComboFillAmount), combo counter text, pop effect when combo activates.
    /// Handle fills after 2 sets, pops, then decays to zero over 15 seconds. Visible when fill > 0.
    /// </summary>
    public class ComboUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Scrollbar scrollbar;
        [SerializeField] private GameObject handleObject;
        [SerializeField] private string format = "Combo x {0}";

        [Header("Pop effect")]
        [SerializeField] private RectTransform popTransform;
        [SerializeField] private float popScalePeak = 1.2f;
        [SerializeField] private float popDuration = 0.2f;

        private float _popTimeLeft;

        private void Awake()
        {
            if (label == null)
                label = GetComponentInChildren<TMP_Text>(true);
            if (scrollbar == null)
                scrollbar = GetComponentInChildren<Scrollbar>(true);
            if (handleObject == null && scrollbar != null && scrollbar.handleRect != null)
                handleObject = scrollbar.handleRect.gameObject;
            if (popTransform == null && scrollbar != null)
                popTransform = scrollbar.GetComponent<RectTransform>();

            // Make scrollbar non-interactive (handle stretches, doesn't move)
            if (scrollbar != null)
            {
                scrollbar.interactable = false;
                scrollbar.value = 0f; // Position handle at left, it will stretch to the right
            }
        }

        private void Start()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.OnComboChanged += OnComboChanged;
            GameManager.Instance.OnComboActivated += OnComboActivated;
            Refresh(GameManager.Instance.Combo, GameManager.Instance.ComboFillAmount);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnComboChanged -= OnComboChanged;
                GameManager.Instance.OnComboActivated -= OnComboActivated;
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;
            Refresh(GameManager.Instance.Combo, GameManager.Instance.ComboFillAmount);

            if (_popTimeLeft > 0f)
            {
                _popTimeLeft -= Time.deltaTime;
                float t = 1f - (_popTimeLeft / popDuration);
                float scale = 1f + (Mathf.Sin(t * Mathf.PI) * (popScalePeak - 1f));
                if (popTransform != null)
                    popTransform.localScale = Vector3.one * scale;
            }
        }

        private void OnComboChanged(int combo)
        {
            Refresh(combo, GameManager.Instance != null ? GameManager.Instance.ComboFillAmount : 0f);
        }

        private void OnComboActivated()
        {
            _popTimeLeft = popDuration;
        }

        private void Refresh(int combo, float fillAmount)
        {
            if (label != null)
                label.text = string.Format(format, combo);

            if (scrollbar != null)
            {
                // Handle stretches from left to right, size = fillAmount (0 = empty, 1 = full)
                scrollbar.size = Mathf.Clamp01(fillAmount);
                scrollbar.value = 0f; // Always keep handle at left position
            }

            if (handleObject != null)
                handleObject.SetActive(fillAmount > 0.001f);
        }
    }
}
