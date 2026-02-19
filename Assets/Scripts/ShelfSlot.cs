using UnityEngine;
using UnityEngine.EventSystems;

namespace SuperBanana
{
    /// <summary>
    /// Single shelf slot. Acts as button: on click notifies ShelfController.
    /// Must be attached to object with Collider2D (or Raycast receiver in UI).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ShelfSlot : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private ShelfController shelf;

        public ShelfController Shelf => shelf;
        /// <summary>Chain blocking this slot's shelf (if any). PlayTap is called on it when tapping blocked slot</summary>
        public ChainController ChainBlocker => shelf?.ChainBlocker;
        /// <summary>Slot is blocked if shelf is blocked by chain (chain blocks all shelf slots)</summary>
        public bool IsBlocked => shelf?.ChainBlocker?.IsBlocking == true;

        public ElementController CurrentElement { get; set; }

        /// <summary>Has item in slot (not null and not blocked by chain)</summary>
        public bool HasItem() => CurrentElement != null && !IsBlocked;

        private void Reset()
        {
            shelf = GetComponentInParent<ShelfController>();
        }

        public void SetShelf(ShelfController s) => shelf = s;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (EventSystem.current?.currentSelectedGameObject != null)
                return;
            shelf?.OnSlotClicked(this);
        }

        private void OnMouseDown()
        {
            shelf?.OnSlotClicked(this);
        }
    }
}
