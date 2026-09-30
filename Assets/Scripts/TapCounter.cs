using TMPro;
using UnityEngine;

public sealed class TapCounter : MonoBehaviour
{
    public const string SaveKey = "HelloTap.Score";
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private RectTransform tapButton;
    private int count;
    private float pulse;
    private static readonly Color[] ScoreColors =
    {
        new Color32(30, 45, 42, 255), new Color32(0, 119, 102, 255),
        new Color32(194, 65, 74, 255), new Color32(39, 98, 177, 255)
    };

    public int Count => count;

    private void Start()
    {
        // PlayerPrefs also survives Android process termination after a pause.
        count = Mathf.Max(0, PlayerPrefs.GetInt(SaveKey, 0));
        UpdateUI();
    }

    public void Increment()
    {
        if (count < int.MaxValue) count++;
        pulse = 1f;
        UpdateUI();
        PlayerPrefs.SetInt(SaveKey, count);
    }

    public void ResetCount()
    {
        count = 0;
        UpdateUI();
        Save();
    }

    private void Update()
    {
        // A short pulse acknowledges a tap without changing the button layout.
        pulse = Mathf.MoveTowards(pulse, 0f, Time.unscaledDeltaTime * 6f);
        if (tapButton != null) tapButton.localScale = Vector3.one * (1f - pulse * 0.045f);
    }

    private void UpdateUI()
    {
        if (scoreText == null) return;
        scoreText.text = count.ToString();
        scoreText.color = ScoreColors[(count / 10) % ScoreColors.Length];
    }

    private void Save()
    {
        PlayerPrefs.SetInt(SaveKey, count);
        PlayerPrefs.Save();
    }

    private void OnApplicationPause(bool paused) { if (paused) Save(); }
    private void OnApplicationFocus(bool focused) { if (!focused) Save(); }
    private void OnApplicationQuit() => Save();
}
