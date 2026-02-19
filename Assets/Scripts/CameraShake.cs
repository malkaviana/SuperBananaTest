using UnityEngine;
using System.Collections;

namespace SuperBanana
{
    /// <summary>
    /// Simple camera shake effect script.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        private static CameraShake _instance;
        private Camera _camera;
        private Vector3 _basePosition;
        private Coroutine _shakeCoroutine;
        private bool _isShaking;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                _camera = GetComponent<Camera>() ?? Camera.main;
            }
            else if (_instance != this)
            {
                Destroy(this);
            }
        }

        private void OnEnable()
        {
            if (_camera == null)
                _camera = Camera.main;
            
            UpdateBasePosition();
        }

        private void LateUpdate()
        {
            if (!_isShaking && _camera != null)
            {
                UpdateBasePosition();
            }
        }

        private void UpdateBasePosition()
        {
            if (_camera != null)
                _basePosition = _camera.transform.position;
        }

        /// <summary>
        /// Start shake effect.
        /// </summary>
        /// <param name="duration">Shake duration in seconds</param>
        /// <param name="magnitude">Shake strength</param>
        public static void Shake(float duration, float magnitude)
        {
            if (_instance?._camera == null) return;

            if (_instance._shakeCoroutine != null)
            {
                _instance.StopCoroutine(_instance._shakeCoroutine);
                _instance._camera.transform.position = _instance._basePosition;
            }
            
            _instance.UpdateBasePosition();
            _instance._shakeCoroutine = _instance.StartCoroutine(_instance.DoShake(duration, magnitude));
        }

        private IEnumerator DoShake(float duration, float magnitude)
        {
            if (_camera == null)
                _camera = Camera.main;
            
            if (_camera == null)
                yield break;

            _isShaking = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                _camera.transform.position = _basePosition + new Vector3(x, y, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            _camera.transform.position = _basePosition;
            _isShaking = false;
            _shakeCoroutine = null;
        }
    }
}
