#if YANDEX_MOBILE_ADS
using System;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

namespace AntiStressLab.Ads
{
    /// <summary>
    /// Yandex Mobile Ads implementation (Unity plugin).
    /// Enable by adding scripting define symbol: YANDEX_MOBILE_ADS
    /// </summary>
    public sealed class YandexAdsService : MonoBehaviour, IAdsService
    {
        private AdsConfig _config;

        private RewardedAdLoader _rewardedLoader;
        private RewardedAd _rewarded;
        private Action<bool> _pendingRewardCallback;

        public bool IsInitialized { get; private set; }

        public void Configure(AdsConfig config) => _config = config;

        public void Initialize()
        {
            if (IsInitialized) return;
            IsInitialized = true;

            _rewardedLoader = new RewardedAdLoader();
            PreloadRewarded();
        }

        public void ShowRewarded(Action<bool> onCompleted)
        {
            _pendingRewardCallback = onCompleted;

            if (_rewarded == null)
            {
                onCompleted?.Invoke(false);
                PreloadRewarded();
                return;
            }

            _rewarded.Show();
        }

        public void ShowInterstitial()
        {
            // TODO: add InterstitialAdLoader + InterstitialAd when you provide interstitial Ad Unit ID.
        }

        public void SetBannerVisible(bool visible)
        {
            // TODO: add BannerAdView when you provide banner Ad Unit ID.
        }

        private void PreloadRewarded()
        {
            if (_rewardedLoader == null) _rewardedLoader = new RewardedAdLoader();

            string unitId = _config != null ? _config.rewardedUnitId : null;
            if (string.IsNullOrWhiteSpace(unitId))
            {
                Debug.LogWarning("YandexAds: rewardedUnitId is empty. Using demo-rewarded-yandex for testing.");
                unitId = "demo-rewarded-yandex";
            }

            var req = new AdRequest(unitId);
            _rewardedLoader.LoadAd(
                adRequest: req,
                onLoaded: HandleRewardedLoaded,
                onFailed: args =>
                {
                    Debug.LogWarning("YandexAds: Rewarded failed to load: " + args.Message);
                    // Avoid retry loops; user can retry by tapping later.
                });
        }

        private void HandleRewardedLoaded(RewardedAd ad)
        {
            DestroyRewarded();
            _rewarded = ad;

            _rewarded.OnRewarded += OnRewarded;
            _rewarded.OnAdDismissed += OnDismissed;
            _rewarded.OnAdFailedToShow += OnFailedToShow;
        }

        private void OnRewarded(object sender, Reward args)
        {
            _pendingRewardCallback?.Invoke(true);
            _pendingRewardCallback = null;
        }

        private void OnDismissed(object sender, EventArgs args)
        {
            if (_pendingRewardCallback != null)
            {
                // User closed without reward event -> no reward.
                _pendingRewardCallback(false);
                _pendingRewardCallback = null;
            }

            DestroyRewarded();
            PreloadRewarded();
        }

        private void OnFailedToShow(object sender, AdFailureEventArgs args)
        {
            Debug.LogWarning("YandexAds: Rewarded failed to show: " + args.Message);
            _pendingRewardCallback?.Invoke(false);
            _pendingRewardCallback = null;

            DestroyRewarded();
            PreloadRewarded();
        }

        private void DestroyRewarded()
        {
            if (_rewarded == null) return;

            _rewarded.OnRewarded -= OnRewarded;
            _rewarded.OnAdDismissed -= OnDismissed;
            _rewarded.OnAdFailedToShow -= OnFailedToShow;

            _rewarded.Destroy();
            _rewarded = null;
        }

        private void OnDestroy()
        {
            DestroyRewarded();
        }
    }
}
#else
// If the Yandex plugin isn't installed, this file compiles as an empty placeholder.
namespace AntiStressLab.Ads { }
#endif

