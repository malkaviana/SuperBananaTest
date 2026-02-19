using UnityEngine;

namespace SuperBanana
{
    /// <summary>
    /// Global audio/vibration settings. Persisted via PlayerPrefs (PauseMenuToggleButton).
    /// Optional: attach to a GameObject or use from first access.
    /// </summary>
    public class GameSettings : MonoBehaviour
    {
        public static GameSettings Instance { get; private set; }

        private const string PrefsMusic = "Settings_Music";
        private const string PrefsSound = "Settings_Sound";
        private const string PrefsVibration = "Settings_Vibration";

        private bool _musicEnabled = true;
        private bool _soundEnabled = true;
        private bool _vibrationEnabled = true;

        public bool MusicEnabled => _musicEnabled;
        public bool SoundEffectsEnabled => _soundEnabled;
        public bool VibrationEnabled => _vibrationEnabled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _musicEnabled = PlayerPrefs.GetInt(PrefsMusic, 1) != 0;
            _soundEnabled = PlayerPrefs.GetInt(PrefsSound, 1) != 0;
            _vibrationEnabled = PlayerPrefs.GetInt(PrefsVibration, 1) != 0;
            ApplyAll();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void SetMusicEnabled(bool on)
        {
            _musicEnabled = on;
            PlayerPrefs.SetInt(PrefsMusic, on ? 1 : 0);
            PlayerPrefs.Save();
            if (BackgroundMusic.Instance != null)
                BackgroundMusic.Instance.SetMuted(!on);
        }

        public void SetSoundEffectsEnabled(bool on)
        {
            _soundEnabled = on;
            PlayerPrefs.SetInt(PrefsSound, on ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetVibrationEnabled(bool on)
        {
            _vibrationEnabled = on;
            PlayerPrefs.SetInt(PrefsVibration, on ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void ApplyAll()
        {
            if (BackgroundMusic.Instance != null)
                BackgroundMusic.Instance.SetMuted(!_musicEnabled);
        }

        public void TriggerVibration()
        {
            if (!_vibrationEnabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }
}
