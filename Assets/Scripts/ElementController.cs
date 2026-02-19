using UnityEngine;

namespace SuperBanana
{
    /// <summary>
    /// Attached to element prefab (Element_Banana etc.). Stores type and reference to current slot.
    /// Type is set in inspector by prefab name or manually.
    /// Requires Collider2D for tap and drag - BoxCollider2D is added in Awake if missing.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ElementController : MonoBehaviour
    {
        [SerializeField] private ElementType type;
        public ElementType Type => type;

        public ShelfSlot CurrentSlot { get; private set; }

        public void SetSlot(ShelfSlot slot)
        {
            CurrentSlot = slot;
            SetOutline(false);
        }

        /// <summary>Element is chained (if its slot is blocked)</summary>
        public bool IsChained => CurrentSlot != null && CurrentSlot.IsBlocked;

        [Header("Outline Settings")]
        [SerializeField] [Tooltip("Outline size (scale of sprite duplicate, e.g. 1.03 = +3%). If 0, uses value from GameConfig")]
        [Range(0f, 1.2f)]
        private float outlineScale = 0f;
        
        [SerializeField] [Tooltip("Outline color. If clear, uses value from GameConfig")]
        private Color outlineColor = Color.clear;

        private GameObject _outlineObject;
        private SpriteRenderer _mainRenderer;
        private Material _outlineMaterialInstance;
        private static Shader _outlineShader;

        /// <summary>
        /// Show/hide white outline around element (when picked up).
        /// Uses sprite duplicate with increased scale and white overlay by alpha.
        /// </summary>
        public void SetOutline(bool show)
        {
            if (!show)
            {
                _outlineObject?.SetActive(false);
                return;
            }

            if (_outlineObject == null)
            {
                _mainRenderer = GetComponentInChildren<SpriteRenderer>();
                if (_mainRenderer?.sprite != null)
                    CreateOutlineDuplicate();
            }

            _outlineObject?.SetActive(true);
        }

        private void CreateOutlineDuplicate()
        {
            if (_mainRenderer == null)
                _mainRenderer = GetComponentInChildren<SpriteRenderer>();
                
            if (_mainRenderer?.sprite == null) return;

            float finalScale = outlineScale > 0 ? outlineScale : GetDefaultOutlineScale();
            Color finalColor = outlineColor.a > 0 ? outlineColor : GetDefaultOutlineColor();

            if (_outlineShader == null)
            {
                _outlineShader = Shader.Find("Universal Render Pipeline/2D/SpriteOutlineAlpha");
                if (_outlineShader == null)
                    Debug.LogWarning("SpriteOutlineAlpha shader not found! Using default sprite material.");
            }

            _outlineObject = new GameObject("Outline");
            _outlineObject.transform.SetParent(transform);
            _outlineObject.transform.localPosition = Vector3.zero;
            _outlineObject.transform.localScale = Vector3.one * finalScale;
            _outlineObject.transform.localRotation = Quaternion.identity;
            _outlineObject.transform.SetSiblingIndex(0);

            SpriteRenderer outlineRenderer = _outlineObject.AddComponent<SpriteRenderer>();
            outlineRenderer.sprite = _mainRenderer.sprite;
            outlineRenderer.color = Color.white;
            outlineRenderer.sortingOrder = _mainRenderer.sortingOrder - 1;
            
            if (_outlineShader != null)
            {
                if (_outlineMaterialInstance == null)
                    _outlineMaterialInstance = new Material(_outlineShader);
                
                _outlineMaterialInstance.SetColor("_OutlineColor", finalColor);
                _outlineMaterialInstance.SetTexture("_MainTex", _mainRenderer.sprite.texture);
                outlineRenderer.material = _outlineMaterialInstance;
            }
            else
            {
                outlineRenderer.material = _mainRenderer.material;
            }
            
            outlineRenderer.flipX = _mainRenderer.flipX;
            outlineRenderer.flipY = _mainRenderer.flipY;
        }

        /// <summary>
        /// Destroy element: clear slot, remove object. Called from boosters and gameplay.
        /// </summary>
        public void DestroyItem()
        {
            CurrentSlot.CurrentElement = null;

            LevelFinishController.TryCheckLevelComplete();

            if (_outlineMaterialInstance != null)
            {
                Destroy(_outlineMaterialInstance);
                _outlineMaterialInstance = null;
            }

            Destroy(gameObject);
        }
        
        /// <summary>
        /// Update outline parameters (if outline already created).
        /// </summary>
        public void UpdateOutlineSettings()
        {
            if (_outlineObject == null) return;

            float finalScale = outlineScale > 0 ? outlineScale : GetDefaultOutlineScale();
            Color finalColor = outlineColor.a > 0 ? outlineColor : GetDefaultOutlineColor();
            
            _outlineObject.transform.localScale = Vector3.one * finalScale;
            _outlineMaterialInstance?.SetColor("_OutlineColor", finalColor);
        }
        
        private float GetDefaultOutlineScale()
        {
            return GameManager.Instance?.Config?.defaultOutlineThickness ?? 1.03f;
        }
        
        private Color GetDefaultOutlineColor()
        {
            return GameManager.Instance?.Config?.defaultOutlineColor ?? Color.white;
        }

        private void Awake()
        {
            if (GetComponent<Collider2D>() == null)
            {
                var col = gameObject.AddComponent<BoxCollider2D>();
                var sr = GetComponentInChildren<SpriteRenderer>();
                col.size = sr?.sprite != null ? sr.sprite.bounds.size : Vector2.one * 0.5f;
            }
        }

        private void OnValidate()
        {
            if (type == 0 && gameObject != null)
            {
                string name = gameObject.name;
                if (name.Contains("Banana")) type = ElementType.Banana;
                else if (name.Contains("CoffeeCup")) type = ElementType.CoffeeCup;
                else if (name.Contains("Coral")) type = ElementType.Coral;
                else if (name.Contains("FlipFlop")) type = ElementType.FlipFlop;
                else if (name.Contains("Snack")) type = ElementType.Snack;
            }
            
            if (Application.isPlaying)
                UpdateOutlineSettings();
        }
    }
}
