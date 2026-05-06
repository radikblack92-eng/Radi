using UnityEngine;

namespace AntiStressLab.Ads
{
    [CreateAssetMenu(menuName = "AntiStress Lab/Ads Config", fileName = "AdsConfig")]
    public sealed class AdsConfig : ScriptableObject
    {
        [Header("Yandex Mobile Ads Unit IDs")]
        public string bannerUnitId = "";
        public string interstitialUnitId = "";
        public string rewardedUnitId = "";

        [Header("Frequency caps (seconds)")]
        [Min(30f)] public float interstitialCooldownSeconds = 120f;
        [Min(0.01f)] public float rewardedCooldownSeconds = 15f;

        [Header("UX defaults")]
        public bool bannerEnabledByDefault = false;
    }
}

