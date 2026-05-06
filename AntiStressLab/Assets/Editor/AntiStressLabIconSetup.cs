using System.Linq;
using UnityEditor;
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

            // Default icons
            var groups = PlayerSettings.GetSupportedBuildTargetGroups();
            foreach (var g in groups)
            {
                if (g == BuildTargetGroup.Unknown) continue;
                TrySetIconsForGroup(g, tex);
            }

            // Android adaptive icons (optional; we just set legacy ones here)
            TrySetIconsForGroup(BuildTargetGroup.Android, tex);

            SessionState.SetBool(DoneKey, true);
            Debug.Log("Антистресс Вайб: применены иконка и название (" + IconPath + ")");
        }

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

