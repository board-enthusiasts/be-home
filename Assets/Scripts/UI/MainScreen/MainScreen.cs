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
    public const string BrowseContentHostName = "browse-content-host";
    public const string BrowseStatusName = "browse-status";
    public const string BrowseBackButtonName = "browse-back";
    public const string BrowsePageUrl = "https://staging.boardenthusiasts.com/browse";
    
    public const string DeveloperOptionsEnabledModifierClassName = "--developer-options-enabled";

    public const string AppsButtonName = "apps";
    public const string BluetoothSettingsButtonName = "bluetooth-settings";
    public const string DeviceInfoSettingsButtonName = "device-info-settings";
    public const string DeveloperOptionsButtonName = "developer-options";
    
    private UIDocument _uiDocument;
    private VisualElement _root;
    private VisualElement _browseContentHost;
    private Label _browseStatus;
    private VisualElement _browseBackButton;
    private IVisualElementScheduledItem _browseLayoutRefresh;
    private bool _isUiBuilt;

#if UNITY_ANDROID && !UNITY_EDITOR
    private WebViewObject _browseWebView;
    private bool _hasStartedBrowseWebView;
#endif

    private void Awake()
    {
        _uiDocument = this.GetRequiredComponent<UIDocument>();
    }

    private void OnEnable()
    {
        BoardApplication.ShowProfileSwitcher();
        BoardApplication.pauseScreenActionReceived += OnPauseScreenActionReceived;
        BuildUI();
        StartBrowseSpike();
    }

    private void OnDisable()
    {
        BoardApplication.HideProfileSwitcher();
        BoardApplication.pauseScreenActionReceived -= OnPauseScreenActionReceived;
        StopBrowseSpike();
    }

    private void OnDestroy()
    {
        DestroyBrowseSpike();
    }

    private void BuildUI()
    {
        _root = _uiDocument.rootVisualElement.Q(className: ClassName);
        _browseContentHost = _root?.Q<VisualElement>(BrowseContentHostName);
        _browseStatus = _root?.Q<Label>(BrowseStatusName);
        _browseBackButton = _root?.Q<VisualElement>(BrowseBackButtonName);

        if (_isUiBuilt)
        {
            return;
        }

        var appsButton = _root?.Q(AppsButtonName, NavButtonClassName);
        appsButton?.AddManipulator(new Clickable(AndroidUtility.OpenAppSettings));
        var bluetoothButton = _root?.Q(BluetoothSettingsButtonName, NavButtonClassName);
        bluetoothButton?.AddManipulator(new Clickable(AndroidUtility.OpenBluetoothSettings));
        var deviceInfoSettingsButton = _root?.Q(DeviceInfoSettingsButtonName, NavButtonClassName);
        deviceInfoSettingsButton?.AddManipulator(new Clickable(AndroidUtility.OpenDeviceInfoSettings));

        var developerSettingsEnabled = AndroidUtility.AreDeveloperOptionsEnabled();
        _root?.EnableInClassList(DeveloperOptionsEnabledModifierClassName, developerSettingsEnabled);

        if (developerSettingsEnabled)
        {
            var developerOptionsButton = _root?.Q(DeveloperOptionsButtonName, NavButtonClassName);
            developerOptionsButton?.AddManipulator(new Clickable(AndroidUtility.OpenDeveloperOptions));
        }

        _browseBackButton?.AddManipulator(new Clickable(GoBackInBrowse));
        SetBrowseBackEnabled(false);

        _isUiBuilt = true;
    }

    private void StartBrowseSpike()
    {
        SetBrowseStatus("Preparing embedded browse spike...");

#if UNITY_ANDROID && !UNITY_EDITOR
        EnsureBrowseWebView();

        if (_browseWebView == null)
        {
            SetBrowseStatus("Browse spike is unavailable because the WebView package is missing.");
            return;
        }

        _browseWebView.SetVisibility(true);
        ScheduleBrowseLayoutRefresh();

        if (_hasStartedBrowseWebView)
        {
            RefreshBrowseLayout();
            return;
        }

        _hasStartedBrowseWebView = true;
        RefreshBrowseLayout();
        SetBrowseStatus($"Loading {BrowsePageUrl}");
        _browseWebView.LoadURL(BrowsePageUrl);
#else
        SetBrowseStatus($"Android device build required for the embedded browse spike: {BrowsePageUrl}");
#endif
    }

    private void StopBrowseSpike()
    {
        _browseLayoutRefresh?.Pause();
        SetBrowseBackEnabled(false);

#if UNITY_ANDROID && !UNITY_EDITOR
        _browseWebView?.SetVisibility(false);
#endif
    }

    private void DestroyBrowseSpike()
    {
        _browseLayoutRefresh?.Pause();
        _browseLayoutRefresh = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (_browseWebView != null)
        {
            Destroy(_browseWebView.gameObject);
            _browseWebView = null;
        }
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void EnsureBrowseWebView()
    {
        if (_browseWebView != null)
        {
            return;
        }

        var webViewGameObject = new GameObject("BrowseWebView");
        webViewGameObject.transform.SetParent(transform, false);

        _browseWebView = webViewGameObject.AddComponent<WebViewObject>();
        _browseWebView.Init(
            err: message => SetBrowseStatus($"WebView error: {message}"),
            httpErr: message => SetBrowseStatus($"HTTP error: {message}"),
            ld: message =>
            {
                RefreshBrowseLayout();
                UpdateBrowseBackState();
                SetBrowseStatus($"Loaded {message}");
            },
            started: message =>
            {
                UpdateBrowseBackState();
                SetBrowseStatus($"Loading {message}");
            });
    }
#endif

    private void ScheduleBrowseLayoutRefresh()
    {
        if (_root == null)
        {
            return;
        }

        if (_browseLayoutRefresh == null)
        {
            _browseLayoutRefresh = _root.schedule.Execute(RefreshBrowseLayout).Every(250);
        }
        else
        {
            _browseLayoutRefresh.Resume();
        }

        RefreshBrowseLayout();
    }

    private void RefreshBrowseLayout()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_browseWebView == null || _root == null || _browseContentHost == null)
        {
            return;
        }

        var rootBounds = _root.worldBound;
        var contentBounds = _browseContentHost.worldBound;

        if (rootBounds.width <= 0f || rootBounds.height <= 0f || contentBounds.width <= 0f || contentBounds.height <= 0f)
        {
            return;
        }

        float widthScale = Screen.width / rootBounds.width;
        float heightScale = Screen.height / rootBounds.height;

        int left = Mathf.RoundToInt(contentBounds.xMin * widthScale);
        int top = Mathf.RoundToInt(contentBounds.yMin * heightScale);
        int right = Mathf.RoundToInt((rootBounds.xMax - contentBounds.xMax) * widthScale);
        int bottom = Mathf.RoundToInt((rootBounds.yMax - contentBounds.yMax) * heightScale);

        _browseWebView.SetMargins(left, top, right, bottom, true);
        UpdateBrowseBackState();
#endif
    }

    private void GoBackInBrowse()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_browseWebView == null || !_browseWebView.CanGoBack())
        {
            return;
        }

        _browseWebView.GoBack();
        SetBrowseStatus("Going back...");
        UpdateBrowseBackState();
#else
        SetBrowseStatus("Browse history is only available in Android device builds.");
#endif
    }

    private void SetBrowseBackEnabled(bool isEnabled)
    {
        if (_browseBackButton == null)
        {
            return;
        }

        _browseBackButton.SetEnabled(isEnabled);
    }

    private void UpdateBrowseBackState()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SetBrowseBackEnabled(_browseWebView != null && _browseWebView.CanGoBack());
#else
        SetBrowseBackEnabled(false);
#endif
    }

    private void SetBrowseStatus(string message)
    {
        if (_browseStatus != null)
        {
            _browseStatus.text = message;
        }

        UnityEngine.Debug.Log($"[MainScreen] {message}");
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
