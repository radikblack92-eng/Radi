using System.Linq;
using UnityEditor;
#if UNITY_6000_0_OR_NEWER
using UnityEditor.Build;
#endif
using UnityEngine;

namespace AntiStressLab.Editor
{
    /// <summary>
    /// One-time helper: assigns app icon from Assets/Art/Icons/app_icon.png.
    /// Runs in the Unity Editor after scripts compile.
    /// </summary>
    [InitializeOnLoad]
    public static class AntiStressLabIconSetup
    {
        private const string IconPath = "Assets/Art/Icons/app_icon.png";
        private const string DoneKey = "AntiStressLab.IconSetup.Done";
        private const string ProductName = "Антистресс Вайб";

        static AntiStressLabIconSetup()
        {
            EditorApplication.delayCall += TryApply;
        }

        private static void TryApply()
        {
            if (SessionState.GetBool(DoneKey, false)) return;

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (tex == null) return;

            PlayerSettings.productName = ProductName;

            // Set icons for a few common targets. (Unity 6 removed some legacy APIs.)
#if UNITY_6000_0_OR_NEWER
            TrySetIconsForTarget(NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Android), tex);
            TrySetIconsForTarget(NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Standalone), tex);
#else
            TrySetIconsForGroup(BuildTargetGroup.Android, tex);
            TrySetIconsForGroup(BuildTargetGroup.Standalone, tex);
#endif

            SessionState.SetBool(DoneKey, true);
            Debug.Log("Антистресс Вайб: применены иконка и название (" + IconPath + ")");
        }

#if UNITY_6000_0_OR_NEWER
        private static void TrySetIconsForTarget(NamedBuildTarget target, Texture2D tex)
        {
            var icons = PlayerSettings.GetIcons(target, IconKind.Application);
            if (icons == null || icons.Length == 0)
            {
                PlayerSettings.SetIcons(target, new[] { tex }, IconKind.Application);
                return;
            }

            var filled = icons.Select(_ => (Texture2D)tex).ToArray();
            PlayerSettings.SetIcons(target, filled, IconKind.Application);
        }
#endif

        private static void TrySetIconsForGroup(BuildTargetGroup group, Texture2D tex)
        {
            var icons = PlayerSettings.GetIconsForTargetGroup(group);
            if (icons == null || icons.Length == 0)
            {
                PlayerSettings.SetIconsForTargetGroup(group, new[] { tex });
                return;
            }

            // Fill all required slots with the same texture
            var filled = icons.Select(_ => (Texture2D)tex).ToArray();
            PlayerSettings.SetIconsForTargetGroup(group, filled);
        }
    }
}

