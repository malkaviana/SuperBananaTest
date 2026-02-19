using System;
using UnityEngine;

namespace SuperBanana
{
    /// <summary>
    /// Rocket flying in arc to target. Automatically rotates in movement direction.
    /// On reaching target spawns VFX, plays sound and invokes callback.
    /// VFX trail should be configured manually on rocket prefab.
    /// </summary>
    public class RocketProjectile : MonoBehaviour
    {
        [SerializeField] private GameObject explosionVfxPrefab;
        [SerializeField] private AudioClip explosionSfx;
        [SerializeField] [Tooltip("If null, uses sound from GameConfig")]
        private AudioClip overrideExplosionSfx;
        [SerializeField] private float rotationSpeed = 10f;
        [Tooltip("Rotation offset in degrees to compensate initial sprite orientation. E.g. if sprite rotated 45° right-up, set -45")]
        [SerializeField] private float rotationOffset = 0f;

        private Vector3 _startPos;
        private Vector3 _targetPos;
        private float _flyTime;
        private float _arcHeight;
        private float _elapsedTime;
        private Action _onImpact;
        private AudioSource _audioSource;
        private Vector3 _lastPosition;
        private bool _hasImpacted;

        public void Initialize(Vector3 start, Vector3 target, float flyTime, float arcHeight, Action onImpact)
        {
            _startPos = start;
            _targetPos = target;
            _flyTime = flyTime;
            _arcHeight = arcHeight;
            _onImpact = onImpact;
            _elapsedTime = 0f;
            _hasImpacted = false;
            transform.position = start;
            _lastPosition = start;

            EnsureAudioSource();

            Vector3 initialDirection = (target - start).normalized;
            if (initialDirection.sqrMagnitude > 0.01f)
            {
                float initialAngle = Mathf.Atan2(initialDirection.y, initialDirection.x) * Mathf.Rad2Deg + rotationOffset;
                transform.rotation = Quaternion.AngleAxis(initialAngle, Vector3.forward);
            }
        }

        private void EnsureAudioSource()
        {
            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();
            
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.spatialBlend = 0f;
            }
        }

        private void Update()
        {
            if (_hasImpacted) return;

            if (_elapsedTime >= _flyTime)
            {
                OnReachTarget();
                return;
            }

            _elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsedTime / _flyTime);

            Vector3 linearPos = Vector3.Lerp(_startPos, _targetPos, t);
            float arc = Mathf.Sin(t * Mathf.PI) * _arcHeight;
            Vector3 arcOffset = Vector3.up * arc;
            Vector3 newPosition = linearPos + arcOffset;
            
            Vector3 movementDirection = (newPosition - _lastPosition).normalized;
            if (movementDirection.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg + rotationOffset;
                Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            transform.position = newPosition;
            _lastPosition = newPosition;
        }

        private void OnReachTarget()
        {
            if (_hasImpacted) return;
            _hasImpacted = true;

            transform.position = _targetPos;

            // Stop all particle systems (trail VFX) on rocket
            StopAllParticleSystems();

            if (explosionVfxPrefab != null)
            {
                GameObject vfx = Instantiate(explosionVfxPrefab, _targetPos, Quaternion.identity);
                Destroy(vfx, 3f);
            }

            AudioClip soundToPlay = overrideExplosionSfx ?? explosionSfx ?? GetDefaultExplosionSound();
            
            if (soundToPlay != null && (GameSettings.Instance == null || GameSettings.Instance.SoundEffectsEnabled))
            {
                AudioSource.PlayClipAtPoint(soundToPlay, _targetPos);
            }

            CameraShake.Shake(0.3f, 0.075f);

            _onImpact?.Invoke();
            
            // Destroy immediately since sound plays independently
            Destroy(gameObject, 0.1f);
        }

        private void StopAllParticleSystems()
        {
            // Stop particle systems on this object
            ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particles)
            {
                if (ps != null && ps.isPlaying)
                {
                    ps.Stop();
                }
            }
        }
        
        private AudioClip GetDefaultExplosionSound()
        {
            return GameManager.Instance?.Config?.soundRocketExplosion;
        }
    }
}
