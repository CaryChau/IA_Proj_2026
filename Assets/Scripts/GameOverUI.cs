using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ResultData
{
    public int exp;
    public int accuracy;
    public string timeStr;

}

public class GameOverUI
{
    private int exp;
    private int accuracy;
    private string timeStr; 
    private VisualElement root;
    private Action onDelete;
    public GameOverUI(VisualElement pageInstance, ResultData data, Action onDelete)
    {
        root = pageInstance;
        exp = data.exp;
        accuracy = data.accuracy;
        timeStr = data.timeStr;
        this.onDelete = onDelete;
        InitPage();
    }

    private void InitPage()
    {
        // ---- Query labels by name ----
        var expLabel = root.Q<Label>("exp-value");
        var accuracyLabel = root.Q<Label>("accuracy-value");
        var timeLabel = root.Q<Label>("time-value");

        if (expLabel == null || accuracyLabel == null || timeLabel == null)
        {
            Debug.LogError("[GameOverUI] One or more value labels not found.");
            return;
        }

        // ---- EXP ----
        expLabel.text = exp.ToString();

        // ---- Accuracy ----
        // Support both 0–1 and 0–100 inputs
        float accuracyPercent = accuracy <= 1f ? accuracy * 100f : accuracy;
        accuracyLabel.text = $"{Mathf.RoundToInt(accuracyPercent)}%";

        // ---- Time ----
        timeLabel.text = timeStr;

        var claimBtn = root.Q<Button>(className: "primary-button");
        claimBtn.clicked += onDelete;
    }


}