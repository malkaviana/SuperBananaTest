using UnityEngine;
using TMPro;

namespace SuperBanana
{
    /// <summary>
    /// Attach to score counter prefab (StarCounter). Updates text by GameManager.Score.
    /// Text is searched in child object (StarAmount) with TextMeshProUGUI or set manually.
    /// </summary>
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        private bool _subscribed;

        private void Awake()
        {
            if (label == null)
                label = GetComponentInChildren<TMP_Text>(true);
        }

        private void Start()
        {
            TrySubscribe();
        }

        private void Update()
        {
            if (!_subscribed)
                TrySubscribe();
        }

        private void TrySubscribe()
        {
            if (_subscribed || GameManager.Instance == null) return;

            GameManager.Instance.OnScoreChanged += UpdateText;
            UpdateText(GameManager.Instance.Score);
            _subscribed = true;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnScoreChanged -= UpdateText;
            _subscribed = false;
        }

        private void UpdateText(int score)
        {
            if (label != null)
                label.text = score.ToString();
        }
    }
}
