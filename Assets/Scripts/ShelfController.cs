using System.Collections.Generic;
using UnityEngine;

namespace SuperBanana
{
    /// <summary>
    /// Shelf container: list of slots. Checks "three identical in one container", removes them, plays VFX and sound.
    /// Setup: child objects with ShelfSlot or slots set manually in list.
    /// </summary>
    public class ShelfController : MonoBehaviour
    {
        [SerializeField] private List<ShelfSlot> slots = new List<ShelfSlot>();
        [SerializeField] private GameConfig config;
        [SerializeField] [Tooltip("VFX spawn point for matches (or shelf center)")]
        private Transform matchVfxSpawnPoint;
        [SerializeField] [Tooltip("If set - chain blocks entire shelf (all slots) until broken")]
        private ChainController chainBlocker;
        [SerializeField] [Tooltip("VFX controller for set of 3 effect (star flies to score)")]
        private SetCollectedVfxController setCollectedVfx;

        private GameManager _gameManager;
        private AudioSource _audioSource;

        /// <summary>Chain blocking this shelf (all slots). Set manually or from ChainController.BlockedShelf</summary>
        public ChainController ChainBlocker => chainBlocker;

        public void SetChainBlocker(ChainController c) => chainBlocker = c;

        /// <summary>Slots of this shelf (for placing elements etc.)</summary>
        public IReadOnlyList<ShelfSlot> Slots => slots;

        private void Awake()
        {
            if (slots.Count == 0)
                slots.AddRange(GetComponentsInChildren<ShelfSlot>(true));
            if (config == null && GameManager.Instance != null)
                config = GameManager.Instance.Config;
            _gameManager = GameManager.Instance;
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();
            if (setCollectedVfx == null)
                setCollectedVfx = FindFirstObjectByType<SetCollectedVfxController>();
        }

        private void Start()
        {
            foreach (var slot in slots)
            {
                slot.SetShelf(this);
                var el = slot.GetComponentInChildren<ElementController>(true);
                if (el != null)
                {
                    slot.CurrentElement = el;
                    el.SetSlot(slot);
                }
            }
        }

        /// <summary>
        /// Called when clicking slot (e.g. blocked chain). Main movement is through drag in GameplayInput.
        /// </summary>
        public void OnSlotClicked(ShelfSlot slot)
        {
            GameManager gm = _gameManager ?? GameManager.Instance;
            if (gm?.IsGameOver == true) return;
            
            if (slot.IsBlocked)
                slot.ChainBlocker?.PlayTap();
        }

        /// <summary>
        /// Place item after dragging to target slot. Called from GameplayInput.
        /// </summary>
        public void PlaceElementFromDrag(ElementController el, ShelfSlot fromSlot, ShelfSlot toSlot)
        {
            fromSlot.CurrentElement = null;
            toSlot.CurrentElement = el;
            el.SetSlot(toSlot);

            el.transform.SetParent(toSlot.transform);
            el.transform.localPosition = Vector3.zero;
            el.SetOutline(false);

            bool hadMatch = CheckAndResolveMatches();
            (_gameManager ?? GameManager.Instance)?.NotifyMoveCompleted(hadMatch);
        }

        /// <summary>
        /// Return item to original slot after canceling drag.
        /// </summary>
        public void ReturnElementFromDrag(ElementController el, ShelfSlot originalSlot)
        {
            originalSlot.CurrentElement = el;
            el.SetSlot(originalSlot);

            el.transform.SetParent(originalSlot.transform);
            el.transform.localPosition = Vector3.zero;
            el.SetOutline(false);
        }

        internal bool CheckAndResolveMatches()
        {
            var byType = new Dictionary<ElementType, List<ShelfSlot>>();
            foreach (var slot in slots)
            {
                if (slot.CurrentElement == null) continue;
                ElementType t = slot.CurrentElement.Type;
                if (!byType.ContainsKey(t))
                    byType[t] = new List<ShelfSlot>();
                byType[t].Add(slot);
            }

            bool anyMatch = false;
            foreach (var kv in byType)
            {
                while (kv.Value.Count >= 3)
                {
                    anyMatch = true;
                    var threeSlots = new List<ShelfSlot>();
                    for (int i = 0; i < 3; i++)
                    {
                        ShelfSlot s = kv.Value[kv.Value.Count - 1];
                        kv.Value.RemoveAt(kv.Value.Count - 1);
                        threeSlots.Add(s);
                    }
                    
                    if (setCollectedVfx != null && threeSlots[0].CurrentElement != null && 
                        threeSlots[1].CurrentElement != null && threeSlots[2].CurrentElement != null)
                    {
                        setCollectedVfx.PlaySetCollected(
                            threeSlots[0].CurrentElement.transform,
                            threeSlots[1].CurrentElement.transform,
                            threeSlots[2].CurrentElement.transform
                        );
                    }
                    
                    foreach (var s in threeSlots)
                    {
                        SpawnMatchVfx(s.transform.position);
                        if (s.CurrentElement != null)
                        {
                            Destroy(s.CurrentElement.gameObject);
                            s.CurrentElement = null;
                        }
                    }
                    
                    PlayMatchSound();
                }
            }

            if (anyMatch)
                LevelFinishController.TryCheckLevelComplete();

            return anyMatch;
        }

        private void SpawnMatchVfx(Vector3 worldPos)
        {
            if (config?.matchVfxPrefab == null) return;
            Transform point = matchVfxSpawnPoint ?? transform;
            GameObject vfx = Instantiate(config.matchVfxPrefab, worldPos, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        private void PlayMatchSound()
        {
            if (_audioSource != null && config?.soundMatch != null && (GameSettings.Instance == null || GameSettings.Instance.SoundEffectsEnabled))
                _audioSource.PlayOneShot(config.soundMatch);
        }

        public void RegisterSlot(ShelfSlot slot)
        {
            if (slot != null && !slots.Contains(slot))
                slots.Add(slot);
        }
    }
}
