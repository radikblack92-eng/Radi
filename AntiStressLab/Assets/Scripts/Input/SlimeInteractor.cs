using System.Collections.Generic;
using AntiStressLab.Audio;
using AntiStressLab.Slime;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AntiStressLab.Input
{
    /// <summary>
    /// Maps pointer events to slime interactions (tap dent vs drag pull).
    /// Supports multiple simultaneous touches (each fingerId has its own grab state).
    /// </summary>
    public sealed class SlimeInteractor : MonoBehaviour, ISlimeInputReceiver
    {
        private SlimeController _slime;
        private InteractionAudio _audio;
        private SlimeSettings _settings;

        private struct PointerState
        {
            public bool HasLastHit;
            public Vector3 LastHitPoint;
        }

        private readonly Dictionary<int, PointerState> _pointers = new(8);

        public void Initialize(SlimeController slime, InteractionAudio audio, SlimeSettings settings)
        {
            _slime = slime;
            _audio = audio;
            _settings = settings;
        }

        public void OnPress(int pointerId, Vector2 screenPos)
        {
            if (IsPointerOverUi(screenPos)) return;

            _pointers[pointerId] = new PointerState { HasLastHit = false };
            TryRaycastAndDent(screenPos, pointerId);
        }

        public void OnDrag(int pointerId, Vector2 screenPos)
        {
            if (IsPointerOverUi(screenPos)) return;
            if (!_pointers.ContainsKey(pointerId))
                _pointers[pointerId] = new PointerState { HasLastHit = false };

            TryRaycastAndPull(screenPos, pointerId);
        }

        public void OnRelease(int pointerId, Vector2 screenPos)
        {
            _pointers.Remove(pointerId);
        }

        private static bool IsPointerOverUi(Vector2 screenPos)
        {
            if (EventSystem.current == null) return false;
            var ped = new PointerEventData(EventSystem.current) { position = screenPos };
            var results = new List<RaycastResult>(8);
            EventSystem.current.RaycastAll(ped, results);
            return results.Count > 0;
        }

        private void TryRaycastAndDent(Vector2 screenPos, int pointerId)
        {
            if (_slime == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            var ray = cam.ScreenPointToRay(screenPos);
            if (_slime.Raycast(ray, out var hit))
            {
                _slime.ClayIndent(hit.point, hit.normal);
                _audio?.TryPlayInteract(_settings != null ? _settings.interactionVolume : 0.25f);

                var st = _pointers[pointerId];
                st.LastHitPoint = hit.point;
                st.HasLastHit = true;
                _pointers[pointerId] = st;
            }
        }

        private void TryRaycastAndPull(Vector2 screenPos, int pointerId)
        {
            if (_slime == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            var ray = cam.ScreenPointToRay(screenPos);
            if (!_slime.Raycast(ray, out var hit)) return;

            var st = _pointers[pointerId];
            if (st.HasLastHit)
            {
                Vector3 delta = hit.point - st.LastHitPoint;
                _slime.ClayGrabDelta(hit.point, delta);
            }
            else
            {
                _slime.DragDeform(hit.point);
            }

            _audio?.TryPlayInteract(_settings != null ? _settings.interactionVolume : 0.25f);
            st.LastHitPoint = hit.point;
            st.HasLastHit = true;
            _pointers[pointerId] = st;
        }
    }
}

