#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;

/// <summary>
/// Synchronizes the active Build Profile BE Home environment override into the runtime settings asset for the duration of a build.
/// </summary>
internal sealed class BeHomeBuildProfileEnvironmentSync :
    IPreprocessBuildWithReport,
    IPostprocessBuildWithReport
{
    private static bool s_hasOriginalEnvironment;
    private static bool s_didMutateRuntimeSettings;
    private static BeHomeTargetEnvironment s_originalEnvironment;

    /// <inheritdoc />
    public int callbackOrder => 0;

    /// <inheritdoc />
    public void OnPreprocessBuild(BuildReport report)
    {
        var settings = BeHomeProjectSettings.Load();
        if (settings == null)
        {
            return;
        }

        var activeBuildProfile = BuildProfile.GetActiveBuildProfile();
        var profileSettings = activeBuildProfile != null
            ? activeBuildProfile.GetComponent<BeHomeBuildProfileSettings>()
            : null;
        if (profileSettings == null)
        {
            ResetTrackedState();
            return;
        }

        s_originalEnvironment = settings.TargetEnvironment;
        s_hasOriginalEnvironment = true;
        s_didMutateRuntimeSettings = settings.TargetEnvironment != profileSettings.TargetEnvironment;

        if (!s_didMutateRuntimeSettings)
        {
            return;
        }

        settings.SetTargetEnvironment(profileSettings.TargetEnvironment);
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }

    /// <inheritdoc />
    public void OnPostprocessBuild(BuildReport report)
    {
        RestoreOriginalEnvironment();
    }

    private static void RestoreOriginalEnvironment()
    {
        if (!s_hasOriginalEnvironment || !s_didMutateRuntimeSettings)
        {
            ResetTrackedState();
            return;
        }

        var settings = BeHomeProjectSettings.Load();
        if (settings != null)
        {
            settings.SetTargetEnvironment(s_originalEnvironment);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        ResetTrackedState();
    }

    private static void ResetTrackedState()
    {
        s_hasOriginalEnvironment = false;
        s_didMutateRuntimeSettings = false;
        s_originalEnvironment = BeHomeTargetEnvironment.Production;
    }
}
#endif
