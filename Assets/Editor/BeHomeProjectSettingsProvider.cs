#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditor.Build.Profile;

using UnityEngine;

/// <summary>
/// Provides the Project Settings UI for configuring BE Home build-time runtime behavior.
/// </summary>
internal static class BeHomeProjectSettingsProvider
{
    private const string SettingsPath = "Project/BE Home";
    private const string SettingsAssetPath = "Assets/Resources/Settings/BeHomeProjectSettings.asset";

    /// <summary>
    /// Creates the custom Project Settings entry for BE Home.
    /// </summary>
    /// <returns>The configured settings provider instance.</returns>
    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        return new SettingsProvider(SettingsPath, SettingsScope.Project)
        {
            label = "BE Home",
            guiHandler = _ => DrawSettingsGui(),
            keywords = new HashSet<string>
            {
                "Board Enthusiasts",
                "BE Home",
                "browse",
                "embed",
                "environment",
                "staging",
                "production",
                "website",
                "sign in",
            },
        };
    }

    /// <summary>
    /// Loads the maintained settings asset, creating it when the project does not yet contain one.
    /// </summary>
    /// <returns>The settings asset instance.</returns>
    private static BeHomeProjectSettings LoadOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<BeHomeProjectSettings>(SettingsAssetPath);
        if (settings != null)
        {
            return settings;
        }

        var settingsDirectory = Path.GetDirectoryName(SettingsAssetPath);
        if (!string.IsNullOrEmpty(settingsDirectory))
        {
            Directory.CreateDirectory(settingsDirectory);
        }

        settings = ScriptableObject.CreateInstance<BeHomeProjectSettings>();
        settings.name = nameof(BeHomeProjectSettings);

        AssetDatabase.CreateAsset(settings, SettingsAssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return settings;
    }

    /// <summary>
    /// Draws the BE Home Project Settings editor UI.
    /// </summary>
    private static void DrawSettingsGui()
    {
        var settings = LoadOrCreateSettings();
        var serializedSettings = new SerializedObject(settings);

        serializedSettings.Update();

        EditorGUILayout.HelpBox(
            "Choose which hosted BE website environment BE Home targets, and whether it loads the full hosted site "
            + "or the Board-specific embedded shell. These are build-time project settings so we can ship different "
            + "Board browser behavior without rewriting the app shell. Build Profiles can override the website environment at build time.",
            MessageType.Info);

        EditorGUILayout.PropertyField(
            serializedSettings.FindProperty("m_targetEnvironment"),
            new GUIContent("Default Website Environment"));

        EditorGUILayout.PropertyField(
            serializedSettings.FindProperty("m_browsePresentationMode"),
            new GUIContent("Browse Presentation Mode"));

        EditorGUILayout.Space();
        var activeBuildProfile = BuildProfile.GetActiveBuildProfile();
        var activeBuildProfileSettings = activeBuildProfile != null
            ? activeBuildProfile.GetComponent<BeHomeBuildProfileSettings>()
            : null;
        var effectiveEnvironment = activeBuildProfileSettings != null
            ? activeBuildProfileSettings.TargetEnvironment
            : settings.TargetEnvironment;
        var resolvedBrowseUrl = BeHomeProjectSettings.ResolveBrowsePageUrl(
            effectiveEnvironment,
            settings.BrowsePresentationMode);

        EditorGUILayout.LabelField(
            "Active Build Profile Override",
            activeBuildProfileSettings != null
                ? effectiveEnvironment.ToString()
                : "None (using default project setting)",
            EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("Resolved Build URL", resolvedBrowseUrl, EditorStyles.wordWrappedLabel);

        if (serializedSettings.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
