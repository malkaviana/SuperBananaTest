using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SuperBanana
{
    /// <summary>
    /// Tap on item - grab, while button held item follows cursor (scale 1.2).
    /// On release: if empty slot under cursor - place there, otherwise return to original position.
    /// Attach to camera or GameManager. Items and slots must have Collider2D.
    /// Works with new Input System (Input System package).
    /// </summary>
    public class GameplayInput : MonoBehaviour
    {
        [SerializeField] private Camera raycastCamera;
        [Tooltip("Item scale when grabbed and dragged")]
        [SerializeField] private float dragScale = 1.2f;

        private ElementController _draggedElement;
        private ShelfSlot _draggedOriginalSlot;
        private Vector3 _draggedOriginalScale;
        private bool _isDragging;
        private AudioSource _audioSource;

        private void Awake()
        {
            if (raycastCamera == null)
                raycastCamera = Camera.main;
            
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void Update()
        {
            if (raycastCamera == null) return;
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;
                TryStartDrag();
            }

            if (_isDragging)
            {
                Vector2 world = GetMouseWorldPosition();
                _draggedElement.transform.position = new Vector3(world.x, world.y, _draggedElement.transform.position.z);
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                if (_isDragging)
                    EndDrag();
            }
        }

        private Vector2 GetMouseWorldPosition()
        {
            Vector2 screen = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            Vector3 screen3 = new Vector3(screen.x, screen.y, -raycastCamera.transform.position.z);
            return raycastCamera.ScreenToWorldPoint(screen3);
        }

        private void TryStartDrag()
        {
            Vector2 world = GetMouseWorldPosition();
            Collider2D[] hits = Physics2D.OverlapPointAll(world);
            
            if (hits.Length == 0) return;

            foreach (Collider2D hit in hits)
            {
                if (hit == null || !hit.enabled) continue;
                
                var element = hit.GetComponent<ElementController>() ?? hit.GetComponentInParent<ElementController>();
                
                if (element != null)
                {
                    if (element.CurrentSlot == null)
                    {
                        var parentSlot = element.GetComponentInParent<ShelfSlot>();
                        if (parentSlot == null) continue;
                        
                        parentSlot.CurrentElement = element;
                        element.SetSlot(parentSlot);
                    }
                    
                    if (element.CurrentSlot.IsBlocked)
                    {
                        element.CurrentSlot.ChainBlocker?.PlayTap();
                        return;
                    }
                    
                    _draggedElement = element;
                    _draggedOriginalSlot = element.CurrentSlot;
                    _draggedOriginalScale = element.transform.localScale;
                    element.transform.localScale = _draggedOriginalScale * dragScale;
                    element.SetOutline(true);
                    _isDragging = true;
                    
                    PlayPickupSound();
                    return;
                }
            }

            foreach (Collider2D hit in hits)
            {
                if (hit == null || !hit.enabled) continue;
                
                var slot = hit.GetComponent<ShelfSlot>();
                if (slot?.IsBlocked == true)
                    slot.ChainBlocker?.PlayTap();
            }
        }

        private void EndDrag()
        {
            if (_draggedElement == null || _draggedOriginalSlot == null)
            {
                _isDragging = false;
                _draggedElement = null;
                _draggedOriginalSlot = null;
                return;
            }

            Vector2 world = GetMouseWorldPosition();
            ShelfSlot targetSlot = GetSlotAtPosition(world, _draggedElement.transform);

            bool placed = false;
            if (targetSlot != null
                && !targetSlot.IsBlocked
                && targetSlot.CurrentElement == null
                && targetSlot != _draggedOriginalSlot)
            {
                targetSlot.Shelf.PlaceElementFromDrag(_draggedElement, _draggedOriginalSlot, targetSlot);
                placed = true;
            }

            if (!placed)
                _draggedOriginalSlot.Shelf.ReturnElementFromDrag(_draggedElement, _draggedOriginalSlot);
            else
                PlayPlaceSound();

            _draggedElement.SetOutline(false);
            _draggedElement.transform.localScale = _draggedOriginalScale;
            _draggedElement = null;
            _draggedOriginalSlot = null;
            _isDragging = false;
        }

        private void PlayPickupSound()
        {
            if (_audioSource == null) return;
            if (GameSettings.Instance != null && !GameSettings.Instance.SoundEffectsEnabled) return;
            GameConfig config = GameManager.Instance?.Config;
            if (config?.soundPickup != null)
                _audioSource.PlayOneShot(config.soundPickup);
        }

        private void PlayPlaceSound()
        {
            if (_audioSource == null) return;
            if (GameSettings.Instance != null && !GameSettings.Instance.SoundEffectsEnabled) return;
            GameConfig config = GameManager.Instance?.Config;
            if (config?.soundPlace != null)
                _audioSource.PlayOneShot(config.soundPlace);
        }

        /// <summary>
        /// Finds slot at position (ignoring dragged object's collider).
        /// </summary>
        private static ShelfSlot GetSlotAtPosition(Vector2 worldPos, Transform ignoreTransform)
        {
            Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);
            foreach (Collider2D c in hits)
            {
                if (ignoreTransform != null && (c.transform == ignoreTransform || c.transform.IsChildOf(ignoreTransform)))
                    continue;
                
                var slot = c.GetComponent<ShelfSlot>();
                if (slot != null)
                    return slot;
            }
            return null;
        }
    }
}
