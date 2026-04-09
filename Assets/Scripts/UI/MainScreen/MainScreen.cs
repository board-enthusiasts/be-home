using System;
using System.Linq;

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
    public const string BrowseBackButtonName = "browse-back";
    public const string BrowseOverlayName = "browse-overlay";
    public const string BrowseOverlayLogoName = "browse-overlay-logo";
    public const string BrowseOverlayTitleName = "browse-overlay-title";
    public const string BrowseOverlayBodyName = "browse-overlay-body";

    public const string DeveloperOptionsEnabledModifierClassName = "--developer-options-enabled";
    public const string DeveloperAuthenticatedModifierClassName = "--developer-authenticated";

    public const string AppsButtonName = "apps";
    public const string BluetoothSettingsButtonName = "bluetooth-settings";
    public const string DeviceInfoSettingsButtonName = "device-info-settings";
    public const string DeveloperOptionsButtonName = "developer-options";

    private const int BrowseLayoutRefreshIntervalMs = 250;
    private const int BrowseRetryIntervalMs = 10000;
    private const int BrowseOverlayAnimationIntervalMs = 33;
    private const float BrowseOverlayAnimationAngleDegrees = 10f;
    private const float BrowseOverlayAnimationFrequency = 2.5f;
    private const string OfflineBrowseMessage = "We can't reach the BE Game Index right now. Make sure you have Wifi connected on your Board";
    private const string BeHomeAuthStateMessageType = "be-home-auth-state";
    private const string BeHomeOpenExternalUrlMessageType = "be-home-open-external-url";

    private UIDocument _uiDocument;
    private VisualElement _root;
    private VisualElement _browseContentHost;
    private VisualElement _browseBackButton;
    private VisualElement _browseOverlay;
    private VisualElement _browseOverlayLogo;
    private Label _browseOverlayTitle;
    private Label _browseOverlayBody;
    private IVisualElementScheduledItem _browseLayoutRefresh;
    private IVisualElementScheduledItem _browseRetryRefresh;
    private IVisualElementScheduledItem _browseOverlayAnimation;
    private bool _isUiBuilt;
    private bool _hasLoadedBrowseContent;
    private bool _isBrowseOffline;
    private string _browsePageUrl;

#if UNITY_ANDROID && !UNITY_EDITOR
    private WebViewObject _browseWebView;
    private bool _hasStartedBrowseWebView;
#endif

    private void Awake()
    {
        _uiDocument = this.GetRequiredComponent<UIDocument>();
        _browsePageUrl = BeHomeProjectSettings.GetConfiguredBrowsePageUrl();
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
        _browseBackButton = _root?.Q<VisualElement>(BrowseBackButtonName);
        _browseOverlay = _root?.Q<VisualElement>(BrowseOverlayName);
        _browseOverlayLogo = _root?.Q<VisualElement>(BrowseOverlayLogoName);
        _browseOverlayTitle = _root?.Q<Label>(BrowseOverlayTitleName);
        _browseOverlayBody = _root?.Q<Label>(BrowseOverlayBodyName);

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
        SetDeveloperShellAccess(false);
        SetBrowseOverlay(isVisible: true, title: null, body: null);

        _isUiBuilt = true;
    }

    private void StartBrowseSpike()
    {
        ScheduleBrowseOverlayAnimation();

#if UNITY_ANDROID && !UNITY_EDITOR
        EnsureBrowseWebView();

        if (_browseWebView == null)
        {
            ShowMessageOverlay("Embedded browse is unavailable on this build.", "The Android WebView package did not initialize.");
            return;
        }

        _browseWebView.SetVisibility(false);
        ScheduleBrowseLayoutRefresh();

        if (_hasStartedBrowseWebView)
        {
            RefreshBrowseLayout();

            if (_hasLoadedBrowseContent && !_isBrowseOffline)
            {
                HideBrowseOverlay();
                _browseWebView.SetVisibility(true);
                UpdateBrowseBackState();
                return;
            }

            if (_isBrowseOffline)
            {
                ShowOfflineOverlay();
                ScheduleBrowseRetry();
                return;
            }

            TryLoadBrowsePage();
            return;
        }

        _hasStartedBrowseWebView = true;
        TryLoadBrowsePage();
#else
        ShowMessageOverlay("Android device build required.", _browsePageUrl);
#endif
    }

    private void StopBrowseSpike()
    {
        _browseLayoutRefresh?.Pause();
        _browseRetryRefresh?.Pause();
        _browseOverlayAnimation?.Pause();
        SetBrowseBackEnabled(false);

#if UNITY_ANDROID && !UNITY_EDITOR
        _browseWebView?.SetVisibility(false);
#endif
    }

    private void DestroyBrowseSpike()
    {
        _browseLayoutRefresh?.Pause();
        _browseLayoutRefresh = null;
        _browseRetryRefresh?.Pause();
        _browseRetryRefresh = null;
        _browseOverlayAnimation?.Pause();
        _browseOverlayAnimation = null;

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
            cb: HandleBrowseJavaScriptMessage,
            err: HandleBrowseLoadError,
            httpErr: HandleBrowseLoadError,
            ld: HandleBrowseLoaded,
            started: HandleBrowseStarted,
            transparent: true);
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
            _browseLayoutRefresh = _root.schedule.Execute(RefreshBrowseLayout).Every(BrowseLayoutRefreshIntervalMs);
        }
        else
        {
            _browseLayoutRefresh.Resume();
        }

        RefreshBrowseLayout();
    }

    private void ScheduleBrowseRetry()
    {
        if (_root == null)
        {
            return;
        }

        if (_browseRetryRefresh == null)
        {
            _browseRetryRefresh = _root.schedule.Execute(() => TryLoadBrowsePage(keepOfflineOverlay: true)).Every(BrowseRetryIntervalMs);
        }
        else
        {
            _browseRetryRefresh.Resume();
        }
    }

    private void StopBrowseRetry()
    {
        _browseRetryRefresh?.Pause();
    }

    private void ScheduleBrowseOverlayAnimation()
    {
        if (_root == null || _browseOverlayLogo == null)
        {
            return;
        }

        if (_browseOverlayAnimation == null)
        {
            _browseOverlayAnimation = _root.schedule.Execute(AnimateBrowseOverlay).Every(BrowseOverlayAnimationIntervalMs);
        }
        else
        {
            _browseOverlayAnimation.Resume();
        }

        AnimateBrowseOverlay();
    }

    private void AnimateBrowseOverlay()
    {
        if (_browseOverlayLogo == null)
        {
            return;
        }

        float angle = Mathf.Sin(Time.unscaledTime * BrowseOverlayAnimationFrequency) * BrowseOverlayAnimationAngleDegrees;
        _browseOverlayLogo.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
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

    private void TryLoadBrowsePage(bool keepOfflineOverlay = false)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_browseWebView == null)
        {
            return;
        }

        RefreshBrowseLayout();

        if (!keepOfflineOverlay)
        {
            ShowLoadingOverlay();
        }

        _browseWebView.SetVisibility(false);
        _browseWebView.LoadURL(_browsePageUrl);
        LogBrowseMessage($"Loading {_browsePageUrl}");
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
        LogBrowseMessage("Going back...");
        UpdateBrowseBackState();
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

    private void SetDeveloperShellAccess(bool hasAccess)
    {
        _root?.EnableInClassList(DeveloperAuthenticatedModifierClassName, hasAccess);
    }

    private void UpdateBrowseBackState()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SetBrowseBackEnabled(_browseWebView != null && !_isBrowseOffline && _browseWebView.CanGoBack());
#else
        SetBrowseBackEnabled(false);
#endif
    }

    private void ShowLoadingOverlay()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _isBrowseOffline = false;
#endif
        SetBrowseOverlay(isVisible: true, title: null, body: null);
    }

    private void ShowOfflineOverlay()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _isBrowseOffline = true;
#endif
        SetBrowseOverlay(isVisible: true, title: null, body: OfflineBrowseMessage);
    }

    private void ShowMessageOverlay(string title, string body)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _isBrowseOffline = false;
#endif
        SetBrowseOverlay(isVisible: true, title, body);
    }

    private void HideBrowseOverlay()
    {
        SetBrowseOverlay(isVisible: false, title: null, body: null);
    }

    private void SetBrowseOverlay(bool isVisible, string title, string body)
    {
        if (_browseOverlay == null)
        {
            return;
        }

        _browseOverlay.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;

        if (_browseOverlayTitle != null)
        {
            bool showTitle = isVisible && !string.IsNullOrWhiteSpace(title);
            _browseOverlayTitle.style.display = showTitle ? DisplayStyle.Flex : DisplayStyle.None;
            _browseOverlayTitle.text = showTitle ? title : string.Empty;
        }

        if (_browseOverlayBody != null)
        {
            bool showBody = isVisible && !string.IsNullOrWhiteSpace(body);
            _browseOverlayBody.style.display = showBody ? DisplayStyle.Flex : DisplayStyle.None;
            _browseOverlayBody.text = showBody ? body : string.Empty;
        }
    }

    private static bool IsBlankBrowsePage(string url)
    {
        return string.IsNullOrWhiteSpace(url)
            || url.StartsWith("about:blank", StringComparison.OrdinalIgnoreCase);
    }

    private static void LogBrowseMessage(string message)
    {
        UnityEngine.Debug.Log($"[MainScreen] {message}");
    }

    private static bool HasDeveloperShellAccess(string[] roles)
    {
        if (roles == null || roles.Length == 0)
        {
            return false;
        }

        return roles.Any((role) =>
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return false;
            }

            switch (role.Trim().ToLowerInvariant())
            {
            case "developer":
            case "verified_developer":
            case "moderator":
            case "admin":
            case "super_admin":
                return true;
            default:
                return false;
            }
        });
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

#if UNITY_ANDROID && !UNITY_EDITOR
    [Serializable]
    private sealed class BeHomeAuthStateMessage
    {
        public string type;
        public bool authenticated;
        public string[] roles;
        public string displayName;
    }

    [Serializable]
    private sealed class BeHomeOpenExternalUrlMessage
    {
        public string type;
        public string url;
    }

    private void HandleBrowseJavaScriptMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        try
        {
            if (message.Contains(BeHomeOpenExternalUrlMessageType, StringComparison.Ordinal))
            {
                var openExternalUrlMessage = JsonUtility.FromJson<BeHomeOpenExternalUrlMessage>(message);
                if (openExternalUrlMessage != null
                    && string.Equals(openExternalUrlMessage.type, BeHomeOpenExternalUrlMessageType, StringComparison.Ordinal)
                    && !string.IsNullOrWhiteSpace(openExternalUrlMessage.url))
                {
                    Application.OpenURL(openExternalUrlMessage.url);
                    LogBrowseMessage($"Opening external URL outside WebView: {openExternalUrlMessage.url}");
                    return;
                }
            }

            var authState = JsonUtility.FromJson<BeHomeAuthStateMessage>(message);
            if (authState == null || !string.Equals(authState.type, BeHomeAuthStateMessageType, StringComparison.Ordinal))
            {
                return;
            }

            bool hasDeveloperShellAccess = authState.authenticated && HasDeveloperShellAccess(authState.roles);
            SetDeveloperShellAccess(hasDeveloperShellAccess);
            LogBrowseMessage(
                $"Received auth bridge state: authenticated={authState.authenticated}, developerShellAccess={hasDeveloperShellAccess}, displayName={authState.displayName ?? "(none)"}");
        }
        catch (Exception ex)
        {
            LogBrowseMessage($"Ignored malformed JS bridge message: {ex.Message}");
        }
    }

    private void HandleBrowseStarted(string url)
    {
        RefreshBrowseLayout();

        if (!_isBrowseOffline && !_hasLoadedBrowseContent)
        {
            ShowLoadingOverlay();
            _browseWebView?.SetVisibility(false);
        }

        UpdateBrowseBackState();
        LogBrowseMessage($"Loading {url}");
    }

    private void HandleBrowseLoaded(string url)
    {
        RefreshBrowseLayout();

        if (IsBlankBrowsePage(url))
        {
            HandleBrowseUnavailable($"The WebView reported a blank page for {_browsePageUrl}.");
            return;
        }

        _hasLoadedBrowseContent = true;
        _isBrowseOffline = false;
        StopBrowseRetry();
        HideBrowseOverlay();
        _browseWebView?.SetVisibility(true);
        UpdateBrowseBackState();
        LogBrowseMessage($"Loaded {url}");
    }

    private void HandleBrowseLoadError(string message)
    {
        HandleBrowseUnavailable(message);
    }

    private void HandleBrowseUnavailable(string message)
    {
        _hasLoadedBrowseContent = false;
        _isBrowseOffline = true;
        _browseWebView?.SetVisibility(false);
        ShowOfflineOverlay();
        ScheduleBrowseRetry();
        UpdateBrowseBackState();
        LogBrowseMessage($"Browse unavailable: {message}");
    }
#endif
}
