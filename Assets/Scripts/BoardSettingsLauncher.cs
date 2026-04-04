using Android;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

#if BOARD_SDK_PRESENT
using Board.Input.UI;
#endif

/// <summary>
/// Minimal runtime bootstrap that creates a full-screen canvas and a single button.
/// Tapping the button launches the Android Settings app.
/// </summary>
public class BoardSettingsLauncher : MonoBehaviour
{
    private const string ButtonLabel = "Open Android Settings";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindAnyObjectByType<BoardSettingsLauncher>(FindObjectsInactive.Include) != null)
            return;

        var root = new GameObject(nameof(BoardSettingsLauncher));
        DontDestroyOnLoad(root);
        root.AddComponent<BoardSettingsLauncher>();
    }

    private void Awake()
    {
        EnsureEventSystem();
        CreateUi();
    }

    private static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include) != null)
            return;

        var eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<EventSystem>();

#if UNITY_ANDROID && !UNITY_EDITOR && BOARD_SDK_PRESENT
        eventSystemGo.AddComponent<BoardUIInputModule>();
#else
    #if ENABLE_INPUT_SYSTEM
        eventSystemGo.AddComponent<InputSystemUIInputModule>();
    #else
        eventSystemGo.AddComponent<StandaloneInputModule>();
    #endif
#endif
    }

    private void CreateUi()
    {
        var canvasGo = new GameObject("Canvas");
        DontDestroyOnLoad(canvasGo);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<GraphicRaycaster>();

        var buttonGo = new GameObject("LaunchSettingsButton");
        buttonGo.transform.SetParent(canvasGo.transform, false);

        var buttonImage = buttonGo.AddComponent<Image>();
        var button = buttonGo.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(OpenAndroidSettings);

        var rect = buttonGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(700f, 180f);
        rect.anchoredPosition = Vector2.zero;

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(buttonGo.transform, false);

        var text = textGo.AddComponent<Text>();
        text.text = ButtonLabel;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 54;
        text.color = Color.black;

        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    private static void OpenAndroidSettings()
    {
        AndroidUtility.OpenAndroidSettings();
    }
}
