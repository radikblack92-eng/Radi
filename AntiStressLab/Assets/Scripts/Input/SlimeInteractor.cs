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
        private Vector2 _pressScreenPos;
        private bool _hasLastHit;
        private Vector3 _lastHitPoint;

        public void Initialize(SlimeController slime, InteractionAudio audio, SlimeSettings settings)
        {
            _slime = slime;
            _audio = audio;
            _settings = settings;
        }

        public void OnPress(int pointerId, Vector2 screenPos)
        {
            _activePointer = pointerId;
            _pressScreenPos = screenPos;
            _hasLastHit = false;

            TryRaycastAndDent(screenPos);
        }

        public void OnDrag(int pointerId, Vector2 screenPos)
        {
            if (pointerId != _activePointer) return;

            TryRaycastAndPull(screenPos);
        }

        public void OnRelease(int pointerId, Vector2 screenPos)
        {
            if (pointerId != _activePointer) return;

            // If it didn't move much, it's basically a tap (already dented on press).
            // If it did move, last drag already applied pull.
            _activePointer = int.MinValue;
            _hasLastHit = false;
        }

        private void TryRaycastAndDent(Vector2 screenPos)
        {
            if (_slime == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            var ray = cam.ScreenPointToRay(screenPos);
            if (_slime.Raycast(ray, out var hit))
            {
                // Clay uses surface normal for realistic indentation.
                _slime.ClayIndent(hit.point, hit.normal);
                _audio?.TryPlayInteract(_settings != null ? _settings.interactionVolume : 0.25f);
                _lastHitPoint = hit.point;
                _hasLastHit = true;
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
                if (_hasLastHit)
                {
                    Vector3 delta = hit.point - _lastHitPoint;
                    _slime.ClayGrabDelta(hit.point, delta);
                }
                else
                {
                    _slime.DragDeform(hit.point);
                }
                _audio?.TryPlayInteract(_settings != null ? _settings.interactionVolume : 0.25f);
                _lastHitPoint = hit.point;
                _hasLastHit = true;
            }
        }
    }
}

