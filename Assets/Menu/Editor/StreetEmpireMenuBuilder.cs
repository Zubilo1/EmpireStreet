using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.IO;

public static class StreetEmpireMenuBuilder
{
    private const string ScenePath = "Assets/Scenes/Menu.unity";

    [MenuItem("Street Empire/Build Visual Menu Scene")]
    public static void BuildMenu()
    {
        if (!Directory.Exists("Assets/Scenes"))
            Directory.CreateDirectory("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Menu";

        var cameraGO = new GameObject("Main Camera");
        var camera = cameraGO.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.02f, 0.025f, 0.04f);
        camera.orthographic = true;
        cameraGO.tag = "MainCamera";
        cameraGO.AddComponent<AudioListener>();

        var canvasGO = new GameObject("Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var background = Make("Background", canvasGO.transform, typeof(RawImage));
        Stretch(background.GetComponent<RectTransform>());
        var raw = background.GetComponent<RawImage>();
        raw.texture = Resources.Load<Texture2D>("MenuBackground");
        raw.color = raw.texture != null ? Color.white : new Color(0.035f, 0.045f, 0.065f);

        // Dark overlay on the left makes UI readable while keeping the art visible.
        var overlay = Make("Left Dark Overlay", canvasGO.transform, typeof(Image));
        var ort = overlay.GetComponent<RectTransform>();
        ort.anchorMin = Vector2.zero;
        ort.anchorMax = new Vector2(0.48f, 1f);
        ort.offsetMin = Vector2.zero; ort.offsetMax = Vector2.zero;
        overlay.GetComponent<Image>().color = new Color(0.015f, 0.02f, 0.03f, 0.58f);

        var logo = MakeText("STREET\nEMPIRE", canvasGO.transform, 78, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
        var lrt = logo.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.065f, 0.64f);
        lrt.anchorMax = new Vector2(0.42f, 0.90f);
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        logo.GetComponent<Text>().lineSpacing = 0.82f;

        var subtitle = MakeText("ГОРОД ТВОИХ ПРАВИЛ", canvasGO.transform, 24, FontStyle.Normal,
            new Color(1f, 0.28f, 0.18f), TextAnchor.MiddleLeft);
        var srt = subtitle.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0.07f, 0.59f);
        srt.anchorMax = new Vector2(0.42f, 0.65f);
        srt.offsetMin = Vector2.zero; srt.offsetMax = Vector2.zero;

        var holder = new GameObject("Menu Buttons", typeof(RectTransform));
        holder.transform.SetParent(canvasGO.transform, false);
        var hrt = holder.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0.06f, 0.17f);
        hrt.anchorMax = new Vector2(0.36f, 0.55f);
        hrt.offsetMin = Vector2.zero; hrt.offsetMax = Vector2.zero;
        var layout = holder.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 13;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        var controller = canvasGO.AddComponent<StreetEmpireMenuController>();
        controller.gameSceneName = "Game";

        AddButton(holder.transform, "▶    НОВАЯ ИГРА", () => controller.StartGame(), true);
        AddButton(holder.transform, "▣    ПРОДОЛЖИТЬ", () => controller.ContinueGame(), false);
        AddButton(holder.transform, "⚙    НАСТРОЙКИ", () => controller.ToggleSettings(), false);
        AddButton(holder.transform, "↪    ВЫХОД", () => controller.ExitGame(), false);

        var footer = MakeText("PROTOTYPE BUILD 0.1", canvasGO.transform, 17, FontStyle.Normal,
            new Color(0.68f, 0.70f, 0.74f), TextAnchor.MiddleLeft);
        var frt = footer.GetComponent<RectTransform>();
        frt.anchorMin = new Vector2(0.07f, 0.045f);
        frt.anchorMax = new Vector2(0.35f, 0.09f);
        frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;

        var settings = Make("Settings Panel", canvasGO.transform, typeof(Image));
        var prt = settings.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.56f, 0.28f);
        prt.anchorMax = new Vector2(0.88f, 0.62f);
        prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;
        settings.GetComponent<Image>().color = new Color(0.035f, 0.04f, 0.055f, 0.97f);
        var settingsText = MakeText("НАСТРОЙКИ\n\nПараметры появятся позже.", settings.transform, 27,
            FontStyle.Normal, Color.white, TextAnchor.MiddleCenter);
        Stretch(settingsText.GetComponent<RectTransform>());
        settings.SetActive(false);
        controller.settingsPanel = settings;

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true),
            new EditorBuildSettingsScene("Assets/Scenes/Game.unity", File.Exists("Assets/Scenes/Game.unity"))
        };

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        Debug.Log("Street Empire visual menu created at " + ScenePath);
        EditorUtility.DisplayDialog("Street Empire", "Визуальное меню создано!\n\nОткрой Assets/Scenes/Menu.unity и нажми Play.\nКнопка Новая игра загрузит сцену Game, когда она будет создана.", "OK");
    }

    private static GameObject Make(string name, Transform parent, params System.Type[] components)
    {
        var go = new GameObject(name, components);
        go.transform.SetParent(parent, false);
        return go;
    }

    private static Text MakeText(string value, Transform parent, int size, FontStyle style, Color color, TextAnchor alignment)
    {
        var go = Make(value, parent, typeof(Text));
        var text = go.GetComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private static void AddButton(Transform parent, string label, UnityEngine.Events.UnityAction action, bool primary)
    {
        var go = Make(label + " Button", parent, typeof(Image), typeof(Button), typeof(LayoutElement));
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 68);
        var image = go.GetComponent<Image>();
        image.color = primary ? new Color(0.76f, 0.045f, 0.035f, 0.96f) : new Color(0.025f, 0.03f, 0.04f, 0.82f);
        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = primary ? new Color(0.95f, 0.10f, 0.07f, 1f) : new Color(0.15f, 0.16f, 0.18f, 0.95f);
        colors.pressedColor = new Color(0.48f, 0.025f, 0.02f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.onClick.AddListener(action);

        var labelGO = Make("Label", go.transform, typeof(Text));
        var text = labelGO.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = label;
        text.fontSize = 25;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        var trt = text.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.08f, 0); trt.anchorMax = new Vector2(0.96f, 1);
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        go.GetComponent<LayoutElement>().preferredHeight = 68;
    }
}
