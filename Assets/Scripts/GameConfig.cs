using UnityEngine;

namespace SuperBanana
{
    /// <summary>
    /// Game settings. Can create asset: Right-click in Project → Create → SuperBanana → Game Config.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "SuperBanana/Game Config", order = 0)]
    public class GameConfig : ScriptableObject
    {
        [Header("Timer")]
        [Tooltip("Level duration in seconds")]
        public int timerSeconds = 120;

        [Header("Score")]
        [Tooltip("Base points per match (one set of 3)")]
        public int basePointsPerMatch = 10;

        [Header("Combo")]
        [Tooltip("Number of sets to collect (any order) to fill combo handle and activate combo")]
        public int setsToActivateCombo = 2;
        [Tooltip("Combo handle decay time in seconds (from full to zero)")]
        [Range(1f, 60f)]
        public float comboDecayDuration = 15f;

        [Header("Chain")]
        [Tooltip("How many matches needed to break chain")]
        public int matchesToBreakChain = 2;
        [Tooltip("Chain break sound delay from animation start (in seconds). Can override per chain individually")]
        [Range(0f, 2f)]
        public float chainBreakSoundDelay = 0.3f;

        [Header("Sounds (AudioClip)")]
        public AudioClip soundMatch;
        public AudioClip soundChainTap;
        public AudioClip soundChainBreak;
        public AudioClip soundPickup;
        public AudioClip soundPlace;
        [Tooltip("Rocket explosion sound. If set, overrides sound from RocketProjectile prefab")]
        public AudioClip soundRocketExplosion;

        [Header("VFX Prefabs")]
        public GameObject matchVfxPrefab;
        public GameObject chainBreakVfxPrefab;

        [Header("Outline Settings")]
        [Tooltip("Default outline size for elements (sprite duplicate scale, e.g. 1.03 = +3%). Can override per element individually")]
        [Range(1.01f, 1.2f)]
        public float defaultOutlineThickness = 1.03f;
        
        [Tooltip("Default outline color. Can override per element individually")]
        public Color defaultOutlineColor = Color.white;
    }
}
