using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public static class HelloTapVerification
{
    [MenuItem("Hello Tap/Verify Scene")]
    public static void Run()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(HelloTapBuilder.ScenePath);
        var counter = UnityEngine.Object.FindAnyObjectByType<TapCounter>();
        var score = GameObject.Find("Score").GetComponent<TMP_Text>();
        var tap = GameObject.Find("Tap Button").GetComponent<Button>();
        var reset = GameObject.Find("Reset Button").GetComponent<Button>();
        // Inspector listeners are RuntimeOnly by default; enable them for this editor check.
        tap.onClick.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
        reset.onClick.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
        bool hadSave = PlayerPrefs.HasKey(TapCounter.SaveKey);
        int saved = PlayerPrefs.GetInt(TapCounter.SaveKey);
        Directory.CreateDirectory("Builds/Verification");
        try
        {
            counter.ResetCount();
            Check(score.text == "0", "Reset UI");
            Color firstColor = score.color;
            // Exercise the same event path used by a pointer click, including Inspector listeners.
            var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            for (int i = 0; i < 10; i++) ExecuteEvents.Execute(tap.gameObject, pointer, ExecuteEvents.pointerClickHandler);
            Check(counter.Count == 10 && score.text == "10", "10 pointer clicks");
            Check(score.color != firstColor, "Color changes at 10");
            counter.SendMessage("OnApplicationPause", true);
            counter.SendMessage("Start");
            Check(counter.Count == 10, "Saved score loads on start");
            Capture(1080, 1920);
            Capture(720, 1600);
            Capture(1200, 1600);
            ExecuteEvents.Execute(reset.gameObject, pointer, ExecuteEvents.pointerClickHandler);
            Check(counter.Count == 0 && PlayerPrefs.GetInt(TapCounter.SaveKey) == 0, "Reset button clears saved score");
            PlayerPrefs.SetInt(TapCounter.SaveKey, int.MaxValue);
            counter.SendMessage("Start");
            counter.Increment();
            Check(counter.Count == int.MaxValue, "Overflow protection");
            PlayerPrefs.SetInt(TapCounter.SaveKey, -12);
            counter.SendMessage("Start");
            Check(counter.Count == 0, "Invalid saved score sanitised");
            File.WriteAllText("Builds/Verification/results.txt", "PASS: pointer clicks, reset, color at 10, pause persistence, startup restore, overflow, invalid save.\nRendered: 1080x1920, 720x1600, 1200x1600.\nPhysical Android device: not verified.\n");
            Debug.Log("HELLO_TAP_VERIFICATION_OK");
        }
        finally
        {
            if (hadSave) PlayerPrefs.SetInt(TapCounter.SaveKey, saved);
            else PlayerPrefs.DeleteKey(TapCounter.SaveKey);
            PlayerPrefs.Save();
            EditorSceneManager.OpenScene(HelloTapBuilder.ScenePath);
        }
    }

    private static void Check(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException("Hello Tap verification failed: " + name);
    }

    private static void Capture(int width, int height)
    {
        var canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        var camera = Camera.main;
        var safe = GameObject.Find("Safe Area").GetComponent<RectTransform>();
        safe.GetComponent<SafeArea>().enabled = false;
        safe.anchorMin = Vector2.zero;
        safe.anchorMax = Vector2.one;
        safe.offsetMin = safe.offsetMax = Vector2.zero;
        var target = new RenderTexture(width, height, 24);
        camera.targetTexture = target;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1;
        // Use target dimensions rather than the batch editor's desktop resolution.
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.enabled = false;
        canvas.scaleFactor = Mathf.Sqrt((width / 1080f) * (height / 1920f));
        Canvas.ForceUpdateCanvases();
        foreach (var label in canvas.GetComponentsInChildren<TMP_Text>()) label.ForceMeshUpdate();
        var tapBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvas.transform, GameObject.Find("Tap Button").transform);
        var resetBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvas.transform, GameObject.Find("Reset Button").transform);
        var scoreBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvas.transform, GameObject.Find("Score").transform);
        Check(tapBounds.min.y > resetBounds.max.y, "Buttons do not overlap");
        Check(scoreBounds.min.y > tapBounds.max.y, "Score does not overlap tap button");
        camera.Render();
        var previous = RenderTexture.active;
        RenderTexture.active = target;
        var image = new Texture2D(width, height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();
        File.WriteAllBytes($"Builds/Verification/HelloTap-{width}x{height}.png", image.EncodeToPNG());
        RenderTexture.active = previous;
        camera.targetTexture = null;
        UnityEngine.Object.DestroyImmediate(image);
        UnityEngine.Object.DestroyImmediate(target);
    }
}
