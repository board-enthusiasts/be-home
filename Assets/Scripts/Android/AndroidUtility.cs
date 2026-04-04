using JetBrains.Annotations;

using UnityEngine;

namespace Android
{
/// <summary>
/// Utilities for interfacing with Android.
/// </summary>
public static class AndroidUtility
{
    /// <summary>
    /// Call a specific Android intent on the current Unity3D activity. 
    /// </summary>
    /// <param name="methodName">The name of the intent to call.</param>
    public static T Call<T>(string methodName) where T : class
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var currentActivity = GetCurrentActivity();
            return currentActivity?.Call<T>(methodName);
        }
        catch(System.Exception ex) { UnityEngine.Debug.LogError($"Failed to call the method with name <{methodName}> and return type <{typeof(T).Name}>: {ex}"); }
    #else
        UnityEngine.Debug.Log($"{nameof(Call)}({methodName}) is only available in Android builds.");
    #endif
        
        return null;
    }
    
    /// <summary>
    /// Call a specific Android intent on the current Unity3D activity. 
    /// </summary>
    /// <param name="intentName">The name of the intent to call.</param>
    public static void StartActivity(string intentName)
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var currentActivity = GetCurrentActivity();
            using var intent = GetIntent(intentName);

            currentActivity?.Call(AndroidMethodNames.StartActivity, intent);
        }
        catch(System.Exception ex) { UnityEngine.Debug.LogError($"Failed to start the activity with intent name <{intentName}>: {ex}"); }
    #else
        UnityEngine.Debug.Log($"{nameof(StartActivity)}({intentName}) is only available in Android builds.");
    #endif
    }
    
    /// <summary>
    /// Open the Android settings.
    /// </summary>
    public static void OpenAndroidSettings()
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            StartActivity(AndroidIntents.AndroidSettings);
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed to open Android settings: {ex}");
        }
    #else
        UnityEngine.Debug.Log($"{nameof(OpenAndroidSettings)}() is only available in Android builds.");
    #endif
    }
    
    /// <summary>
    /// Open the Android bluetooth settings.
    /// </summary>
    public static void OpenBluetoothSettings()
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            StartActivity(AndroidIntents.BluetoothSettings);
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed to open bluetooth settings: {ex}");
        }
    #else
        UnityEngine.Debug.Log($"{nameof(OpenBluetoothSettings)}() is only available in Android builds.");
    #endif
    }
    
    /// <summary>
    /// Open the Android developer options.
    /// </summary>
    public static void OpenDeveloperOptions()
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            StartActivity(AndroidIntents.DeveloperOptions);
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed to open developer options: {ex}");
        }
    #else
        UnityEngine.Debug.Log($"{nameof(OpenDeveloperOptions)}() is only available in Android builds.");
    #endif
    }
    
    /// <summary>
    /// Open the Android device information settings.
    /// </summary>
    public static void OpenDeviceInfoSettings()
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            StartActivity(AndroidIntents.DeviceInfoSettings);
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed to open device information settings: {ex}");
        }
    #else
        UnityEngine.Debug.Log($"{nameof(OpenDeviceInfoSettings)}() is only available in Android builds.");
    #endif
    }
    
    /// <summary>
    /// Open the Android app settings.
    /// </summary>
    public static void OpenAppSettings()
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            StartActivity(AndroidIntents.ManageAppSettings);
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed to open app settings: {ex}");
        }
    #else
        UnityEngine.Debug.Log($"{nameof(OpenAppSettings)}() is only available in Android builds.");
    #endif
    }
    
    public static bool AreDeveloperOptionsEnabled()
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var resolver = Call<AndroidJavaObject>(AndroidMethodNames.GetContentResolver);
            UnityEngine.Debug.LogError($"Content resolver: {resolver}");
            using var settingsGlobal = new AndroidJavaClass(AndroidClassNames.GlobalSettings);

            int enabled = settingsGlobal.CallStatic<int>(
                AndroidMethodNames.GetInt,
                resolver,
                AndroidGlobalSettingsNames.DevelopmentSettingsEnabled,
                0
            );

            return enabled != 0;
        }
        catch(System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed to read developer-options state: {ex}");
            return false;
        }
    #else
        return false;
    #endif
    }
    
    
    /// <summary>
    /// Get the current Unity player activity, which is required for most Android interactions.
    /// </summary>
    [CanBeNull]
    public static AndroidJavaObject GetCurrentActivity()
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var unityPlayer = new AndroidJavaClass(AndroidClassNames.UnityPlayerClassName);
            return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
        catch(System.Exception ex) { UnityEngine.Debug.LogError($"Failed to get the current activity: {ex}"); }
    #endif

        return null;
    }

    /// <summary>
    /// Get an object for a specific Android intent. 
    /// </summary>
    /// <param name="intentName">The name of the intent to get the object for.</param>
    [CanBeNull]
    public static AndroidJavaObject GetIntent(string intentName)
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            return new AndroidJavaObject(AndroidClassNames.AndroidIntent, intentName);
        }
        catch(System.Exception ex) { UnityEngine.Debug.LogError($"Failed to get the intent with name <{intentName}>: {ex}"); }
    #endif

        return null;
    }

    /// <summary>
    /// Get the content resolver from the current Unity 3D activity.
    /// </summary>
    [CanBeNull]
    public static AndroidJavaObject GetContentResolver()
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var currentActivity = GetCurrentActivity();
            return currentActivity?.Call<AndroidJavaObject>(AndroidMethodNames.GetContentResolver);
        }
        catch(System.Exception ex) { UnityEngine.Debug.LogError($"Failed to get the content resolver: {ex}"); }
    #endif

        return null;
    }
}
}
