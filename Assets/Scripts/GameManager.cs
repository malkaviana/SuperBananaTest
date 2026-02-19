using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperBanana
{
    /// <summary>
    /// Global game state: score, combo, timer. Combo: after 2 sets collected, handle fills and pops,
    /// then decays over 15s. Each set collected while combo is active multiplies points by combo and resets the timer.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private readonly List<ChainController> _chainListeners = new List<ChainController>();

        [SerializeField] private GameConfig config;
        [Tooltip("If not set - searched in Resources or created default")]
        public GameConfig Config => config;

        private int _score;
        private int _combo;
        private int _setsCollected;
        private float _comboTimer;
        private float _timerRemaining;
        private bool _lastMoveHadMatch;
        private bool _gameOver;

        public int Score => _score;
        /// <summary>Current combo multiplier (0 when inactive, 2+ when active).</summary>
        public int Combo => _combo;
        /// <summary>Sets collected toward next combo activation (0, 1, or 2).</summary>
        public int SetsCollected => _setsCollected;
        /// <summary>Combo timer remaining in seconds (0 when inactive).</summary>
        public float ComboTimerRemaining => _comboTimer;
        /// <summary>Combo handle fill 0..1: only visible when combo is active, then decreases from 1 to 0 over decayDuration.</summary>
        public float ComboFillAmount => _comboTimer > 0
            ? Mathf.Clamp01(_comboTimer / config.comboDecayDuration)
            : 0f;
        public float TimerRemaining => _timerRemaining;
        public bool IsGameOver => _gameOver;

        public int TimerSecondsRemaining => Mathf.Max(0, Mathf.CeilToInt(_timerRemaining));

        public event Action<int> OnMatchScored;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnComboChanged;
        /// <summary>Fired when combo activates (handle full, do pop effect).</summary>
        public event Action OnComboActivated;
        public event Action<int> OnTimerTick;
        public event Action OnTimerEnd;
        /// <summary>Fired when all elements are collected (level cleared).</summary>
        public event Action OnLevelComplete;
        public event Action OnChainBroken;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (config == null)
                config = Resources.Load<GameConfig>("GameConfig");
            if (config == null)
                config = ScriptableObject.CreateInstance<GameConfig>();
        }

        private void Start()
        {
            _timerRemaining = config.timerSeconds;
            _score = 0;
            _combo = 0;
            _setsCollected = 0;
            _comboTimer = 0f;
            _lastMoveHadMatch = false;
            _gameOver = false;
            OnScoreChanged?.Invoke(_score);
            OnComboChanged?.Invoke(_combo);
            OnTimerTick?.Invoke(TimerSecondsRemaining);
        }

        private void Update()
        {
            if (_gameOver) return;

            _timerRemaining -= Time.deltaTime;
            int prevSeconds = TimerSecondsRemaining + (Mathf.CeilToInt(Time.deltaTime) > 0 ? 1 : 0);
            int nowSeconds = TimerSecondsRemaining;
            if (nowSeconds != prevSeconds)
                OnTimerTick?.Invoke(nowSeconds);

            if (_timerRemaining <= 0f)
            {
                _timerRemaining = 0f;
                _gameOver = true;
                OnTimerEnd?.Invoke();
            }

            // Combo decay
            if (_comboTimer > 0f)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f)
                {
                    _comboTimer = 0f;
                    SetCombo(0);
                }
            }
        }

        /// <summary>
        /// Call after move: was at least one match destroyed in this move.
        /// Does not reset combo (combo is time-based).
        /// </summary>
        public void NotifyMoveCompleted(bool hadMatch)
        {
            _lastMoveHadMatch = hadMatch;
        }

        /// <summary>
        /// Call for each set of 3 collected.
        /// If combo is active: points *= combo, combo++, timer reset to 15s.
        /// Otherwise: add base points; after 2 sets total, activate combo (handle full, pop, 15s decay).
        /// </summary>
        public void AddMatch()
        {
            int pointsToAdd;

            if (_comboTimer > 0f)
            {
                // Combo active: multiply points by combo, then increase combo and reset timer
                pointsToAdd = config.basePointsPerMatch * _combo;
                _score += pointsToAdd;
                SetCombo(_combo + 1);
                _comboTimer = config.comboDecayDuration;
            }
            else
            {
                // Building toward combo: add base points, count sets
                pointsToAdd = config.basePointsPerMatch;
                _score += pointsToAdd;
                _setsCollected = Mathf.Min(_setsCollected + 1, config.setsToActivateCombo);

                if (_setsCollected >= config.setsToActivateCombo)
                {
                    // Activate combo: handle was full, now it "pops" and starts 15s decay
                    _setsCollected = 0;
                    SetCombo(config.setsToActivateCombo);
                    _comboTimer = config.comboDecayDuration;
                    OnComboActivated?.Invoke();
                }
            }

            OnMatchScored?.Invoke(pointsToAdd);
            OnScoreChanged?.Invoke(_score);

            for (int i = _chainListeners.Count - 1; i >= 0; i--)
            {
                if (_chainListeners[i] != null)
                    _chainListeners[i].NotifyMatchScored();
                else
                    _chainListeners.RemoveAt(i);
            }
        }

        internal void RegisterChain(ChainController chain)
        {
            if (chain != null && !_chainListeners.Contains(chain))
                _chainListeners.Add(chain);
        }

        internal void UnregisterChain(ChainController chain)
        {
            _chainListeners.Remove(chain);
        }

        private void SetCombo(int value)
        {
            _combo = value;
            OnComboChanged?.Invoke(_combo);
        }

        /// <summary>Call when no elements remain (level cleared). Sets game over and fires OnLevelComplete.</summary>
        public void NotifyLevelComplete()
        {
            if (_gameOver) return;
            _gameOver = true;
            OnLevelComplete?.Invoke();
        }

        public void NotifyChainBroken()
        {
            OnChainBroken?.Invoke();
        }
    }
}
