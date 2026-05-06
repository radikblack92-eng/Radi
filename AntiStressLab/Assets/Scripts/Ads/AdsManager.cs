using System;
using UnityEngine;

namespace AntiStressLab.Ads
{
    /// <summary>
    /// Central point for ads + vibe-safe throttling.
    /// Real Yandex SDK integration is wrapped behind compilation symbols to keep the project buildable.
    /// </summary>
    public sealed class AdsManager : MonoBehaviour, IAdsService
    {
        [SerializeField] private AdsConfig config;

        private IAdsService _impl;
        private float _lastInterstitialTime = -99999f;
        private float _lastRewardedTime = -99999f;

        public bool IsInitialized => _impl != null && _impl.IsInitialized;

        public void Initialize()
        {
            if (_impl == null)
            {
                if (config == null)
                {
                    config = ScriptableObject.CreateInstance<AdsConfig>();
                    // Default: use your current rewarded unit id. Add the other IDs later.
                    config.rewardedUnitId = "R-M-19232510-1";
                    config.interstitialCooldownSeconds = 150f;
                    config.rewardedCooldownSeconds = 15f;
                    config.bannerEnabledByDefault = false;
                }

#if YANDEX_MOBILE_ADS
                var yandex = gameObject.AddComponent<YandexAdsService>();
                yandex.Configure(config);
                _impl = yandex;
#else
                // Safe fallback when the Yandex plugin isn't installed yet.
                _impl = gameObject.AddComponent<EditorFakeAdsService>();
#endif
            }

            _impl.Initialize();

            if (config != null && config.bannerEnabledByDefault)
            {
                _impl.SetBannerVisible(true);
            }
        }

        public void ShowRewarded(Action<bool> onCompleted)
        {
            if (!IsInitialized) Initialize();

            float cd = config != null ? Mathf.Max(0.01f, config.rewardedCooldownSeconds) : 15f;
            if (Time.unscaledTime - _lastRewardedTime < cd)
            {
                onCompleted?.Invoke(false);
                return;
            }

            _lastRewardedTime = Time.unscaledTime;
            _impl.ShowRewarded(onCompleted);
        }

        public void ShowInterstitial()
        {
            if (!IsInitialized) Initialize();

            float cd = config != null ? Mathf.Max(30f, config.interstitialCooldownSeconds) : 120f;
            if (Time.unscaledTime - _lastInterstitialTime < cd) return;

            _lastInterstitialTime = Time.unscaledTime;
            _impl.ShowInterstitial();
        }

        public void SetBannerVisible(bool visible)
        {
            if (!IsInitialized) Initialize();
            _impl.SetBannerVisible(visible);
        }
    }
}

