using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Android;

using BoardEnthusiasts.BeHome.Api.DeviceIdentity;
using BoardEnthusiasts.BeHome.Api.Http;
using BoardEnthusiasts.BeHome.Api.Models;
using BoardEnthusiasts.BeHome.Api.Services;

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
    public const string BrowseHomeButtonName = "browse-home";
    public const string BrowseBackButtonName = "browse-back";
    public const string BrowseOverlayName = "browse-overlay";
    public const string BrowseOverlayLogoName = "browse-overlay-logo";
    public const string BrowseOverlayTitleName = "browse-overlay-title";
    public const string BrowseOverlayBodyName = "browse-overlay-body";
    public const string ExternalBrowserBackdropName = "external-browser-backdrop";
    public const string ExternalBrowserSurfaceName = "external-browser-surface";
    public const string ExternalBrowserHostName = "external-browser-host";
    public const string ExternalBrowserOverlayName = "external-browser-overlay";
    public const string ExternalBrowserOverlayLogoName = "external-browser-overlay-logo";
    public const string ExternalBrowserOverlayTitleName = "external-browser-overlay-title";
    public const string ExternalBrowserOverlayBodyName = "external-browser-overlay-body";
    public const string ExternalNoticeBackdropName = "external-notice-backdrop";
    public const string ExternalNoticeDialogName = "external-notice-dialog";
    public const string ExternalNoticeTitleName = "external-notice-title";
    public const string ExternalNoticeBodyName = "external-notice-body";
    public const string ExternalNoticeButtonName = "external-notice-ok";

    public const string DeveloperOptionsEnabledModifierClassName = "--developer-options-enabled";
    public const string DeveloperAuthenticatedModifierClassName = "--developer-authenticated";

    public const string AppsButtonName = "apps";
    public const string BluetoothSettingsButtonName = "bluetooth-settings";
    public const string DeviceInfoSettingsButtonName = "device-info-settings";
    public const string DeveloperOptionsButtonName = "developer-options";
    public const string QuickYoutubeButtonName = "quick-youtube";
    public const string QuickGptButtonName = "quick-gpt";
    public const string QuickBugReportButtonName = "quick-bug-report";
    public const string QuickLinkOfflineModifierClassName = ClassName + "__quick-link-button--offline";

    private const int BrowseLayoutRefreshIntervalMs = 250;
    private const int BrowseRetryIntervalMs = 10000;
    private const int BrowseOverlayAnimationIntervalMs = 33;
    private const int BeHomePresenceLeaseCheckIntervalMs = 30000;
    private const int BeHomeApiTimeoutSeconds = 10;
    private const float BrowseOverlayAnimationAngleDegrees = 10f;
    private const float BrowseOverlayAnimationFrequency = 2.5f;
    private const float BeHomePresenceLeaseRenewAfterSeconds = 120f;
    private const string LoadingOverlayTitle = "Loading...";
    private const string OfflineBrowseMessage = "We can't reach the BE Game Index right now. Make sure you have Wifi connected on your Board";
    private const string ExternalBrowseUnavailableMessage = "We couldn't open that page right now. Please try again.";
    private const string DiscordUnavailableTitle = "Discord isn't available on Board";
    private const string DiscordUnavailableMessage = "Please open Discord from your laptop or desktop computer instead.";
    private const string ConnectWifiFirstTitle = "Connect your Board to Wi-Fi first";
    private const string ConnectWifiFirstMessage = "These tools need an internet connection before they can open.";
    private const string QuickYoutubeUrl = "https://m.youtube.com/@boardenthusiasts";
    private const string QuickGptUrl = "https://chatgpt.com/g/g-69b033db223c81919edf748c33b08b3f-board-enthusiast";
    private const string BeHomeAuthStateMessageType = "be-home-auth-state";
    private const string BeHomeDiagnosticsMessageType = "be-home-diagnostics";
    private const string BeHomeOpenExternalUrlMessageType = "be-home-open-external-url";
    private const string BeHomeRouteStateMessageType = "be-home-route-state";
    private const string WebViewRenderProcessGoneErrorPrefix = "RENDER_PROCESS_GONE\t";
    private const int AndroidForceDarkModeOn = 2;
    private const int AndroidForceDarkModeOff = 0;
    private const float ExternalLoadErrorGracePeriodSeconds = 60f;

    private UIDocument _uiDocument;
    private VisualElement _root;
    private VisualElement _browseContentHost;
    private VisualElement _browseBackButton;
    private VisualElement _browseOverlay;
    private VisualElement _browseOverlayLogo;
    private Label _browseOverlayTitle;
    private Label _browseOverlayBody;
    private VisualElement _externalBrowserBackdrop;
    private VisualElement _externalBrowserSurface;
    private VisualElement _externalBrowserHost;
    private VisualElement _externalBrowserOverlay;
    private VisualElement _externalBrowserOverlayLogo;
    private Label _externalBrowserOverlayTitle;
    private Label _externalBrowserOverlayBody;
    private VisualElement _externalNoticeBackdrop;
    private VisualElement _externalNoticeDialog;
    private Label _externalNoticeTitle;
    private Label _externalNoticeBody;
    private VisualElement _externalNoticeButton;
    private VisualElement _quickYoutubeButton;
    private VisualElement _quickGptButton;
    private VisualElement _quickBugReportButton;
    private IVisualElementScheduledItem _browseLayoutRefresh;
    private IVisualElementScheduledItem _browseRetryRefresh;
    private IVisualElementScheduledItem _browseOverlayAnimation;
    private IVisualElementScheduledItem _beHomePresenceLeaseRefresh;
    private readonly BeHomeBrowseNavigationPolicy _browseNavigationPolicy = new BeHomeBrowseNavigationPolicy();
    private readonly BeHomePresenceLeasePolicy _beHomePresenceLeasePolicy = new BeHomePresenceLeasePolicy(BeHomePresenceLeaseRenewAfterSeconds);
    private BeHomeApiTransport _beHomeApiTransport;
    private IBeHomePresenceService _beHomePresenceService;
    private BeHomePresenceCoordinator _beHomePresenceCoordinator;
    private bool _isUiBuilt;
    private bool _isBeHomePresenceRegistrationInFlight;
    private bool _isBeHomeAnalyticsUnavailable;
    private bool _hasLoadedBrowseContent;
    private bool _isBrowseOffline;
    private bool _isExternalBrowserOpen;
    private string _lastHostedBrowseRoute;
    private bool _isExternalNoticeOpen;
    private string _browsePageUrl;
    private string _browseSiteHost;
    private string _externalBrowseUrl;
    private float _lastBeHomePresenceRefreshAt;
    private string _lastHostedDiagnosticsSummary;
    private string _lastResolvedBrowseUrl;
    private string _lastBrowseStartedUrl;
    private float _lastBrowseStartedAt;
    private string _lastBrowseLoadedUrl;
    private float _lastBrowseLoadedAt;

#if UNITY_ANDROID && !UNITY_EDITOR
    private bool _hasLoadedExternalContent;
    private WebViewObject _browseWebView;
    private WebViewObject _externalWebView;
    private bool _hasStartedBrowseWebView;
    private float _externalLoadStartedAt;
#endif

    private void Awake()
    {
        _uiDocument = this.GetRequiredComponent<UIDocument>();
        _browsePageUrl = BeHomeProjectSettings.GetConfiguredBrowsePageUrl();
        _browseSiteHost = TryGetHost(_browsePageUrl);
        InitializeBeHomeApiClient();
    }

    private void OnEnable()
    {
        BoardApplication.ShowProfileSwitcher();
        BoardApplication.pauseScreenActionReceived += OnPauseScreenActionReceived;
        BuildUI();
        StartBeHomeAnalytics();
        StartBrowseSpike();
    }

    private void OnDisable()
    {
        BoardApplication.HideProfileSwitcher();
        BoardApplication.pauseScreenActionReceived -= OnPauseScreenActionReceived;
        StopBeHomeAnalytics();
        StopBrowseSpike();
    }

    private void OnDestroy()
    {
        DestroyBrowseSpike();
        _beHomeApiTransport?.Dispose();
        _beHomeApiTransport = null;
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
        _externalBrowserBackdrop = _root?.Q<VisualElement>(ExternalBrowserBackdropName);
        _externalBrowserSurface = _root?.Q<VisualElement>(ExternalBrowserSurfaceName);
        _externalBrowserHost = _root?.Q<VisualElement>(ExternalBrowserHostName);
        _externalBrowserOverlay = _root?.Q<VisualElement>(ExternalBrowserOverlayName);
        _externalBrowserOverlayLogo = _root?.Q<VisualElement>(ExternalBrowserOverlayLogoName);
        _externalBrowserOverlayTitle = _root?.Q<Label>(ExternalBrowserOverlayTitleName);
        _externalBrowserOverlayBody = _root?.Q<Label>(ExternalBrowserOverlayBodyName);
        _externalNoticeBackdrop = _root?.Q<VisualElement>(ExternalNoticeBackdropName);
        _externalNoticeDialog = _root?.Q<VisualElement>(ExternalNoticeDialogName);
        _externalNoticeTitle = _root?.Q<Label>(ExternalNoticeTitleName);
        _externalNoticeBody = _root?.Q<Label>(ExternalNoticeBodyName);
        _externalNoticeButton = _root?.Q<VisualElement>(ExternalNoticeButtonName);
        _quickYoutubeButton = _root?.Q<VisualElement>(QuickYoutubeButtonName);
        _quickGptButton = _root?.Q<VisualElement>(QuickGptButtonName);
        _quickBugReportButton = _root?.Q<VisualElement>(QuickBugReportButtonName);

        if (_isUiBuilt)
        {
            return;
        }

        var browseHomeButton = _root?.Q(BrowseHomeButtonName, NavButtonClassName);
        browseHomeButton?.AddManipulator(new Clickable(GoHomeInBrowse));
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
        _quickYoutubeButton?.AddManipulator(new Clickable(OpenYoutubeQuickLink));
        _quickGptButton?.AddManipulator(new Clickable(OpenGptQuickLink));
        _quickBugReportButton?.AddManipulator(new Clickable(OpenBugReportQuickLink));
        _externalBrowserBackdrop?.RegisterCallback<ClickEvent>(OnExternalBrowserBackdropClicked);
        _externalBrowserSurface?.RegisterCallback<ClickEvent>((evt) => evt.StopPropagation());
        _externalNoticeBackdrop?.RegisterCallback<ClickEvent>(OnExternalNoticeBackdropClicked);
        _externalNoticeDialog?.RegisterCallback<ClickEvent>((evt) => evt.StopPropagation());
        _externalNoticeButton?.AddManipulator(new Clickable(HideExternalNotice));
        _externalNoticeButton?.RegisterCallback<ClickEvent>((evt) => evt.StopPropagation());
        SetBrowseBackEnabled(false);
        SetDeveloperShellAccess(false);
        SetBrowseOverlay(isVisible: true, title: null, body: null);
        SetExternalBrowserOverlay(isVisible: true, title: null, body: null);
        SetExternalBrowserModalVisible(false);
        SetExternalNoticeVisible(false, null, null);
        UpdateQuickLinkAvailability();

        _isUiBuilt = true;
    }

    private void InitializeBeHomeApiClient()
    {
        if (!ShouldRunBeHomeAnalytics())
        {
            return;
        }

        _beHomePresenceCoordinator = new BeHomePresenceCoordinator(
            new BeHomeDeviceIdentityProvider(new PlayerPrefsBeHomeInstallIdStore()),
            ResolveClientVersion(),
            BeHomeProjectSettings.GetConfiguredAppEnvironmentName());
        _beHomeApiTransport = new BeHomeApiTransport(
            BeHomeProjectSettings.GetConfiguredApiBaseUrl(),
            new UnityBeHomeJsonSerializer(),
            TimeSpan.FromSeconds(BeHomeApiTimeoutSeconds),
            _beHomePresenceCoordinator);
        _beHomePresenceService = new BeHomePresenceService(_beHomeApiTransport);
    }

    private void StartBeHomeAnalytics()
    {
        if (!ShouldRunBeHomeAnalytics()
            || _isBeHomeAnalyticsUnavailable
            || _beHomePresenceCoordinator == null
            || _beHomePresenceService == null)
        {
            return;
        }

        RequestBeHomePresenceRegistration();
        ScheduleBeHomePresenceLeaseRefresh();
    }

    private void StopBeHomeAnalytics()
    {
        _beHomePresenceLeaseRefresh?.Pause();

        if (_beHomePresenceCoordinator != null && _beHomePresenceService != null)
        {
            _ = EndBeHomePresenceBestEffortAsync(_beHomePresenceCoordinator.SessionId);
        }
    }

    private void RequestBeHomePresenceRegistration()
    {
        if (!ShouldRunBeHomeAnalytics()
            || _isBeHomeAnalyticsUnavailable
            || !isActiveAndEnabled
            || _beHomePresenceCoordinator == null
            || _beHomePresenceService == null)
        {
            return;
        }

        if (_isBeHomePresenceRegistrationInFlight)
        {
            return;
        }

        if (!IsInternetAvailable())
        {
            return;
        }

        _beHomePresenceCoordinator.MarkUserInteraction();
        StartCoroutine(RegisterBeHomePresenceCoroutine());
    }

    private IEnumerator RegisterBeHomePresenceCoroutine()
    {
        _isBeHomePresenceRegistrationInFlight = true;
        var requestedPresence = _beHomePresenceCoordinator.CreatePresenceSnapshot();
        var registrationTask = _beHomePresenceService.RegisterSessionAsync(requestedPresence, CancellationToken.None);
        yield return new WaitUntil(() => registrationTask.IsCompleted);
        _isBeHomePresenceRegistrationInFlight = false;

        if (registrationTask.IsCanceled)
        {
            TryRequestFollowUpPresenceRegistration(requestedPresence.AuthState, "canceled");
            yield break;
        }

        if (registrationTask.IsFaulted)
        {
            if (TryDisableBeHomeAnalyticsForMissingRoute(registrationTask.Exception?.GetBaseException()))
            {
                yield break;
            }

            LogBrowseMessage($"BE Home initial presence registration failed: {registrationTask.Exception?.GetBaseException() ?? new Exception("Unknown error.")}");
            TryRequestFollowUpPresenceRegistration(requestedPresence.AuthState, "failed");
            yield break;
        }

        _lastBeHomePresenceRefreshAt = Time.unscaledTime;
        LogBrowseMessage(
            $"BE Home presence registration accepted: session={_beHomePresenceCoordinator.SessionId}, authState={_beHomePresenceCoordinator.CurrentAuthState}, env={BeHomeProjectSettings.GetConfiguredAppEnvironmentName()}");
        TryRequestFollowUpPresenceRegistration(requestedPresence.AuthState, "completed");
    }

    private async Task EndBeHomePresenceBestEffortAsync(string sessionId)
    {
        try
        {
            using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await _beHomePresenceService.EndSessionAsync(sessionId, cancellationSource.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogBrowseMessage($"BE Home disconnect request failed: {ex}");
        }
    }

    private bool TryDisableBeHomeAnalyticsForMissingRoute(Exception exception)
    {
        if (exception is not BeHomeApiException beHomeApiException || beHomeApiException.StatusCode != 404)
        {
            return false;
        }

        _isBeHomeAnalyticsUnavailable = true;
        _isBeHomePresenceRegistrationInFlight = false;
        LogBrowseMessage(
            $"BE Home analytics endpoints are unavailable at {BeHomeProjectSettings.GetConfiguredApiBaseUrl()}. " +
            "Disabling native analytics for this session until the internal API routes are deployed.");
        return true;
    }

    private void UpdateBeHomeAuthState(BeHomeAuthState authState)
    {
        if (!ShouldRunBeHomeAnalytics() || _beHomePresenceCoordinator == null)
        {
            return;
        }

        var previousAuthState = _beHomePresenceCoordinator.CurrentAuthState;
        if (!_beHomePresenceCoordinator.SetAuthState(authState))
        {
            return;
        }

        _beHomePresenceCoordinator.MarkUserInteraction();
        if (_isBeHomePresenceRegistrationInFlight)
        {
            LogBrowseMessage(
                $"BE Home auth bridge state changed from {previousAuthState} to {authState} while presence registration is in flight. " +
                "A follow-up presence refresh will run after the current request finishes.");
            return;
        }

        LogBrowseMessage($"BE Home auth bridge state changed from {previousAuthState} to {authState}. Requesting immediate presence refresh.");
        RequestBeHomePresenceRegistration();
    }

    private void TryRequestFollowUpPresenceRegistration(BeHomeAuthState registeredAuthState, string completionState)
    {
        if (!ShouldRunBeHomeAnalytics()
            || _beHomePresenceCoordinator == null
            || _beHomePresenceCoordinator.CurrentAuthState == registeredAuthState)
        {
            return;
        }

        LogBrowseMessage(
            $"BE Home auth state changed during a {completionState} presence registration " +
            $"({registeredAuthState} -> {_beHomePresenceCoordinator.CurrentAuthState}). Requesting follow-up presence refresh.");
        RequestBeHomePresenceRegistration();
    }

    private void ScheduleBeHomePresenceLeaseRefresh()
    {
        if (_root == null)
        {
            return;
        }

        _beHomePresenceLeaseRefresh ??= _root.schedule.Execute(() =>
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            TryRenewBeHomePresenceLease();
#endif
        }).Every(BeHomePresenceLeaseCheckIntervalMs);

        _beHomePresenceLeaseRefresh.Resume();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void TryRenewBeHomePresenceLease()
    {
        if (!ShouldRunBeHomeAnalytics()
            || _isBeHomeAnalyticsUnavailable
            || !isActiveAndEnabled
            || _beHomePresenceCoordinator == null
            || _beHomePresenceService == null
            || !IsInternetAvailable())
        {
            return;
        }

        if (!_beHomePresenceLeasePolicy.ShouldRenew(_lastBeHomePresenceRefreshAt, Time.unscaledTime, _isBeHomePresenceRegistrationInFlight))
        {
            return;
        }

        _beHomePresenceCoordinator.MarkUserInteraction();
        LogBrowseMessage($"Renewing BE Home presence lease for session {_beHomePresenceCoordinator.SessionId}.");
        StartCoroutine(RegisterBeHomePresenceCoroutine());
    }
#endif

    private static bool ShouldRunBeHomeAnalytics()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return true;
#else
        return false;
#endif
    }

    private static string ResolveClientVersion()
    {
        return !string.IsNullOrWhiteSpace(Application.version)
            ? Application.version
            : "unknown";
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
        CloseExternalBrowser();
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
        _beHomePresenceLeaseRefresh?.Pause();
        _beHomePresenceLeaseRefresh = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (_browseWebView != null)
        {
            Destroy(_browseWebView.gameObject);
            _browseWebView = null;
        }

        DestroyExternalWebView();
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
            hooked: HandleBrowseHookedNavigation,
            androidForceDarkMode: AndroidForceDarkModeOn,
            transparent: true);
        _browseWebView.SetURLPattern(string.Empty, string.Empty, BuildExternalHookPattern());
    }

    private void EnsureExternalWebView()
    {
        if (_externalWebView != null)
        {
            return;
        }

        var webViewGameObject = new GameObject("ExternalBrowseWebView");
        webViewGameObject.transform.SetParent(transform, false);

        _externalWebView = webViewGameObject.AddComponent<WebViewObject>();
        _externalWebView.Init(
            cb: HandleBrowseJavaScriptMessage,
            err: HandleExternalLoadError,
            httpErr: HandleExternalLoadError,
            ld: HandleExternalLoaded,
            started: HandleExternalStarted,
            hooked: HandleExternalHookedNavigation,
            androidForceDarkMode: AndroidForceDarkModeOff,
            transparent: false);
        _externalWebView.SetURLPattern(string.Empty, string.Empty, BuildInternalHookPattern());
    }

    private void DestroyExternalWebView()
    {
        if (_externalWebView == null)
        {
            return;
        }

        Destroy(_externalWebView.gameObject);
        _externalWebView = null;
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
        if (_root == null || (_browseOverlayLogo == null && _externalBrowserOverlayLogo == null))
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
        float angle = Mathf.Sin(Time.unscaledTime * BrowseOverlayAnimationFrequency) * BrowseOverlayAnimationAngleDegrees;
        ApplyOverlayLogoRotation(_browseOverlayLogo, angle);
        ApplyOverlayLogoRotation(_externalBrowserOverlayLogo, angle);
    }

    private static void ApplyOverlayLogoRotation(VisualElement logo, float angle)
    {
        if (logo == null)
        {
            return;
        }

        logo.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
    }

    private void RefreshBrowseLayout()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        UpdateQuickLinkAvailability();

        if (_root == null)
        {
            return;
        }

        RefreshWebViewLayout(_browseWebView, _browseContentHost);
        RefreshWebViewLayout(_externalWebView, _externalBrowserHost);
        UpdateBrowseBackState();
#endif
    }

    private void OpenYoutubeQuickLink()
    {
        OpenOnlineQuickLink(QuickYoutubeUrl);
    }

    private void OpenGptQuickLink()
    {
        OpenOnlineQuickLink(QuickGptUrl);
    }

    private void OpenBugReportQuickLink()
    {
        if (!IsInternetAvailable())
        {
            ShowExternalNotice(ConnectWifiFirstTitle, ConnectWifiFirstMessage);
            return;
        }

        CloseExternalBrowser();
        LoadBrowseUrl(BuildSupportPageUrl(autoOpenSupport: true));
    }

    private void OpenOnlineQuickLink(string url)
    {
        if (!IsInternetAvailable())
        {
            ShowExternalNotice(ConnectWifiFirstTitle, ConnectWifiFirstMessage);
            return;
        }

        HideExternalNotice();
        OpenExternalBrowser(url);
    }

    private bool IsInternetAvailable()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    private void UpdateQuickLinkAvailability()
    {
        bool isOffline = !IsInternetAvailable();
        SetQuickLinkOfflineState(_quickYoutubeButton, isOffline);
        SetQuickLinkOfflineState(_quickGptButton, isOffline);
        SetQuickLinkOfflineState(_quickBugReportButton, isOffline);
    }

    private static void SetQuickLinkOfflineState(VisualElement button, bool isOffline)
    {
        button?.EnableInClassList(QuickLinkOfflineModifierClassName, isOffline);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void RefreshWebViewLayout(WebViewObject webView, VisualElement host)
    {
        if (webView == null || host == null || _root == null)
        {
            return;
        }

        var rootBounds = _root.worldBound;
        var contentBounds = host.worldBound;

        if (rootBounds.width <= 0f || rootBounds.height <= 0f || contentBounds.width <= 0f || contentBounds.height <= 0f)
        {
            return;
        }

        float widthScale = Screen.width / rootBounds.width;
        float heightScale = Screen.height / rootBounds.height;

        int left = Mathf.RoundToInt((contentBounds.xMin - rootBounds.xMin) * widthScale);
        int top = Mathf.RoundToInt((contentBounds.yMin - rootBounds.yMin) * heightScale);
        int right = Mathf.RoundToInt((rootBounds.xMax - contentBounds.xMax) * widthScale);
        int bottom = Mathf.RoundToInt((rootBounds.yMax - contentBounds.yMax) * heightScale);

        webView.SetMargins(left, top, right, bottom, true);
    }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    private void SetExternalContentLoaded(bool isLoaded)
    {
        _hasLoadedExternalContent = isLoaded;
    }

    private bool HasLoadedExternalContent()
    {
        return _hasLoadedExternalContent;
    }
#else
    private void SetExternalContentLoaded(bool isLoaded)
    {
    }

    private bool HasLoadedExternalContent()
    {
        return false;
    }
#endif

    private void TryLoadBrowsePage(bool keepOfflineOverlay = false)
    {
        LoadBrowseUrl(ResolveActiveBrowseUrl(), keepOfflineOverlay);
    }

    private void LoadBrowseUrl(string url, bool keepOfflineOverlay = false)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_browseWebView == null || string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        if (IsBrowseSiteUrl(url))
        {
            _lastResolvedBrowseUrl = url;
        }

        _browseNavigationPolicy.BeginTopLevelNavigation();
        RefreshBrowseLayout();

        if (!keepOfflineOverlay)
        {
            ShowLoadingOverlay();
        }

        _browseWebView.SetVisibility(false);
        _browseWebView.LoadURL(url);
        LogBrowseMessage($"Loading {url}");
#endif
    }

    private void GoBackInBrowse()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_isExternalNoticeOpen)
        {
            HideExternalNotice();
            UpdateBrowseBackState();
            return;
        }

        if (_isExternalBrowserOpen)
        {
            if (_externalWebView != null && _externalWebView.CanGoBack())
            {
                _externalWebView.GoBack();
                LogBrowseMessage("Going back in external browser...");
            }
            else
            {
                CloseExternalBrowser();
                LogBrowseMessage("Closing external browser...");
            }

            UpdateBrowseBackState();
            return;
        }

        if (_browseWebView == null || !_browseWebView.CanGoBack())
        {
            return;
        }

        _browseWebView.GoBack();
        LogBrowseMessage("Going back...");
        UpdateBrowseBackState();
#endif
    }

    private void GoHomeInBrowse()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        CloseExternalBrowser();
        _lastHostedBrowseRoute = null;
        _lastResolvedBrowseUrl = _browsePageUrl;
        LoadBrowseUrl(_browsePageUrl);
        LogBrowseMessage($"Returning browse surface to home: {_browsePageUrl}");
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
        if (_isExternalNoticeOpen)
        {
            SetBrowseBackEnabled(true);
            return;
        }

        if (_isExternalBrowserOpen)
        {
            SetBrowseBackEnabled(true);
            return;
        }

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
        SetBrowseOverlay(isVisible: true, title: LoadingOverlayTitle, body: null);
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

    private void OnExternalBrowserBackdropClicked(ClickEvent evt)
    {
        if (_externalBrowserSurface != null && _externalBrowserSurface.worldBound.Contains(evt.position))
        {
            evt.StopPropagation();
            return;
        }

        CloseExternalBrowser();
        evt.StopPropagation();
    }

    private void OnExternalNoticeBackdropClicked(ClickEvent evt)
    {
        if (_externalNoticeDialog != null && _externalNoticeDialog.worldBound.Contains(evt.position))
        {
            evt.StopPropagation();
            return;
        }

        HideExternalNotice();
        evt.StopPropagation();
    }

    private void SetExternalBrowserModalVisible(bool isVisible)
    {
        _isExternalBrowserOpen = isVisible;

        if (_externalBrowserBackdrop != null)
        {
            _externalBrowserBackdrop.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        _browseWebView?.SetInteractionEnabled(!isVisible);
        if (_browseWebView != null)
        {
            if (isVisible)
            {
                _browseWebView.SetVisibility(false);
            }
            else if (_hasLoadedBrowseContent && !_isBrowseOffline)
            {
                _browseWebView.SetVisibility(true);
            }
        }

        _externalWebView?.SetVisibility(isVisible && HasLoadedExternalContent());
#endif

        UpdateBrowseBackState();
    }

    private void SetExternalBrowserOverlay(bool isVisible, string title, string body)
    {
        if (_externalBrowserOverlay == null)
        {
            return;
        }

        _externalBrowserOverlay.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;

        if (_externalBrowserOverlayTitle != null)
        {
            bool showTitle = isVisible && !string.IsNullOrWhiteSpace(title);
            _externalBrowserOverlayTitle.style.display = showTitle ? DisplayStyle.Flex : DisplayStyle.None;
            _externalBrowserOverlayTitle.text = showTitle ? title : string.Empty;
        }

        if (_externalBrowserOverlayBody != null)
        {
            bool showBody = isVisible && !string.IsNullOrWhiteSpace(body);
            _externalBrowserOverlayBody.style.display = showBody ? DisplayStyle.Flex : DisplayStyle.None;
            _externalBrowserOverlayBody.text = showBody ? body : string.Empty;
        }
    }

    private void SetExternalNoticeVisible(bool isVisible, string title, string body)
    {
        _isExternalNoticeOpen = isVisible;

        if (_externalNoticeBackdrop != null)
        {
            _externalNoticeBackdrop.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (_externalNoticeTitle != null)
        {
            _externalNoticeTitle.text = isVisible && !string.IsNullOrWhiteSpace(title) ? title : string.Empty;
        }

        if (_externalNoticeBody != null)
        {
            _externalNoticeBody.text = isVisible && !string.IsNullOrWhiteSpace(body) ? body : string.Empty;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        _browseWebView?.SetInteractionEnabled(!isVisible && !_isExternalBrowserOpen);
        if (_browseWebView != null)
        {
            if (isVisible)
            {
                _browseWebView.SetVisibility(false);
            }
            else if (!_isExternalBrowserOpen && _hasLoadedBrowseContent && !_isBrowseOffline)
            {
                _browseWebView.SetVisibility(true);
            }
        }

        if (_externalWebView != null)
        {
            if (isVisible)
            {
                _externalWebView.SetVisibility(false);
            }
            else if (_isExternalBrowserOpen && HasLoadedExternalContent())
            {
                _externalWebView.SetVisibility(true);
            }
        }
#endif

        UpdateBrowseBackState();
    }

    private void ShowExternalNotice(string title, string body)
    {
        SetExternalNoticeVisible(true, title, body);
    }

    private void HideExternalNotice()
    {
        SetExternalNoticeVisible(false, null, null);
    }

    private void ShowExternalBrowserLoadingOverlay()
    {
        SetExternalContentLoaded(false);
        SetExternalBrowserOverlay(isVisible: true, title: LoadingOverlayTitle, body: null);
    }

    private void ShowExternalBrowserMessageOverlay(string title, string body)
    {
        SetExternalContentLoaded(false);
        SetExternalBrowserOverlay(isVisible: true, title, body);
    }

    private void HideExternalBrowserOverlay()
    {
        SetExternalBrowserOverlay(isVisible: false, title: null, body: null);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void ApplyExternalBrowserTheme(string url)
    {
        if (_externalWebView == null || !ShouldApplyExternalBrowserDarkTheme(url))
        {
            return;
        }

        if (url.IndexOf("youtube", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            const string youtubeDarkThemeScript = @"
(function () {
  try {
    document.cookie = 'PREF=f6=400;domain=.youtube.com;path=/;max-age=31536000';
    document.cookie = 'PREF=f6=400;domain=.m.youtube.com;path=/;max-age=31536000';
    var root = document.documentElement;
    var body = document.body;
    if (root) {
      root.style.backgroundColor = '#0f0f0f';
      root.style.colorScheme = 'dark';
    }
    if (body) {
      body.style.backgroundColor = '#0f0f0f';
      body.style.colorScheme = 'dark';
    }
  } catch (e) {
  }
})();";

            _externalWebView.EvaluateJS(youtubeDarkThemeScript);
            return;
        }

        if (url.IndexOf("chatgpt", StringComparison.OrdinalIgnoreCase) >= 0
            || url.IndexOf("openai", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            const string openAiDarkThemeScript = @"
(function () {
  try {
    var root = document.documentElement;
    var body = document.body;
    if (root) {
      root.style.backgroundColor = '#0b0f12';
      root.style.colorScheme = 'dark';
      root.classList.add('dark');
      root.setAttribute('data-theme', 'dark');
    }
    if (body) {
      body.style.backgroundColor = '#0b0f12';
      body.style.colorScheme = 'dark';
      body.classList.add('dark');
    }
    try { localStorage.setItem('theme', 'dark'); } catch (e) {}
    try { localStorage.setItem('oai/theme', 'dark'); } catch (e) {}
    try { localStorage.setItem('oai/apps/theme', 'dark'); } catch (e) {}
  } catch (e) {
  }
})();";

            _externalWebView.EvaluateJS(openAiDarkThemeScript);
        }
    }
#endif

    private void HandleRequestedExternalUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        HideExternalNotice();

        if (IsDiscordInviteUrl(url))
        {
            ShowExternalNotice(DiscordUnavailableTitle, DiscordUnavailableMessage);
            LogBrowseMessage($"Redirected Discord request to in-app notice: {url}");
            return;
        }

        OpenExternalBrowser(url);
    }

    private void OpenExternalBrowser(string url)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        if (_isExternalBrowserOpen && _externalBrowseUrl != null && string.Equals(_externalBrowseUrl, url, StringComparison.Ordinal))
        {
            return;
        }

        DestroyExternalWebView();
        EnsureExternalWebView();
        if (_externalWebView == null)
        {
            ShowMessageOverlay("Embedded browser is unavailable on this build.", "The Android WebView package did not initialize.");
            return;
        }

        _externalBrowseUrl = url;
        SetExternalContentLoaded(false);
        _externalLoadStartedAt = Time.unscaledTime;
        SetExternalBrowserModalVisible(true);
        ShowExternalBrowserLoadingOverlay();
        RefreshBrowseLayout();
        _externalWebView.SetVisibility(false);
        _externalWebView.LoadURL(url);
        LogBrowseMessage($"Opening external browser modal for {url}");
#endif
    }

    private void CloseExternalBrowser()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _externalBrowseUrl = null;
        _externalLoadStartedAt = 0f;
        SetExternalContentLoaded(false);
        _externalWebView?.SetVisibility(false);
        DestroyExternalWebView();
#endif
        HideExternalNotice();
        HideExternalBrowserOverlay();
        SetExternalBrowserModalVisible(false);
    }

    private static bool IsBlankBrowsePage(string url)
    {
        return string.IsNullOrWhiteSpace(url)
            || url.StartsWith("about:blank", StringComparison.OrdinalIgnoreCase);
    }

    private static string TryGetHost(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.Host : string.Empty;
    }

    private string ResolveBrowseSiteUrl(string routeOrUrl)
    {
        return BeHomeBrowseUrlResolver.ResolveBrowseSiteUrl(_browsePageUrl, routeOrUrl);
    }

    private string ResolveActiveBrowseUrl()
    {
        return BeHomeBrowseUrlResolver.ResolveActiveBrowseUrl(
            _browsePageUrl,
            _lastResolvedBrowseUrl,
            _lastHostedBrowseRoute);
    }

    private static bool TryGetWebViewRenderProcessGoneDetail(string message, out string detail)
    {
        if (!string.IsNullOrWhiteSpace(message)
            && message.StartsWith(WebViewRenderProcessGoneErrorPrefix, StringComparison.Ordinal))
        {
            detail = message.Substring(WebViewRenderProcessGoneErrorPrefix.Length);
            return true;
        }

        detail = null;
        return false;
    }

    private string BuildSupportPageUrl(bool autoOpenSupport)
    {
        if (!Uri.TryCreate(_browsePageUrl, UriKind.Absolute, out var browseUri))
        {
            return autoOpenSupport ? "/support?beHomeSupportOpen=1" : "/support";
        }

        var uriBuilder = new UriBuilder(browseUri)
        {
            Path = "/support",
        };

        string[] existingQuerySegments = (browseUri.Query ?? string.Empty)
            .TrimStart('?')
            .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
            .Where((segment) => !segment.StartsWith("beHomeSupportOpen=", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (autoOpenSupport)
        {
            existingQuerySegments = existingQuerySegments.Concat(new[] { "beHomeSupportOpen=1" }).ToArray();
        }

        uriBuilder.Query = string.Join("&", existingQuerySegments);
        return uriBuilder.Uri.ToString();
    }

    private static bool HostMatches(string host, string match)
    {
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(match))
        {
            return false;
        }

        return string.Equals(host, match, StringComparison.OrdinalIgnoreCase)
            || host.EndsWith($".{match}", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDiscordInviteUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (HostMatches(uri.Host, "discord.gg"))
        {
            return true;
        }

        if (!HostMatches(uri.Host, "discord.com") && !HostMatches(uri.Host, "discordapp.com"))
        {
            return false;
        }

        string path = uri.AbsolutePath ?? string.Empty;
        return path.StartsWith("/invite", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/invites", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldApplyExternalBrowserDarkTheme(string url)
    {
        string host = TryGetHost(url);

        if (HostMatches(host, "board.fun"))
        {
            return false;
        }

        return HostMatches(host, "youtube.com")
            || HostMatches(host, "m.youtube.com")
            || HostMatches(host, "youtu.be")
            || HostMatches(host, "chatgpt.com")
            || HostMatches(host, "chat.openai.com")
            || HostMatches(host, "openai.com");
    }

    private bool IsBrowseSiteUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(_browseSiteHost))
        {
            return false;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            && string.Equals(uri.Host, _browseSiteHost, StringComparison.OrdinalIgnoreCase);
    }

    private string BuildExternalHookPattern()
    {
        if (string.IsNullOrWhiteSpace(_browseSiteHost))
        {
            return @"^https?://.+";
        }

        string escapedHost = Regex.Escape(_browseSiteHost);
        return $@"^https?://(?!{escapedHost}(?::\d+)?(?:[/?#]|$)).+";
    }

    private string BuildInternalHookPattern()
    {
        if (string.IsNullOrWhiteSpace(_browseSiteHost))
        {
            return string.Empty;
        }

        string escapedHost = Regex.Escape(_browseSiteHost);
        return $@"^https?://{escapedHost}(?::\d+)?(?:[/?#].*)?$";
    }

    private string BuildBrowseDiagnosticsContext()
    {
        string hostedRoute = string.IsNullOrWhiteSpace(_lastHostedBrowseRoute) ? "(none)" : _lastHostedBrowseRoute;
        string hostedSummary = string.IsNullOrWhiteSpace(_lastHostedDiagnosticsSummary) ? "(none)" : _lastHostedDiagnosticsSummary;
        string lastStartedUrl = string.IsNullOrWhiteSpace(_lastBrowseStartedUrl) ? "(none)" : _lastBrowseStartedUrl;
        string lastLoadedUrl = string.IsNullOrWhiteSpace(_lastBrowseLoadedUrl) ? "(none)" : _lastBrowseLoadedUrl;
        return $"hostedRoute={hostedRoute}; hosted={hostedSummary}; lastStart={lastStartedUrl}; lastLoad={lastLoadedUrl}; lastLoadDuration={FormatBrowseDuration(_lastBrowseStartedAt, _lastBrowseLoadedAt)}";
    }

    private static string FormatBrowseDuration(float startedAt, float completedAt)
    {
        if (startedAt <= 0f || completedAt <= 0f || completedAt < startedAt)
        {
            return "n/a";
        }

        return $"{completedAt - startedAt:0.00}s";
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

    [Serializable]
    private sealed class BeHomeRouteStateMessage
    {
        public string type;
        public string path;
    }

    private void HandleBrowseJavaScriptMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        try
        {
            if (message.Contains(BeHomeRouteStateMessageType, StringComparison.Ordinal))
            {
                var routeState = JsonUtility.FromJson<BeHomeRouteStateMessage>(message);
                if (routeState != null
                    && string.Equals(routeState.type, BeHomeRouteStateMessageType, StringComparison.Ordinal)
                    && !string.IsNullOrWhiteSpace(routeState.path)
                    && !string.Equals(_lastHostedBrowseRoute, routeState.path, StringComparison.Ordinal))
                {
                    _lastHostedBrowseRoute = routeState.path;
                    _lastResolvedBrowseUrl = ResolveBrowseSiteUrl(routeState.path);
                    LogBrowseMessage($"Hosted route changed to {routeState.path} ({_lastResolvedBrowseUrl})");
                    return;
                }
            }

            if (message.Contains(BeHomeDiagnosticsMessageType, StringComparison.Ordinal))
            {
                var diagnostics = JsonUtility.FromJson<BeHomeBrowseDiagnostics>(message);
                if (diagnostics != null
                    && string.Equals(diagnostics.type, BeHomeDiagnosticsMessageType, StringComparison.Ordinal))
                {
                    if (!string.IsNullOrWhiteSpace(diagnostics.route))
                    {
                        _lastHostedBrowseRoute = diagnostics.route;
                        _lastResolvedBrowseUrl = ResolveBrowseSiteUrl(diagnostics.route);
                    }

                    _lastHostedDiagnosticsSummary = BeHomeBrowseDiagnosticsFormatter.Summarize(diagnostics);
                    LogBrowseMessage($"Hosted diagnostics updated: {_lastHostedDiagnosticsSummary}");
                    return;
                }
            }

            if (message.Contains(BeHomeOpenExternalUrlMessageType, StringComparison.Ordinal))
            {
                var openExternalUrlMessage = JsonUtility.FromJson<BeHomeOpenExternalUrlMessage>(message);
                if (openExternalUrlMessage != null
                    && string.Equals(openExternalUrlMessage.type, BeHomeOpenExternalUrlMessageType, StringComparison.Ordinal)
                    && !string.IsNullOrWhiteSpace(openExternalUrlMessage.url))
                {
                    HandleRequestedExternalUrl(openExternalUrlMessage.url);
                    LogBrowseMessage($"Opening external URL inside modal browser: {openExternalUrlMessage.url}");
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
            UpdateBeHomeAuthState(authState.authenticated ? BeHomeAuthState.SignedIn : BeHomeAuthState.Anonymous);
            LogBrowseMessage(
                $"Received auth bridge state: authenticated={authState.authenticated}, developerShellAccess={hasDeveloperShellAccess}, displayName={authState.displayName ?? "(none)"}");
        }
        catch (Exception ex)
        {
            LogBrowseMessage($"Ignored malformed JS bridge message: {ex.Message}");
        }
    }

    private void HandleBrowseHookedNavigation(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            HandleRequestedExternalUrl(url);
        }
    }

    private void HandleExternalHookedNavigation(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        if (!IsBrowseSiteUrl(url))
        {
            HandleRequestedExternalUrl(url);
            return;
        }

        CloseExternalBrowser();
        _lastResolvedBrowseUrl = ResolveBrowseSiteUrl(url);
        LoadBrowseUrl(url);
        LogBrowseMessage($"Returning modal browser navigation to main browse surface: {url}");
    }

    private void HandleBrowseStarted(string url)
    {
        if (!string.IsNullOrWhiteSpace(url) && IsBrowseSiteUrl(url))
        {
            _lastResolvedBrowseUrl = url;
        }

        _lastBrowseStartedUrl = url;
        _lastBrowseStartedAt = Time.unscaledTime;
        _browseNavigationPolicy.BeginTopLevelNavigation();
        RefreshBrowseLayout();

        if (!_isBrowseOffline && !_hasLoadedBrowseContent)
        {
            ShowLoadingOverlay();
            _browseWebView?.SetVisibility(false);
        }

        UpdateBrowseBackState();
        LogBrowseMessage($"Loading {url} | {BuildBrowseDiagnosticsContext()}");
    }

    private void HandleBrowseLoaded(string url)
    {
        RefreshBrowseLayout();

        if (IsBlankBrowsePage(url))
        {
            if (TryRecoverBrowseWebViewFromBlankPage())
            {
                return;
            }

            HandleBrowseUnavailable($"The WebView reported a blank page for {_browsePageUrl}.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(url) && IsBrowseSiteUrl(url))
        {
            _lastResolvedBrowseUrl = url;
        }

        _hasLoadedBrowseContent = true;
        _browseNavigationPolicy.MarkPrimaryContentLoaded();
        _isBrowseOffline = false;
        StopBrowseRetry();
        HideBrowseOverlay();
        _browseWebView?.SetVisibility(true);
        _lastBrowseLoadedUrl = url;
        _lastBrowseLoadedAt = Time.unscaledTime;
        UpdateBrowseBackState();
        LogBrowseMessage($"Loaded {url} in {FormatBrowseDuration(_lastBrowseStartedAt, _lastBrowseLoadedAt)} | {BuildBrowseDiagnosticsContext()}");
    }

    private void HandleBrowseLoadError(string message)
    {
        if (TryGetWebViewRenderProcessGoneDetail(message, out var rendererCrashDetail))
        {
            RecoverBrowseWebViewAfterRendererCrash(rendererCrashDetail);
            return;
        }

        if (!_browseNavigationPolicy.ShouldTreatLoadErrorAsUnavailable())
        {
            LogBrowseMessage($"Ignoring browse load error after primary content already loaded: {message} | {BuildBrowseDiagnosticsContext()}");
            return;
        }

        HandleBrowseUnavailable(message);
    }

    private void HandleExternalStarted(string url)
    {
        _externalLoadStartedAt = Time.unscaledTime;
        RefreshBrowseLayout();
        ShowExternalBrowserLoadingOverlay();
        _externalWebView?.SetVisibility(false);
        UpdateBrowseBackState();
        LogBrowseMessage($"Loading external modal URL {url}");
    }

    private void HandleExternalLoaded(string url)
    {
        RefreshBrowseLayout();

        if (IsBlankBrowsePage(url))
        {
            ShowExternalBrowserLoadingOverlay();
            _externalWebView?.SetVisibility(false);
            LogBrowseMessage($"Ignoring interim blank external load for {_externalBrowseUrl ?? "(unknown external URL)"}.");
            return;
        }

        SetExternalContentLoaded(true);
        ApplyExternalBrowserTheme(url);
        HideExternalBrowserOverlay();
        if (_isExternalBrowserOpen)
        {
            _externalWebView?.SetVisibility(true);
        }

        UpdateBrowseBackState();
        LogBrowseMessage($"Loaded external modal URL {url}");
    }

    private void HandleExternalLoadError(string message)
    {
        if (TryGetWebViewRenderProcessGoneDetail(message, out var rendererCrashDetail))
        {
            RecoverExternalWebViewAfterRendererCrash(rendererCrashDetail);
            return;
        }

        if (HasLoadedExternalContent())
        {
            LogBrowseMessage($"Ignoring external modal load error after content already loaded: {message}");
            return;
        }

        if (!HasLoadedExternalContent() && (Time.unscaledTime - _externalLoadStartedAt) < ExternalLoadErrorGracePeriodSeconds)
        {
            ShowExternalBrowserLoadingOverlay();
            _externalWebView?.SetVisibility(false);
            LogBrowseMessage($"Ignoring transient external modal load error during startup: {message}");
            return;
        }

        SetExternalContentLoaded(false);
        _externalWebView?.SetVisibility(false);
        ShowExternalBrowserMessageOverlay("This page is unavailable", ExternalBrowseUnavailableMessage);
        UpdateBrowseBackState();
        LogBrowseMessage($"External modal browser unavailable: {message}");
    }

    private void HandleBrowseUnavailable(string message)
    {
        _browseNavigationPolicy.MarkPrimaryContentUnavailable();
        _hasLoadedBrowseContent = false;
        _isBrowseOffline = true;
        _browseWebView?.SetVisibility(false);
        ShowOfflineOverlay();
        ScheduleBrowseRetry();
        UpdateBrowseBackState();
        LogBrowseMessage($"Browse unavailable: {message} | {BuildBrowseDiagnosticsContext()}");
    }

    private bool TryRecoverBrowseWebViewFromBlankPage()
    {
        if (!_browseNavigationPolicy.ShouldAttemptBlankPageRecovery())
        {
            return false;
        }

        _browseNavigationPolicy.MarkBlankPageRecoveryAttempted();
        RecoverBrowseWebView("Blank page reported after prior successful browse load");
        return true;
    }

    private void RecoverBrowseWebViewAfterRendererCrash(string detail)
    {
        RecoverBrowseWebView($"Renderer exited ({detail})");
    }

    private void RecoverBrowseWebView(string reason)
    {
        string recoveryUrl = ResolveActiveBrowseUrl();
        LogBrowseMessage($"Browse WebView recovery requested: {reason}. Recreating browse surface for {recoveryUrl}. | {BuildBrowseDiagnosticsContext()}");

        _browseNavigationPolicy.BeginTopLevelNavigation();
        _hasLoadedBrowseContent = false;
        _isBrowseOffline = false;
        StopBrowseRetry();

        if (_browseWebView != null)
        {
            Destroy(_browseWebView.gameObject);
            _browseWebView = null;
        }

        EnsureBrowseWebView();
        if (_browseWebView == null)
        {
            HandleBrowseUnavailable("The embedded browser crashed and could not be recreated.");
            return;
        }

        RefreshBrowseLayout();
        ShowLoadingOverlay();
        _browseWebView.SetVisibility(false);
        _browseWebView.LoadURL(recoveryUrl);
        UpdateBrowseBackState();
    }

    private void RecoverExternalWebViewAfterRendererCrash(string detail)
    {
        LogBrowseMessage($"External modal WebView renderer exited ({detail}). Recreating modal browser.");

        DestroyExternalWebView();
        if (!_isExternalBrowserOpen || string.IsNullOrWhiteSpace(_externalBrowseUrl))
        {
            UpdateBrowseBackState();
            return;
        }

        EnsureExternalWebView();
        if (_externalWebView == null)
        {
            ShowExternalBrowserMessageOverlay("This page is unavailable", ExternalBrowseUnavailableMessage);
            UpdateBrowseBackState();
            return;
        }

        SetExternalContentLoaded(false);
        _externalLoadStartedAt = Time.unscaledTime;
        RefreshBrowseLayout();
        ShowExternalBrowserLoadingOverlay();
        _externalWebView.SetVisibility(false);
        _externalWebView.LoadURL(_externalBrowseUrl);
        UpdateBrowseBackState();
    }
#endif
}
