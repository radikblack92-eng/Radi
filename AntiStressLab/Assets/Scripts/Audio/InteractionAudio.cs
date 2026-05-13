using UnityEngine;

namespace AntiStressLab.Audio
{
    /// <summary>
    /// Simple audio feedback for interactions.
    /// Uses a procedurally generated one-shot to avoid external assets.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InteractionAudio : MonoBehaviour
    {
        private AudioSource _src;
        private AudioClip _clip;
        private float _lastPlayTime;

        [SerializeField] private float minIntervalSeconds = 0.07f;

        private void Awake()
        {
            _src = gameObject.AddComponent<AudioSource>();
            _src.playOnAwake = false;
            _src.spatialBlend = 0f;
            _src.loop = false;
            _src.volume = 1f;

            _clip = ProceduralPloop.Create();
        }

        public void SetMinInterval(float seconds)
        {
            minIntervalSeconds = Mathf.Max(0f, seconds);
        }

        public void TryPlayInteract(float volume)
        {
            if (volume <= 0f) return;

            float now = Time.unscaledTime;
            if (now - _lastPlayTime < minIntervalSeconds) return;
            _lastPlayTime = now;

            if (_clip == null) _clip = ProceduralPloop.Create();
            _src.PlayOneShot(_clip, Mathf.Clamp01(volume));
        }
    }
}

