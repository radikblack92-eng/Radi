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
                // In MVP we default to Editor fake unless a real implementation is added.
                // You can replace this with a Yandex implementation behind a define symbol later.
                _impl = gameObject.AddComponent<EditorFakeAdsService>();
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

