using System;
using System.Collections;
using UnityEngine;

namespace AntiStressLab.Ads
{
    /// <summary>
    /// Editor-safe mock: simulates ads without SDK.
    /// Helps iterate on UX without external dependencies.
    /// </summary>
    public sealed class EditorFakeAdsService : MonoBehaviour, IAdsService
    {
        public bool IsInitialized { get; private set; }

        public void Initialize() => IsInitialized = true;

        public void ShowRewarded(Action<bool> onCompleted)
        {
            if (!IsInitialized) Initialize();
            StartCoroutine(FakeRewardRoutine(onCompleted));
        }

        public void ShowInterstitial()
        {
            if (!IsInitialized) Initialize();
            Debug.Log("Ads (Editor): interstitial would show.");
        }

        public void SetBannerVisible(bool visible)
        {
            Debug.Log("Ads (Editor): banner visible = " + visible);
        }

        private static IEnumerator FakeRewardRoutine(Action<bool> onCompleted)
        {
            // Small delay to emulate an ad.
            yield return new WaitForSecondsRealtime(0.35f);
            onCompleted?.Invoke(true);
        }
    }
}

