#if UNITY_EDITOR
using UnityEditor.Build.Profile;

using UnityEngine;

/// <summary>
/// Build-profile-specific BE Home website targeting settings.
/// </summary>
public sealed class BeHomeBuildProfileSettings : ScriptableObject
{
    [SerializeField]
    private BeHomeTargetEnvironment m_targetEnvironment = BeHomeTargetEnvironment.Production;

    /// <summary>
    /// Gets the hosted website environment override configured for the owning build profile.
    /// </summary>
    public BeHomeTargetEnvironment TargetEnvironment => m_targetEnvironment;

    /// <summary>
    /// Creates the Build Profile window section for BE Home website environment overrides.
    /// </summary>
    /// <returns>The provider used by Unity to display the section.</returns>
    [BuildProfileSettingsProvider(typeof(BeHomeBuildProfileSettings))]
    public static BuildProfileSettingsProvider CreateSettingsProvider()
    {
        return new BuildProfileSettingsProvider("BE Home")
        {
            tooltip = "Overrides which hosted BE website environment this build profile bakes into BE Home at build time.",
            hasCustomEditor = false,
            canAddSetting = buildProfile => buildProfile != null && buildProfile.GetComponent<BeHomeBuildProfileSettings>() == null,
        };
    }
}
#endif
