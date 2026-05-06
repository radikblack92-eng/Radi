using System;

namespace AntiStressLab.Ads
{
    public interface IAdsService
    {
        bool IsInitialized { get; }

        void Initialize();

        /// <summary>
        /// Shows rewarded ad. Callback result indicates whether the reward should be granted.
        /// </summary>
        void ShowRewarded(Action<bool> onCompleted);

        void ShowInterstitial();

        /// <summary>
        /// Banner is intentionally optional for "vibe-safe" UX.
        /// </summary>
        void SetBannerVisible(bool visible);
    }
}

