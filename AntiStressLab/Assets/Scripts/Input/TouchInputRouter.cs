using UnityEngine;

namespace AntiStressLab.Input
{
    /// <summary>
    /// Forwards all active touches to the receiver (multi-touch: each fingerId is independent).
    /// </summary>
    public sealed class TouchInputRouter : MonoBehaviour
    {
        private ISlimeInputReceiver _receiver;

        public void Initialize(ISlimeInputReceiver receiver) => _receiver = receiver;

        private void Update()
        {
            if (_receiver == null) return;

            // If touchCount > 0 we used to skip the mouse entirely. On many Windows machines Unity
            // still reports touches while the user sculpts with a mouse in the Game view — then
            // only TouchPhase.Began-style events fire and it feels like "tap only, no drag".
            // When the left mouse button is used this frame, prefer the mouse path.
            bool mouseLeftThisFrame =
                UnityEngine.Input.GetMouseButton(0) ||
                UnityEngine.Input.GetMouseButtonDown(0) ||
                UnityEngine.Input.GetMouseButtonUp(0);

            if (UnityEngine.Input.touchCount > 0 && !mouseLeftThisFrame)
            {
                for (int i = 0; i < UnityEngine.Input.touchCount; i++)
                {
                    var t = UnityEngine.Input.GetTouch(i);
                    switch (t.phase)
                    {
                        case TouchPhase.Began:
                            _receiver.OnPress(t.fingerId, t.position);
                            break;
                        case TouchPhase.Moved:
                        case TouchPhase.Stationary:
                            _receiver.OnDrag(t.fingerId, t.position);
                            break;
                        case TouchPhase.Ended:
                        case TouchPhase.Canceled:
                            _receiver.OnRelease(t.fingerId, t.position);
                            break;
                    }
                }
                return;
            }

            // Mouse (Editor / desktop, or mouse in Game view while touch hardware is present)
            const int mouseId = 0;
            if (UnityEngine.Input.GetMouseButtonDown(0)) _receiver.OnPress(mouseId, UnityEngine.Input.mousePosition);
            if (UnityEngine.Input.GetMouseButton(0)) _receiver.OnDrag(mouseId, UnityEngine.Input.mousePosition);
            if (UnityEngine.Input.GetMouseButtonUp(0)) _receiver.OnRelease(mouseId, UnityEngine.Input.mousePosition);
        }
    }

    public interface ISlimeInputReceiver
    {
        void OnPress(int pointerId, Vector2 screenPos);
        void OnDrag(int pointerId, Vector2 screenPos);
        void OnRelease(int pointerId, Vector2 screenPos);
    }
}

