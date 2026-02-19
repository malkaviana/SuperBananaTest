using UnityEngine;
using UnityEngine.UI;

namespace SuperBanana
{
    /// <summary>
    /// Attach to the Pause button. On click opens the pause popup (sets IsOpen true).
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class PauseButtonOpener : MonoBehaviour
    {
        [SerializeField] private PauseMenuController pausePopup;

        private void Awake()
        {
            Button button = GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(OpenPause);
        }

        private void OpenPause()
        {
            var popup = pausePopup != null ? pausePopup : PauseMenuController.Instance;
            if (popup != null)
                popup.Open();
        }
    }
}
