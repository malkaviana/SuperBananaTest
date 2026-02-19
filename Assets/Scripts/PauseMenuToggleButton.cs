using UnityEngine;
using UnityEngine.UI;

namespace SuperBanana
{
    public enum PauseMenuToggleType
    {
        Music,
        SoundEffects,
        Vibration
    }

    /// <summary>
    /// Toggle button in pause menu: Music / Sound / Vibration.
    /// When OFF: overlay image is visible, button uses pressed color. Saves state to PlayerPrefs.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class PauseMenuToggleButton : MonoBehaviour
    {
        [SerializeField] private PauseMenuToggleType type;
        [SerializeField] private Image overlayImage;
        [SerializeField] private Image targetGraphic;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);

        private Button _button;
        private bool _isOn;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (targetGraphic == null)
                targetGraphic = _button.targetGraphic as Image;
            if (overlayImage == null)
            {
                var t = transform.Find("TurnOffIcon");
                if (t != null)
                    overlayImage = t.GetComponent<Image>();
            }

            _button.onClick.AddListener(OnClick);
        }

        private void Start()
        {
            if (GameSettings.Instance != null)
            {
                _isOn = type == PauseMenuToggleType.Music ? GameSettings.Instance.MusicEnabled
                    : type == PauseMenuToggleType.SoundEffects ? GameSettings.Instance.SoundEffectsEnabled
                    : GameSettings.Instance.VibrationEnabled;
            }
            else
            {
                _isOn = true;
            }
            ApplyState();
        }

        private void OnClick()
        {
            _isOn = !_isOn;
            ApplyState();
            ApplyToGame();
        }

        private void ApplyState()
        {
            if (overlayImage != null)
                overlayImage.gameObject.SetActive(!_isOn);
            if (targetGraphic != null)
                targetGraphic.color = _isOn ? normalColor : pressedColor;
        }

        private void ApplyToGame()
        {
            if (GameSettings.Instance == null) return;
            switch (type)
            {
                case PauseMenuToggleType.Music:
                    GameSettings.Instance.SetMusicEnabled(_isOn);
                    break;
                case PauseMenuToggleType.SoundEffects:
                    GameSettings.Instance.SetSoundEffectsEnabled(_isOn);
                    break;
                case PauseMenuToggleType.Vibration:
                    GameSettings.Instance.SetVibrationEnabled(_isOn);
                    break;
            }
        }
    }
}
