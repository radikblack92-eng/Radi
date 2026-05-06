using UnityEditor;
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
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, AndroidAppId);
        }

        private static void ApplyAndroidBuildSettings()
        {
            // Scripting backend / architectures
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

            // ARM64 required for stores; ARMv7 optional (add later if you want broader device support)
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // SDK levels
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)MinSdk;

            // Keep target SDK up-to-date on the machine that builds the release.
            // Unity will map this to the highest installed Android API.
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // Quality-of-life defaults for release builds
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Low);
        }
    }
}

