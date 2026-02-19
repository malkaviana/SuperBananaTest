using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SuperBanana
{
    /// <summary>
    /// "Rocket" booster controller. On button press launches rocket that destroys random element and up to 2 adjacent.
    /// Attach to object in Canvas_Dynamic_Boosters. Button must be set in inspector.
    /// </summary>
    public class RocketBoosterController : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private RectTransform buttonRect;
        [SerializeField] private GameObject rocketProjectilePrefab;
        [SerializeField] private float zDepth = 0f;
        [SerializeField] private float flyTime = 1f;
        [SerializeField] private float arcHeight = 2f;

        private bool _busy;
        private RandomShelfPlacer _shelfPlacer;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();
            if (buttonRect == null)
                buttonRect = GetComponent<RectTransform>();
            
            button?.onClick.AddListener(OnButtonClicked);
        }

        private void Start()
        {
            if (GameManager.Instance != null)
                _shelfPlacer = FindFirstObjectByType<RandomShelfPlacer>();
        }

        private void OnButtonClicked()
        {
            if (_busy || button?.interactable == false) return;

            List<ShelfSlot> validSlots = CollectValidSlots();
            if (validSlots.Count == 0)
            {
                Debug.LogWarning("RocketBooster: No valid targets found");
                return;
            }

            ShelfSlot target = validSlots[Random.Range(0, validSlots.Count)];
            LaunchRocket(target);
        }

        private List<ShelfSlot> CollectValidSlots()
        {
            var validSlots = new List<ShelfSlot>();
            
            if (_shelfPlacer == null)
                _shelfPlacer = FindFirstObjectByType<RandomShelfPlacer>();
            
            if (_shelfPlacer?.Shelves == null)
                return validSlots;

            foreach (var shelf in _shelfPlacer.Shelves)
            {
                if (shelf == null) continue;
                foreach (var slot in shelf.Slots)
                {
                    if (slot?.HasItem() == true && !slot.CurrentElement.IsChained)
                        validSlots.Add(slot);
                }
            }

            return validSlots;
        }

        private void LaunchRocket(ShelfSlot target)
        {
            _busy = true;
            if (button != null)
                button.interactable = false;

            Vector3 startWorld = GetStartWorldPosition();
            Vector3 targetWorld = target.transform.position;

            GameObject rocketObj = Instantiate(rocketProjectilePrefab, startWorld, Quaternion.identity);
            var rocket = rocketObj.GetComponent<RocketProjectile>();
            
            if (rocket == null)
            {
                Debug.LogWarning("RocketBoosterController: RocketProjectile component not found on prefab! Adding component, but explosion sound may not work.");
                rocket = rocketObj.AddComponent<RocketProjectile>();
            }

            rocket.Initialize(startWorld, targetWorld, flyTime, arcHeight, () =>
            {
                OnRocketImpact(target);
                _busy = false;
                if (button != null)
                    button.interactable = true;
            });
        }

        private Vector3 GetStartWorldPosition()
        {
            if (buttonRect == null) return Vector3.zero;
            
            Camera cam = Camera.main ?? FindFirstObjectByType<Camera>();
            if (cam == null) return buttonRect.position;

            Canvas canvas = buttonRect.GetComponentInParent<Canvas>();
            if (canvas == null) return buttonRect.position;

            Vector3 worldPos;
            
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, buttonRect.position);
                float distance = Mathf.Abs(cam.transform.position.z - zDepth);
                Vector3 screen3 = new Vector3(screenPos.x, screenPos.y, distance);
                worldPos = cam.ScreenToWorldPoint(screen3);
            }
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Camera canvasCam = canvas.worldCamera ?? cam;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    buttonRect, 
                    RectTransformUtility.WorldToScreenPoint(canvasCam, buttonRect.position), 
                    canvasCam, 
                    out worldPos))
                {
                    worldPos.z = zDepth;
                }
                else
                {
                    worldPos = buttonRect.position;
                }
            }
            else
            {
                worldPos = buttonRect.position;
            }

            return worldPos;
        }

        private void OnRocketImpact(ShelfSlot target)
        {
            if (target?.HasItem() != true) return;

            ElementController targetItem = target.CurrentElement;
            if (targetItem == null) return;

            int shelfIndex = GetShelfIndex(target.Shelf);
            int slotIndex = GetSlotIndex(target);

            targetItem.DestroyItem();

            int destroyedCount = 0;
            const int maxAdditional = 2;

            if (shelfIndex < 0 || _shelfPlacer?.Shelves == null) return;

            ShelfController[] shelves = _shelfPlacer.Shelves;
            
            if (shelfIndex > 0 && shelfIndex - 1 < shelves.Length)
            {
                ShelfSlot adjSlot = FindSlotToDestroy(shelves[shelfIndex - 1], slotIndex);
                if (adjSlot != null && destroyedCount < maxAdditional)
                {
                    adjSlot.CurrentElement?.DestroyItem();
                    destroyedCount++;
                }
            }

            if (shelfIndex + 1 < shelves.Length)
            {
                ShelfSlot adjSlot = FindSlotToDestroy(shelves[shelfIndex + 1], slotIndex);
                if (adjSlot != null && destroyedCount < maxAdditional)
                {
                    adjSlot.CurrentElement?.DestroyItem();
                    destroyedCount++;
                }
            }
        }

        private int GetShelfIndex(ShelfController shelf)
        {
            if (_shelfPlacer?.Shelves == null) return -1;
            for (int i = 0; i < _shelfPlacer.Shelves.Length; i++)
            {
                if (_shelfPlacer.Shelves[i] == shelf)
                    return i;
            }
            return -1;
        }

        private int GetSlotIndex(ShelfSlot slot)
        {
            if (slot?.Shelf == null) return -1;
            var slots = slot.Shelf.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == slot)
                    return i;
            }
            return -1;
        }

        private ShelfSlot FindSlotToDestroy(ShelfController shelf, int preferredSlotIndex)
        {
            if (shelf == null) return null;

            var slots = shelf.Slots;
            if (preferredSlotIndex >= 0 && preferredSlotIndex < slots.Count)
            {
                ShelfSlot slot = slots[preferredSlotIndex];
                if (slot?.HasItem() == true && !slot.CurrentElement.IsChained)
                    return slot;
            }

            int closestIndex = -1;
            int minDistance = int.MaxValue;
            for (int i = 0; i < slots.Count; i++)
            {
                ShelfSlot slot = slots[i];
                if (slot?.HasItem() != true || slot.CurrentElement.IsChained) continue;
                
                int dist = Mathf.Abs(i - preferredSlotIndex);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestIndex = i;
                }
            }

            return closestIndex >= 0 ? slots[closestIndex] : null;
        }
    }
}
