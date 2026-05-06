using AntiStressLab.Audio;
using AntiStressLab.Slime;
using UnityEngine;

namespace AntiStressLab.Input
{
    /// <summary>
    /// Maps pointer events to slime interactions (tap dent vs drag pull).
    /// </summary>
    public sealed class SlimeInteractor : MonoBehaviour, ISlimeInputReceiver
    {
        private SlimeController _slime;
        private InteractionAudio _audio;
        private SlimeSettings _settings;

        private int _activePointer = int.MinValue;
        private bool _moved;
        private Vector2 _pressScreenPos;

        public void Initialize(SlimeController slime, InteractionAudio audio, SlimeSettings settings)
        {
            _slime = slime;
            _audio = audio;
            _settings = settings;
        }

        public void OnPress(int pointerId, Vector2 screenPos)
        {
            _activePointer = pointerId;
            _moved = false;
            _pressScreenPos = screenPos;

            TryRaycastAndDent(screenPos);
        }

        public void OnDrag(int pointerId, Vector2 screenPos)
        {
            if (pointerId != _activePointer) return;
            if ((screenPos - _pressScreenPos).sqrMagnitude > 12f * 12f) _moved = true;

            TryRaycastAndPull(screenPos);
        }

        public void OnRelease(int pointerId, Vector2 screenPos)
        {
            if (pointerId != _activePointer) return;

            // If it didn't move much, it's basically a tap (already dented on press).
            // If it did move, last drag already applied pull.
            _activePointer = int.MinValue;
        }

        private void TryRaycastAndDent(Vector2 screenPos)
        {
            if (_slime == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            var ray = cam.ScreenPointToRay(screenPos);
            if (_slime.Raycast(ray, out var hit))
            {
                _slime.TapDeform(hit.point);
                _audio?.TryPlayInteract(_settings != null ? _settings.interactionVolume : 0.25f);
            }
        }

        private void TryRaycastAndPull(Vector2 screenPos)
        {
            if (_slime == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            var ray = cam.ScreenPointToRay(screenPos);
            if (_slime.Raycast(ray, out var hit))
            {
                _slime.DragDeform(hit.point);
                _audio?.TryPlayInteract(_settings != null ? _settings.interactionVolume : 0.25f);
            }
        }
    }
}

