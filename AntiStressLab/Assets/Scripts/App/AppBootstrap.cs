using AntiStressLab.Audio;
using AntiStressLab.Ads;
using AntiStressLab.Input;
using AntiStressLab.Slime;
using AntiStressLab.UI;
using UnityEngine;

namespace AntiStressLab.App
{
    /// <summary>
    /// Scene bootstrapper: builds the whole MVP scene in code.
    /// Keeps the scene file minimal and avoids manual setup.
    /// </summary>
    public sealed class AppBootstrap : MonoBehaviour
    {
        [Header("Slime")]
        [SerializeField] private SlimeSettings settings;

        private void Awake()
        {
            Application.targetFrameRate = 60;

            if (settings == null)
            {
                // Provide sane defaults even if no asset is assigned in the inspector.
                settings = ScriptableObject.CreateInstance<SlimeSettings>();
            }

            CreateCameraAndLight();

            var ads = CreateAds();
            var audio = CreateAudio();
            audio.SetMinInterval(settings.minSoundIntervalSeconds);
            var slime = CreateSlime(settings);
            var ui = CreateUI(slime, settings, ads);

            CreateInput(slime, audio, settings);
        }

        private static void CreateCameraAndLight()
        {
            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.06f, 0.06f, 0.07f, 1f);
                cam.orthographic = false;
                cam.fieldOfView = 45f;
                cam.nearClipPlane = 0.03f;
                cam.farClipPlane = 100f;
                camGo.transform.position = new Vector3(0f, 1.2f, -2.2f);
                camGo.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
            }

            if (Object.FindAnyObjectByType<Light>() == null)
            {
                var lightGo = new GameObject("Directional Light");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.1f;
                light.color = new Color(1f, 0.98f, 0.95f);
                lightGo.transform.rotation = Quaternion.Euler(55f, -30f, 0f);
            }
        }

        private static InteractionAudio CreateAudio()
        {
            var go = new GameObject("Audio");
            var audio = go.AddComponent<InteractionAudio>();
            return audio;
        }

        private static AdsManager CreateAds()
        {
            var go = new GameObject("Ads");
            var ads = go.AddComponent<AdsManager>();
            ads.Initialize();
            return ads;
        }

        private static SlimeController CreateSlime(SlimeSettings settings)
        {
            var go = new GameObject("Slime");
            go.transform.position = Vector3.zero;

            var controller = go.AddComponent<SlimeController>();
            controller.ApplySettings(settings);
            controller.Initialize();

            return controller;
        }

        private static SlimeUI CreateUI(SlimeController slime, SlimeSettings settings, IAdsService ads)
        {
            var go = new GameObject("UI");
            var ui = go.AddComponent<SlimeUI>();
            ui.Initialize(slime, settings, ads);
            return ui;
        }

        private static TouchInputRouter CreateInput(SlimeController slime, InteractionAudio audio, SlimeSettings settings)
        {
            var go = new GameObject("Input");
            var router = go.AddComponent<TouchInputRouter>();

            var interactor = go.AddComponent<SlimeInteractor>();
            interactor.Initialize(slime, audio, settings);

            router.Initialize(interactor);
            return router;
        }
    }
}

