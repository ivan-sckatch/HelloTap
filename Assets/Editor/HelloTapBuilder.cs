using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

public static class HelloTapBuilder
{
    public const string ScenePath = "Assets/Scenes/HelloTap.unity";
    private static TMP_FontAsset font;
    private static readonly Color Ink = new Color32(30, 45, 42, 255);
    private static readonly Color Coral = new Color32(239, 101, 91, 255);

    [MenuItem("Hello Tap/Create Scene")]
    public static void CreateScene()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory("Assets/Fonts");
        Directory.CreateDirectory("Assets/Art");
        if (!Directory.Exists("Assets/TextMesh Pro/Resources"))
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMP_Text).Assembly);
            UnityEditor.AssetPackage.Package.Import(Path.Combine(package.resolvedPath, "Package Resources/TMP Essential Resources.unitypackage"), false);
        }
        AssetDatabase.Refresh();
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/HelloTapFont.asset");
        if (font == null)
        {
            var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Arial.ttf");
            if (source == null) throw new InvalidOperationException("Missing Assets/Fonts/Arial.ttf");
            font = TMP_FontAsset.CreateFontAsset(source, 80, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
            font.name = "HelloTapFont";
            font.isMultiAtlasTexturesEnabled = true;
            AssetDatabase.CreateAsset(font, "Assets/Fonts/HelloTapFont.asset");
            AssetDatabase.AddObjectToAsset(font.material, font);
            foreach (var atlas in font.atlasTextures) AssetDatabase.AddObjectToAsset(atlas, font);
            font.TryAddCharacters("Hello Tap0123456789\u0421\u0447\u0451\u0442\u0422\u0430\u043f\u043d\u0438 \u043c\u0435\u043d\u044f\u0431\u0440\u043e\u0441");
            EditorUtility.SetDirty(font);
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Main Camera", typeof(Camera)).GetComponent<Camera>();
        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.transform.position = new Vector3(0, 0, -10);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(243, 249, 246, 255);

        var canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        var safe = Rect("Safe Area", canvas.transform);
        Stretch(safe);
        safe.gameObject.AddComponent<SafeArea>();

        var title = Label("Title", safe, "Hello Tap", 92, Ink);
        Place(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -155), new Vector2(940, 150));
        title.fontStyle = FontStyles.Bold;
        var line = Rect("Accent", safe);
        Place(line, new Vector2(0.5f, 1), new Vector2(0, -265), new Vector2(72, 8));
        line.gameObject.AddComponent<Image>().color = Coral;

        var caption = Label("Score Label", safe, "\u0421\u0447\u0451\u0442", 46, new Color32(82, 108, 99, 255));
        Place(caption.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0, 130), new Vector2(900, 80));
        var score = Label("Score", safe, "0", 210, Ink);
        Place(score.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0, -25), new Vector2(940, 250));
        score.fontStyle = FontStyles.Bold;
        score.enableAutoSizing = true;
        score.fontSizeMin = 65;
        score.fontSizeMax = 210;

        var tap = MakeButton("Tap Button", safe, "\u0422\u0430\u043f\u043d\u0438 \u043c\u0435\u043d\u044f", Coral, Color.white);
        Place((RectTransform)tap.transform, new Vector2(0.5f, 0), new Vector2(0, 430), new Vector2(470, 470));
        tap.GetComponent<Image>().sprite = CreateTapSprite();
        tap.GetComponent<Image>().type = Image.Type.Simple;
        var tapLabel = tap.GetComponentInChildren<TMP_Text>();
        tapLabel.fontSize = 48;
        tapLabel.rectTransform.offsetMin = new Vector2(40, 0);
        tapLabel.rectTransform.offsetMax = new Vector2(-40, 0);

        var reset = MakeButton("Reset Button", safe, "\u0421\u0431\u0440\u043e\u0441", new Color32(226, 238, 231, 255), Ink);
        Place((RectTransform)reset.transform, new Vector2(0.5f, 0), new Vector2(0, 105), new Vector2(290, 90));
        // Serialized references and persistent listeners remain visible in the Inspector.
        var counter = canvas.AddComponent<TapCounter>();
        var serialized = new SerializedObject(counter);
        serialized.FindProperty("scoreText").objectReferenceValue = score;
        serialized.FindProperty("tapButton").objectReferenceValue = tap.transform;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        UnityEventTools.AddPersistentListener(tap.onClick, counter.Increment);
        UnityEventTools.AddPersistentListener(reset.onClick, counter.ResetCount);
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        ConfigureAndroid();
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
    }

    private static RectTransform Rect(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static TMP_Text Label(string name, Transform parent, string value, int size, Color color)
    {
        var label = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font;
        label.text = value;
        label.fontSize = size;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }

    private static Button MakeButton(string name, Transform parent, string caption, Color background, Color textColor)
    {
        var rect = Rect(name, parent);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = background;
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f);
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        var label = Label("Label", rect, caption, 38, textColor);
        label.fontStyle = FontStyles.Bold;
        Stretch(label.rectTransform);
        return button;
    }

    private static Sprite CreateTapSprite()
    {
        const string path = "Assets/Art/TapCircle.png";
        if (!File.Exists(path))
        {
            var texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            var pixels = new Color[256 * 256];
            for (int y = 0; y < 256; y++)
                for (int x = 0; x < 256; x++)
                    pixels[y * 256 + x] = new Color(1, 1, 1, Mathf.Clamp01(126.5f - Vector2.Distance(new Vector2(x, y), new Vector2(127.5f, 127.5f))));
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void ConfigureAndroid()
    {
        PlayerSettings.companyName = "Skachkov41";
        PlayerSettings.productName = "Hello Tap";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.skachkov41.hellotap");
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.defaultScreenWidth = 540;
        PlayerSettings.defaultScreenHeight = 960;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        EditorUserBuildSettings.buildAppBundle = false;
    }

    [MenuItem("Hello Tap/Build Android APK")]
    public static void BuildAndroid()
    {
        if (!File.Exists(ScenePath)) CreateScene();
        ConfigureAndroid();
        Directory.CreateDirectory("Builds/Android");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath }, target = BuildTarget.Android,
            locationPathName = "Builds/Android/HelloTap_Skachkov41.apk", options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("Android build failed: " + report.summary.result);
        Debug.Log("HELLO_TAP_APK_OK: " + report.summary.totalSize);
    }

    public static void PrepareAndVerify()
    {
        CreateScene();
        HelloTapVerification.Run();
    }
}
