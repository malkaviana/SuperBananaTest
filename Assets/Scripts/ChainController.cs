using UnityEngine;

namespace SuperBanana
{
    /// <summary>
    /// Chain that blocks an entire shelf (all 3 slots). On tap - plays Tap animation + sound.
    /// After N matches destroyed (from GameConfig) - plays Break animation + VFX + sound, shelf unlocks.
    /// Attach to root object of Chain prefab (with Animator). Animator parameters: Tap (Trigger), Break (Trigger).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class ChainController : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] [Tooltip("Shelf (all its slots) that this chain blocks. If empty and chain is child of shelf, will be set automatically")]
        private ShelfController blockedShelf;
        [SerializeField] [Tooltip("Delay of break sound from animation start (in seconds). If 0, uses value from GameConfig")]
        private float breakSoundDelay = 0f;

        private Animator _animator;
        private GameConfig _config;
        private int _matchesRemaining;
        private bool _isBroken;
        private static readonly int Tap = Animator.StringToHash("Tap");
        private static readonly int Break = Animator.StringToHash("Break");

        /// <summary>Chain blocks shelf while not broken and chain object is active in hierarchy</summary>
        public bool IsBlocking => !_isBroken && _matchesRemaining > 0 && gameObject.activeInHierarchy;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            EnsureAudioSource();
            LoadConfig();

            if (blockedShelf == null)
                blockedShelf = GetComponentInParent<ShelfController>();
            
            blockedShelf?.SetChainBlocker(this);
        }

        private void EnsureAudioSource()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        private void LoadConfig()
        {
            if (_config == null && GameManager.Instance != null)
                _config = GameManager.Instance.Config;

            _matchesRemaining = _config != null ? _config.matchesToBreakChain : 2;
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null && !_isBroken)
                GameManager.Instance.RegisterChain(this);
        }

        private void Start()
        {
            if (GameManager.Instance != null && !_isBroken)
                GameManager.Instance.RegisterChain(this);
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.UnregisterChain(this);
        }

        /// <summary>Called by GameManager when a set is collected. Counts down; at 0 plays break.</summary>
        public void NotifyMatchScored()
        {
            if (_isBroken || _matchesRemaining <= 0) return;
            _matchesRemaining--;
            if (_matchesRemaining <= 0)
                PlayBreak();
        }

        /// <summary>Called when tapping a slot blocked by chain</summary>
        public void PlayTap()
        {
            _animator.SetTrigger(Tap);
            EnsureAudioSource();
            LoadConfig();
            
            if (audioSource.enabled && _config?.soundChainTap != null && (GameSettings.Instance == null || GameSettings.Instance.SoundEffectsEnabled))
                audioSource.PlayOneShot(_config.soundChainTap);
        }

        private void PlayBreak()
        {
            if (_isBroken) return;
            _isBroken = true;

            if (GameManager.Instance != null)
                GameManager.Instance.UnregisterChain(this);

            _animator.SetTrigger(Break);
            EnsureAudioSource();
            
            float delay = breakSoundDelay > 0 ? breakSoundDelay : GetDefaultBreakSoundDelay();
            StartCoroutine(PlayBreakSoundDelayed(delay));
            
            if (_config?.chainBreakVfxPrefab != null)
            {
                GameObject vfx = Instantiate(_config.chainBreakVfxPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 4f);
            }

            blockedShelf?.SetChainBlocker(null);
            GameManager.Instance?.NotifyChainBroken();
        }

        private System.Collections.IEnumerator PlayBreakSoundDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (audioSource.enabled && _config?.soundChainBreak != null && (GameSettings.Instance == null || GameSettings.Instance.SoundEffectsEnabled))
                audioSource.PlayOneShot(_config.soundChainBreak);
        }
        
        private float GetDefaultBreakSoundDelay()
        {
            return GameManager.Instance?.Config?.chainBreakSoundDelay ?? 0.3f;
        }
    }
}
