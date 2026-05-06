using UnityEditor;
#if UNITY_6000_0_OR_NEWER
using UnityEditor.Build;
#endif
using UnityEngine;

namespace AntiStressLab.Editor
{
    /// <summary>
    /// One-time helper: applies Android/RuStore-friendly Player Settings.
    /// </summary>
    [InitializeOnLoad]
    public static class AntiStressLabAndroidSetup
    {
        private const string DoneKey = "AntiStressLab.AndroidSetup.Done";

        private const string AndroidAppId = "ru.radikblack.antistressvibe";
        private const int MinSdk = 24; // Android 7.0

        static AntiStressLabAndroidSetup()
        {
            EditorApplication.delayCall += TryApply;
        }

        private static void TryApply()
        {
            if (SessionState.GetBool(DoneKey, false)) return;

            ApplyIdentifiers();
            ApplyAndroidBuildSettings();

            SessionState.SetBool(DoneKey, true);
            Debug.Log("Антистресс Вайб: применены Android настройки (package name, IL2CPP, ARM64).");
        }

        private static void ApplyIdentifiers()
        {
#if UNITY_6000_0_OR_NEWER
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Android), AndroidAppId);
#else
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, AndroidAppId);
#endif
        }

        private static void ApplyAndroidBuildSettings()
        {
            // Scripting backend / architectures
#if UNITY_6000_0_OR_NEWER
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Android), ScriptingImplementation.IL2CPP);
#else
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
#endif

            // ARM64 required for stores; ARMv7 optional (add later if you want broader device support)
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // SDK levels
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)MinSdk;

            // Keep target SDK up-to-date on the machine that builds the release.
            // Unity will map this to the highest installed Android API.
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // Quality-of-life defaults for release builds
            PlayerSettings.stripEngineCode = true;
#if UNITY_6000_0_OR_NEWER
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Android), ManagedStrippingLevel.Low);
#else
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Low);
#endif
        }
    }
}

