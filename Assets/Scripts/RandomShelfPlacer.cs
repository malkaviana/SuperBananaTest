using System.Collections.Generic;
using UnityEngine;

namespace SuperBanana
{
    /// <summary>
    /// At round start fills shelf slots randomly: two sets of three elements per type
    /// (6 elements of each of 5 types = 30 elements). Places so no three identical in a row in one shelf.
    /// Attach to object in scene. Set shelves and element prefabs by type.
    /// </summary>
    public class RandomShelfPlacer : MonoBehaviour
    {
        [SerializeField] [Tooltip("Shelves to fill (3 slots each)")]
        private ShelfController[] shelves;
        [SerializeField] [Tooltip("Element prefabs by type (index = ElementType)")]
        private GameObject[] elementPrefabsByType;
        [Tooltip("How many sets of three per type (2 = 6 elements per type)")]
        [SerializeField] private int setsPerType = 2;

        private const int ElementsPerSet = 3;
        private const int TypeCount = 5; // Banana, CoffeeCup, Coral, FlipFlop, Snack

        /// <summary>Array of shelves for access from boosters and other systems</summary>
        public ShelfController[] Shelves => shelves;

        private void Start()
        {
            if (shelves == null || shelves.Length == 0) return;
            if (elementPrefabsByType == null || elementPrefabsByType.Length < TypeCount) return;

            List<ShelfSlot> allSlots = new List<ShelfSlot>();
            foreach (var shelf in shelves)
            {
                if (shelf == null) continue;
                for (int i = 0; i < shelf.Slots.Count; i++)
                    allSlots.Add(shelf.Slots[i]);
            }

            int totalNeeded = TypeCount * setsPerType * ElementsPerSet; // 5 * 2 * 3 = 30
            if (allSlots.Count < totalNeeded) return;

            ClearSlots(allSlots);

            ElementType[] types = BuildAndShuffleTypeList(setsPerType);
            FixNoTripleInSameShelf(shelves, types);
            PlaceElements(allSlots, types, totalNeeded);
        }

        private void ClearSlots(List<ShelfSlot> slots)
        {
            foreach (var slot in slots)
            {
                slot.CurrentElement = null;
                for (int i = slot.transform.childCount - 1; i >= 0; i--)
                {
                    var child = slot.transform.GetChild(i);
                    if (child.GetComponent<ElementController>() != null)
                        Destroy(child.gameObject);
                }
            }
        }

        /// <summary>Type list: (setsPerType * 3) of each type, shuffled</summary>
        private static ElementType[] BuildAndShuffleTypeList(int setsPerType)
        {
            int perType = setsPerType * ElementsPerSet;
            var list = new List<ElementType>();
            for (int t = 0; t < TypeCount; t++)
            {
                var type = (ElementType)t;
                for (int i = 0; i < perType; i++)
                    list.Add(type);
            }
            Shuffle(list);
            return list.ToArray();
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>Rearrange types so no three identical in one shelf</summary>
        private static void FixNoTripleInSameShelf(ShelfController[] shelves, ElementType[] types)
        {
            int maxIterations = 200;
            for (int iter = 0; iter < maxIterations; iter++)
            {
                int slotIndex = 0;
                bool anyTriple = false;
                foreach (var shelf in shelves)
                {
                    if (shelf == null || shelf.Slots.Count < 3) continue;
                    int s0 = slotIndex;
                    int s1 = slotIndex + 1;
                    int s2 = slotIndex + 2;
                    slotIndex += 3;
                    if (s2 >= types.Length) break;
                    if (types[s0] == types[s1] && types[s1] == types[s2])
                    {
                        anyTriple = true;
                        int swapWith = Random.Range(0, types.Length);
                        while (swapWith == s0 || swapWith == s1 || swapWith == s2)
                            swapWith = Random.Range(0, types.Length);
                        (types[s1], types[swapWith]) = (types[swapWith], types[s1]);
                    }
                }
                if (!anyTriple) break;
            }
        }

        private void PlaceElements(List<ShelfSlot> allSlots, ElementType[] types, int count)
        {
            for (int i = 0; i < count && i < allSlots.Count; i++)
            {
                ShelfSlot slot = allSlots[i];
                ElementType type = types[i];
                if (type < 0 || (int)type >= elementPrefabsByType.Length) continue;
                GameObject prefab = elementPrefabsByType[(int)type];
                if (prefab == null) continue;

                GameObject go = Instantiate(prefab, slot.transform);
                go.transform.localPosition = Vector3.zero;
                var el = go.GetComponent<ElementController>();
                if (el != null)
                {
                    slot.CurrentElement = el;
                    el.SetSlot(slot);
                }
            }
        }
    }
}
