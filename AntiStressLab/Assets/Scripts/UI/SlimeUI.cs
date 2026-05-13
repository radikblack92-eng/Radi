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
            CreateHeader(panel.transform);
            _preview = CreatePreview(panel.transform);

            _r = CreateSlider(panel.transform, "R", new Color(0.95f, 0.35f, 0.4f, 0.9f));
            _g = CreateSlider(panel.transform, "G", new Color(0.35f, 0.85f, 0.55f, 0.9f));
            _b = CreateSlider(panel.transform, "B", new Color(0.4f, 0.55f, 0.98f, 0.9f));

            var buttonsRow = CreateButtonsRow(panel.transform);
            var bonus = CreateButton(buttonsRow, "Бонус", kind: "BonusButton", primary: false);
            var reset = CreateButton(buttonsRow, "Сброс", kind: "ResetButton", primary: true);

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
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            // Mobile-friendly defaults
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
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
            rt.anchoredPosition = new Vector2(0f, 20f);
            rt.sizeDelta = new Vector2(560f, 0f);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.12f, 0.12f, 0.2f, 0.92f);

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.55f, 0.45f, 0.85f, 0.45f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            shadow.effectDistance = new Vector2(0f, -6f);

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 20, 18);
            layout.spacing = 14f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            go.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rt;
        }

        private static void CreateHeader(Transform parent)
        {
            var row = new GameObject("Header");
            row.transform.SetParent(parent, false);
            var rowRt = row.AddComponent<RectTransform>();
            rowRt.sizeDelta = new Vector2(0f, 64f);
            row.AddComponent<LayoutElement>().preferredHeight = 64f;

            var v = row.AddComponent<VerticalLayoutGroup>();
            v.spacing = 4f;
            v.childAlignment = TextAnchor.UpperCenter;
            v.childControlHeight = true;
            v.childControlWidth = true;
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = true;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(row.transform, false);
            var title = titleGo.AddComponent<Text>();
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.text = "Антистресс Вайб";
            title.fontSize = 22;
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = new Color(0.96f, 0.95f, 0.99f, 1f);
            titleGo.AddComponent<LayoutElement>().preferredHeight = 30f;
            var trt = titleGo.GetComponent<RectTransform>();
            trt.sizeDelta = new Vector2(0f, 30f);

            var subGo = new GameObject("Subtitle");
            subGo.transform.SetParent(row.transform, false);
            var st = subGo.AddComponent<Text>();
            st.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            st.text = "Лепи и растягивай — как пластилин";
            st.fontSize = 13;
            st.color = new Color(0.75f, 0.74f, 0.82f, 0.95f);
            st.alignment = TextAnchor.MiddleCenter;
            subGo.AddComponent<LayoutElement>().preferredHeight = 22f;
            var srt = subGo.GetComponent<RectTransform>();
            srt.sizeDelta = new Vector2(0f, 22f);
        }

        private static Image CreatePreview(Transform parent)
        {
            var go = new GameObject("Preview");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = Color.white;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.6f, 0.55f, 0.9f, 0.5f);
            outline.effectDistance = new Vector2(1f, -1f);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 36f);
            go.AddComponent<LayoutElement>().preferredHeight = 36f;
            return img;
        }

        private static Slider CreateSlider(Transform parent, string label, Color fillColor)
        {
            var root = new GameObject($"Slider{label}");
            root.transform.SetParent(parent, false);
            var rootRt = root.AddComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(0f, 48f);

            var h = root.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 14f;
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childForceExpandHeight = true;
            h.childForceExpandWidth = true;

            var text = CreateText(root.transform, label, 0.18f);
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(0.92f, 0.92f, 0.96f, 1f);

            var sliderGo = new GameObject("Slider");
            sliderGo.transform.SetParent(root.transform, false);
            var slider = sliderGo.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.5f;

            var bg = new GameObject("Background");
            bg.transform.SetParent(sliderGo.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.08f, 0.12f, 0.85f);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.22f);
            bgRt.anchorMax = new Vector2(1f, 0.78f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGo.transform, false);
            var faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0f, 0.22f);
            faRt.anchorMax = new Vector2(1f, 0.78f);
            faRt.offsetMin = new Vector2(8f, 0f);
            faRt.offsetMax = new Vector2(-8f, 0f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = fillColor;
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(1f, 1f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;

            var handle = new GameObject("Handle");
            handle.transform.SetParent(sliderGo.transform, false);
            var handleImg = handle.AddComponent<Image>();
            handleImg.color = new Color(0.98f, 0.98f, 1f, 1f);
            var handleRt = handle.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(20f, 30f);

            slider.targetGraphic = handleImg;
            slider.fillRect = fillRt;
            slider.handleRect = handleRt;

            sliderGo.AddComponent<LayoutElement>().flexibleWidth = 1f;
            text.gameObject.AddComponent<LayoutElement>().preferredWidth = 36f;

            return slider;
        }

        private static Button CreateButton(Transform parent, string title, string kind, bool primary)
        {
            var go = new GameObject(kind);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            if (primary)
            {
                img.color = new Color(0.45f, 0.72f, 0.68f, 0.95f);
            }
            else
            {
                img.color = new Color(0.22f, 0.22f, 0.32f, 0.95f);
            }

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.95f, 0.95f, 0.98f, 1f);
            colors.pressedColor = new Color(0.88f, 0.88f, 0.92f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = primary
                ? new Color(0.35f, 0.55f, 0.52f, 0.6f)
                : new Color(0.45f, 0.4f, 0.65f, 0.45f);
            outline.effectDistance = new Vector2(1f, -1f);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 48f);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 48f;
            le.flexibleWidth = 1f;

            var text = CreateText(go.transform, title, 1f);
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 16;
            text.fontStyle = FontStyle.Bold;
            text.color = primary ? new Color(0.08f, 0.1f, 0.12f, 1f) : new Color(0.95f, 0.94f, 0.99f, 1f);

            return btn;
        }

        private static RectTransform CreateButtonsRow(Transform parent)
        {
            var go = new GameObject("Buttons");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 52f);
            go.AddComponent<LayoutElement>().preferredHeight = 52f;

            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 14f;
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childForceExpandHeight = true;
            h.childForceExpandWidth = true;

            return rt;
        }

        private static Text CreateText(Transform parent, string content, float stretch)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text = content;
            t.fontSize = 18;
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

