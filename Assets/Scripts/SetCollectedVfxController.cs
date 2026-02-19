using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SuperBanana
{
    /// <summary>
    /// "Set of 3 collected" effect: items disappear → star flies to score.
    /// Canvas = Screen Space – Overlay; camera = null in ScreenPointToLocalPointInRectangle.
    /// </summary>
    public class SetCollectedVfxController : MonoBehaviour
    {
        [Header("Canvas (Screen Space – Overlay)")]
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private RectTransform scoreTarget;
        [SerializeField] private Sprite starSprite;

        [Header("Timing")]
        [SerializeField] private float disappearDuration = 0.2f;
        [SerializeField] private float starPopInDelay = 0.12f;
        [SerializeField] private float starPopInTime = 0.08f;
        [SerializeField] private float starFlyTime = 0.45f;
        [SerializeField] private float impactScaleDuration = 0.12f;

        [Header("Arc & Scale")]
        [SerializeField] private float arcHeight = 70f;
        [SerializeField] private float impactScalePeak = 1.15f;

        private Camera _mainCam;

        private void Awake()
        {
            _mainCam = Camera.main;
            if (canvasRect == null)
                canvasRect = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Start effect. Pass list of 3 Transform (world-space).
        /// </summary>
        public void PlaySetCollected(IReadOnlyList<Transform> threeItems)
        {
            if (threeItems == null || threeItems.Count < 3)
            {
                Debug.LogWarning($"SetCollectedVfxController: Need 3 items. Got: {threeItems?.Count ?? 0}");
                return;
            }
            if (canvasRect == null)
            {
                Debug.LogError("SetCollectedVfxController: canvasRect not assigned! Assign Canvas RectTransform in Inspector.");
                return;
            }
            if (scoreTarget == null)
            {
                Debug.LogError("SetCollectedVfxController: scoreTarget not assigned! Assign score element RectTransform in Inspector.");
                return;
            }
            if (starSprite == null)
            {
                Debug.LogError("SetCollectedVfxController: starSprite not assigned! Assign star Sprite in Inspector.");
                return;
            }

            Vector3 center = (threeItems[0].position + threeItems[1].position + threeItems[2].position) / 3f;
            StartCoroutine(PlaySequence(threeItems, center));
        }

        public void PlaySetCollected(Transform a, Transform b, Transform c)
        {
            var list = new List<Transform> { a, b, c };
            PlaySetCollected(list);
        }

        private IEnumerator PlaySequence(IReadOnlyList<Transform> items, Vector3 worldCenter)
        {
            yield return AnimateDisappear(items);

            float waitTime = Mathf.Max(0f, starPopInDelay - disappearDuration);
            yield return new WaitForSeconds(waitTime);

            Vector2 localCenter = WorldToCanvasLocal(worldCenter);
            RectTransform starRect = SpawnStar(localCenter);
            if (starRect == null)
                yield break;

            yield return AnimateStarPopIn(starRect);

            Vector2 targetAnchored = GetScoreTargetPosition();
            yield return AnimateStarFly(starRect, targetAnchored);

            GameManager.Instance?.AddMatch();
            StartCoroutine(PlayScoreImpact());
            Destroy(starRect.gameObject);
        }

        private Vector2 GetScoreTargetPosition()
        {
            if (scoreTarget == null) return Vector2.zero;
            
            Vector3[] worldCorners = new Vector3[4];
            scoreTarget.GetWorldCorners(worldCorners);
            Vector3 centerWorld = (worldCorners[0] + worldCorners[2]) / 2f;
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, centerWorld);
            
            bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                null,
                out Vector2 localPos
            );
            
            if (!success)
            {
                if (scoreTarget.parent == canvasRect)
                {
                    localPos = scoreTarget.anchoredPosition;
                }
                else
                {
                    RectTransform parent = scoreTarget.parent as RectTransform;
                    Vector2 pos = scoreTarget.anchoredPosition;
                    while (parent != null && parent != canvasRect)
                    {
                        pos += parent.anchoredPosition;
                        parent = parent.parent as RectTransform;
                    }
                    localPos = pos;
                }
            }
            
            return localPos;
        }

        private Vector2 WorldToCanvasLocal(Vector3 worldPos)
        {
            if (_mainCam == null) _mainCam = Camera.main;
            if (_mainCam == null)
            {
                Debug.LogError("SetCollectedVfxController: Camera.main not found!");
                return Vector2.zero;
            }
            
            Vector3 screenPos = _mainCam.WorldToScreenPoint(worldPos);
            
            if (screenPos.z < 0)
                screenPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            
            bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, 
                new Vector2(screenPos.x, screenPos.y), 
                null, 
                out Vector2 localPos
            );
            
            if (!success)
                localPos = Vector2.zero;
            
            return localPos;
        }

        private RectTransform SpawnStar(Vector2 localPos)
        {
            var go = new GameObject("StarVfx");
            go.transform.SetParent(canvasRect, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = localPos;
            rect.sizeDelta = new Vector2(80f, 80f);
            rect.localScale = Vector3.zero;

            var image = go.AddComponent<Image>();
            image.sprite = starSprite;
            image.raycastTarget = false;
            
            var canvasGroup = go.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            return rect;
        }

        private IEnumerator AnimateDisappear(IReadOnlyList<Transform> items)
        {
            float elapsed = 0f;
            var startScales = new Dictionary<Transform, Vector3>();
            var canvasGroups = new List<CanvasGroup>();
            var spriteRenderers = new List<SpriteRenderer>();

            foreach (Transform t in items)
            {
                if (t == null) continue;
                startScales[t] = t.localScale;
                var cg = t.GetComponent<CanvasGroup>();
                if (cg != null) canvasGroups.Add(cg);
                else if (t.GetComponent<SpriteRenderer>() is SpriteRenderer sr) spriteRenderers.Add(sr);
            }

            while (elapsed < disappearDuration)
            {
                elapsed += Time.deltaTime;
                float k = Mathf.Clamp01(elapsed / disappearDuration);
                k = EaseInBack(k);

                foreach (var kv in startScales)
                {
                    if (kv.Key != null)
                        kv.Key.localScale = kv.Value * (1f - k);
                }
                float alpha = 1f - k;
                foreach (var cg in canvasGroups)
                {
                    if (cg != null) cg.alpha = alpha;
                }
                foreach (var sr in spriteRenderers)
                {
                    if (sr != null)
                    {
                        var c = sr.color;
                        c.a = alpha;
                        sr.color = c;
                    }
                }
                yield return null;
            }

            foreach (Transform t in items)
            {
                if (t != null)
                    t.gameObject.SetActive(false);
            }
        }

        private IEnumerator AnimateStarPopIn(RectTransform starRect)
        {
            float elapsed = 0f;
            while (elapsed < starPopInTime)
            {
                elapsed += Time.deltaTime;
                float k = Mathf.Clamp01(elapsed / starPopInTime);
                starRect.localScale = Vector3.one * (0.9f * EaseOutBack(k));
                yield return null;
            }
            starRect.localScale = Vector3.one * 0.9f;
        }

        private IEnumerator AnimateStarFly(RectTransform starRect, Vector2 targetAnchored)
        {
            Vector2 start = starRect.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < starFlyTime)
            {
                elapsed += Time.deltaTime;
                float k = Mathf.Clamp01(elapsed / starFlyTime);
                k = EaseInOutQuad(k);
                Vector2 pos = Vector2.Lerp(start, targetAnchored, k);
                pos.y += Mathf.Sin(k * Mathf.PI) * arcHeight;
                starRect.anchoredPosition = pos;
                yield return null;
            }
            starRect.anchoredPosition = targetAnchored;
        }

        private IEnumerator PlayScoreImpact()
        {
            if (scoreTarget == null) yield break;
            Vector3 baseScale = scoreTarget.localScale;
            float elapsed = 0f;

            while (elapsed < impactScaleDuration)
            {
                elapsed += Time.deltaTime;
                float k = Mathf.Clamp01(elapsed / impactScaleDuration);
                float scale = Mathf.Lerp(1f, impactScalePeak, k <= 0.5f ? k * 2f : 2f - k * 2f);
                scoreTarget.localScale = baseScale * scale;
                yield return null;
            }
            scoreTarget.localScale = baseScale;
        }

        private static float EaseInBack(float t) => t * t * (2.7f * t - 1.7f);
        private static float EaseOutBack(float t) => 1f - (1f - t) * (1f - t) * (2.7f * (1f - t) - 1.7f);
        private static float EaseInOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }
}
