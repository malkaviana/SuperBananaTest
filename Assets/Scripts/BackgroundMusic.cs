using UnityEngine;

namespace SuperBanana
{
    /// <summary>
    /// Optional: attach to GameObject with AudioSource for background music.
    /// Mutes/unmutes based on GameSettings.MusicEnabled.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class BackgroundMusic : MonoBehaviour
    {
        public static BackgroundMusic Instance { get; private set; }

        private AudioSource _source;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _source = GetComponent<AudioSource>();
        }

        private void Start()
        {
            if (GameSettings.Instance != null)
                SetMuted(!GameSettings.Instance.MusicEnabled);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void SetMuted(bool muted)
        {
            if (_source != null)
                _source.mute = muted;
        }
    }
}
