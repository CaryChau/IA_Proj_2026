using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;

public class ListeningQuestion : QuestionBase
{
    private Label questionLabel;
    private Label transcriptLabel;

    private readonly Dictionary<string, Button> optionButtons =
        new Dictionary<string, Button>();

    private string correctOptionId;
    private string curChoise;

    public ListeningQuestion(VisualElement page, JToken data)
        : base(page, data)
    {
        InitPage();
    }

    private void InitPage()
    {
        // ---------- 1. Query UI elements ----------
        questionLabel   = pageInstance.Q<Label>("QuestionLabel");
        transcriptLabel = pageInstance.Q<Label>("TranscriptLabel");

        optionButtons.Clear();
        optionButtons["A"] = pageInstance.Q<Button>("OptionA");
        optionButtons["B"] = pageInstance.Q<Button>("OptionB");
        optionButtons["C"] = pageInstance.Q<Button>("OptionC");

        // Defensive checks (very important in UITK)
        if (questionLabel == null)
            Debug.LogError("[ListeningQuestion] QuestionLabel not found.");
        if (transcriptLabel == null)
            Debug.LogError("[ListeningQuestion] TranscriptLabel not found.");

        // ---------- 2. Read JSON fields ----------
        string prompt = questionData.Value<string>("prompt");
        string transcript = questionData.Value<string>("audioTranscript");
        correctOptionId = questionData.Value<string>("correctOptionId");

        // ---------- 3. Assign question text ----------
        if (questionLabel != null)
            questionLabel.text = prompt;

        if (transcriptLabel != null)
        {
            transcriptLabel.text = transcript;

            // Optional: hide transcript initially
            transcriptLabel.AddToClassList("isHidden");
        }

        // ---------- 4. Assign options ----------
        JArray options = questionData["options"] as JArray;
        if (options == null)
        {
            Debug.LogError("[ListeningQuestion] options array missing.");
            return;
        }

        foreach (var opt in options)
        {
            string id   = opt.Value<string>("id");   // "A", "B", "C"
            string text = opt.Value<string>("text");

            if (!optionButtons.TryGetValue(id, out var btn) || btn == null)
            {
                Debug.LogWarning($"[ListeningQuestion] Option button {id} not found.");
                continue;
            }

            btn.text = $"{id}. {text}";

            // Clear any previous visual state
            btn.RemoveFromClassList("selected");
            btn.RemoveFromClassList("correct");
            btn.RemoveFromClassList("incorrect");

            // Capture id for callback
            string capturedId = id;
            btn.clicked += () => OnOptionSelected(capturedId);
        }

        var checkBtn = pageInstance.Q<Button>("CheckButton");
        checkBtn.clicked += () => {
            onCheck(curChoise == correctOptionId);
        };
    }

    // ---------- Selection logic (no evaluation yet) ----------
    private void OnOptionSelected(string selectedId)
    {
        foreach (var kv in optionButtons)
        {
            kv.Value.RemoveFromClassList("selected");
        }

        if (optionButtons.TryGetValue(selectedId, out var btn))
        {
            btn.AddToClassList("selected");
        }

        curChoise = selectedId;
    }
}
