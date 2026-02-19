using System.Collections.Generic;
using UnityEngine;

namespace SuperBanana
{
    /// <summary>
    /// Detects set of 3 items and calls VFX. Connect your set detection logic,
    /// then call OnSetDetected(transform1, transform2, transform3).
    /// Integrates with GameManager to award points after VFX completion.
    /// </summary>
    public class MatchSystem : MonoBehaviour
    {
        [SerializeField] private SetCollectedVfxController vfxController;
        [SerializeField] private int scorePerSet = 100;

        private void Awake()
        {
            if (vfxController == null)
                vfxController = FindFirstObjectByType<SetCollectedVfxController>();
        }

        /// <summary>
        /// Call when set of 3 detected (e.g. from game logic).
        /// Awards points through GameManager after VFX completion.
        /// </summary>
        public void OnSetDetected(Transform a, Transform b, Transform c)
        {
            if (vfxController == null)
            {
                Debug.LogError("MatchSystem: vfxController == null! Assign SetCollectedVfxController in Inspector.");
                return;
            }
            vfxController.PlaySetCollected(a, b, c);
        }

        /// <summary>
        /// Call when set of 3 detected (list).
        /// </summary>
        public void OnSetDetected(IReadOnlyList<Transform> threeItems)
        {
            if (vfxController == null || threeItems == null || threeItems.Count < 3) return;
            vfxController.PlaySetCollected(threeItems);
        }
    }
}
