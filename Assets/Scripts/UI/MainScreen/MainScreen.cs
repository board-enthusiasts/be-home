using Android;

using Board.Core;

using Rahmen.Extensions;

using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainScreen : MonoBehaviour
{
    public const string ClassName = "main-screen";
    public const string NavButtonClassName = ClassName + "__nav-button";
    
    public const string DeveloperOptionsEnabledModifierClassName = "--developer-options-enabled";

    public const string AppsButtonName = "apps";
    public const string BluetoothSettingsButtonName = "bluetooth-settings";
    public const string DeviceInfoSettingsButtonName = "device-info-settings";
    public const string DeveloperOptionsButtonName = "developer-options";
    
    private UIDocument _uiDocument;

    private void Awake()
    {
        _uiDocument = this.GetRequiredComponent<UIDocument>();
    }

    private void OnEnable()
    {
        BoardApplication.ShowProfileSwitcher();
        BoardApplication.pauseScreenActionReceived += OnPauseScreenActionReceived;
        BuildUI();
    }

    private void OnDisable()
    {
        BoardApplication.HideProfileSwitcher();
        BoardApplication.pauseScreenActionReceived -= OnPauseScreenActionReceived;
    }

    private void BuildUI()
    {
        var root = _uiDocument.rootVisualElement.Q(className: ClassName);
        var appsButton = root?.Q(AppsButtonName, NavButtonClassName);
        appsButton?.AddManipulator(new Clickable(AndroidUtility.OpenAppSettings));
        var bluetoothButton = root?.Q(BluetoothSettingsButtonName, NavButtonClassName);
        bluetoothButton?.AddManipulator(new Clickable(AndroidUtility.OpenBluetoothSettings));
        var deviceInfoSettingsButton = root?.Q(DeviceInfoSettingsButtonName, NavButtonClassName);
        deviceInfoSettingsButton?.AddManipulator(new Clickable(AndroidUtility.OpenDeviceInfoSettings));

        var developerSettingsEnabled = AndroidUtility.AreDeveloperOptionsEnabled();
        root?.EnableInClassList(DeveloperOptionsEnabledModifierClassName, developerSettingsEnabled);

        if(developerSettingsEnabled)
        {
            var developerOptionsButton = root?.Q(DeveloperOptionsButtonName, NavButtonClassName);
            developerOptionsButton?.AddManipulator(new Clickable(AndroidUtility.OpenDeveloperOptions));
        }
    }
    
    private void OnPauseScreenActionReceived(BoardPauseAction pauseAction, BoardPauseAudioTrack[] audioTracks)
    {
        switch (pauseAction)
        {
        case BoardPauseAction.ExitGameSaved:
        case BoardPauseAction.ExitGameUnsaved:
            BoardApplication.Exit();
            break;
        }
    }
}
