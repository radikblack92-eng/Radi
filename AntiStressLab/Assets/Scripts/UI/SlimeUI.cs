using AntiStressLab.Slime;
using AntiStressLab.Ads;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AntiStressLab.UI
{
    /// <summary>
    /// Minimal UI: RGB sliders + reset button.
    /// Built in code to keep the MVP setup frictionless.
    /// </summary>
    public sealed class SlimeUI : MonoBehaviour
    {
        private SlimeController _slime;
        private SlimeSettings _settings;
        private IAdsService _ads;

        private Slider _r;
        private Slider _g;
        private Slider _b;
        private Image _preview;

        public void Initialize(SlimeController slime, SlimeSettings settings, IAdsService ads = null)
        {
            _slime = slime;
            _settings = settings;
            _ads = ads;

            EnsureEventSystem();
            var canvas = CreateCanvas();

            var panel = CreatePanel(canvas.transform);
            _preview = CreatePreview(panel.transform);

            _r = CreateSlider(panel.transform, "R", 0);
            _g = CreateSlider(panel.transform, "G", 1);
            _b = CreateSlider(panel.transform, "B", 2);

            var buttonsRow = CreateButtonsRow(panel.transform);
            var bonus = CreateButton(buttonsRow, "Бонус", kind: "BonusButton");
            var reset = CreateButton(buttonsRow, "Сброс", kind: "ResetButton");

            Color start = _slime != null ? _slime.GetColor() : (_settings != null ? _settings.initialColor : Color.white);
            SetSlidersFromColor(start);
            ApplyColorFromSliders();

            _r.onValueChanged.AddListener(_ => ApplyColorFromSliders());
            _g.onValueChanged.AddListener(_ => ApplyColorFromSliders());
            _b.onValueChanged.AddListener(_ => ApplyColorFromSliders());

            reset.onClick.AddListener(() =>
            {
                _slime?.ResetSlime();
                var c = _settings != null ? _settings.initialColor : start;
                SetSlidersFromColor(c);
                ApplyColorFromSliders();
            });

            bonus.onClick.AddListener(() =>
            {
                if (_ads == null)
                {
                    // No ads configured yet; keep UX: grant the bonus anyway in MVP.
                    GrantBonus();
                    return;
                }

                _ads.ShowRewarded(granted =>
                {
                    if (granted) GrantBonus();
                });
            });
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private static Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static RectTransform CreatePanel(Transform parent)
        {
            var go = new GameObject("Panel");
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 24f);
            rt.sizeDelta = new Vector2(680f, 210f);

            var img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.35f);

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 14, 14);
            layout.spacing = 10f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            go.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rt;
        }

        private static Image CreatePreview(Transform parent)
        {
            var go = new GameObject("Preview");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = Color.white;

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 32f);
            return img;
        }

        private static Slider CreateSlider(Transform parent, string label, int row)
        {
            var root = new GameObject($"Slider{label}");
            root.transform.SetParent(parent, false);
            var rootRt = root.AddComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(0f, 46f);

            var h = root.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 12f;
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childForceExpandHeight = true;
            h.childForceExpandWidth = true;

            var text = CreateText(root.transform, label, 0.18f);
            text.alignment = TextAnchor.MiddleCenter;

            var sliderGo = new GameObject("Slider");
            sliderGo.transform.SetParent(root.transform, false);
            var slider = sliderGo.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.5f;

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(sliderGo.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(1f, 1f, 1f, 0.18f);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.25f);
            bgRt.anchorMax = new Vector2(1f, 0.75f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            // Fill area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGo.transform, false);
            var faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0f, 0.25f);
            faRt.anchorMax = new Vector2(1f, 0.75f);
            faRt.offsetMin = new Vector2(10f, 0f);
            faRt.offsetMax = new Vector2(-10f, 0f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.9f, 0.9f, 0.9f, 0.75f);
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(1f, 1f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;

            // Handle
            var handle = new GameObject("Handle");
            handle.transform.SetParent(sliderGo.transform, false);
            var handleImg = handle.AddComponent<Image>();
            handleImg.color = new Color(1f, 1f, 1f, 0.95f);
            var handleRt = handle.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(20f, 40f);

            slider.targetGraphic = handleImg;
            slider.fillRect = fillRt;
            slider.handleRect = handleRt;

            // Layout widths
            sliderGo.AddComponent<LayoutElement>().flexibleWidth = 1f;
            text.gameObject.AddComponent<LayoutElement>().preferredWidth = 42f;

            return slider;
        }

        private static Button CreateButton(Transform parent, string title, int row)
        {
            return CreateButton(parent, title, kind: "Button");
        }

        private static RectTransform CreateButtonsRow(Transform parent)
        {
            var go = new GameObject("Buttons");
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 46f);

            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 12f;
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childForceExpandHeight = true;
            h.childForceExpandWidth = true;

            return rt;
        }

        private static Button CreateButton(Transform parent, string title, string kind)
        {
            var go = new GameObject(kind);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.12f);

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.20f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0.28f);
            btn.colors = colors;

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 44f);

            var text = CreateText(go.transform, title, 1f);
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 1f, 1f, 0.92f);

            return btn;
        }

        private static Text CreateText(Transform parent, string content, float stretch)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.text = content;
            t.fontSize = 20;
            t.color = new Color(1f, 1f, 1f, 0.85f);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return t;
        }

        private void SetSlidersFromColor(Color c)
        {
            if (_r != null) _r.value = c.r;
            if (_g != null) _g.value = c.g;
            if (_b != null) _b.value = c.b;
        }

        private void ApplyColorFromSliders()
        {
            if (_slime == null) return;
            var c = new Color(_r.value, _g.value, _b.value, 1f);
            _slime.SetColor(c);
            if (_preview != null) _preview.color = c;
        }

        private void GrantBonus()
        {
            if (_slime == null) return;

            // "Vibe-safe" reward: a pleasant random pastel color.
            float h = Random.value;
            float s = 0.35f + Random.value * 0.25f;
            float v = 0.85f + Random.value * 0.12f;
            Color c = Color.HSVToRGB(h, s, v);

            SetSlidersFromColor(c);
            ApplyColorFromSliders();
        }
    }
}

