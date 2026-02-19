using UnityEngine;
using TMPro;

namespace SuperBanana
{
    /// <summary>
    /// Attach to Timer prefab. Updates text in MM:SS format by GameManager timer.
    /// Text is searched in child object (TimeRemider) with TextMeshProUGUI or set manually.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TimerUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        private void Awake()
        {
            if (label == null)
                label = GetComponentInChildren<TMP_Text>(true);
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTimerTick += UpdateText;
                UpdateText(GameManager.Instance.TimerSecondsRemaining);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnTimerTick -= UpdateText;
        }

        private void UpdateText(int secondsRemaining)
        {
            if (label == null) return;
            int m = secondsRemaining / 60;
            int s = secondsRemaining % 60;
            label.text = $"{m:D2}:{s:D2}";
        }
    }
}
