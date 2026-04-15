using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;
using TextSpeech;

public class SpellingQuestion : QuestionBase
{
    private Label promptLabel;
    private TextField answerField;
    private Button audioBtn;
    private Button checkBtn;

    private string correctAnswer;

    public SpellingQuestion(VisualElement page, JToken data)
        : base(page, data)
    {
        InitPage();
    }

    protected override void InitPage()
    {
        // ---- Query UI ----
        promptLabel = pageInstance.Q<Label>("PromptLabel");
        answerField = pageInstance.Q<TextField>("AnswerField");
        audioBtn    = pageInstance.Q<Button>("AudioBtn");

        if (promptLabel == null || answerField == null)
        {
            Debug.LogError("[SpellingQuestion] Required UI elements missing.");
            return;
        }

        // ---- Read JSON ----
        string prompt = questionData.Value<string>("prompt");
        correctAnswer = questionData.Value<string>("answer");

        // ---- Assign UI ----
        promptLabel.text = prompt;
        answerField.value = string.Empty;
        answerField[0][0].style.fontSize = 50;
        answerField[0].style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        // Optional: autofocus
        answerField.Focus();

        // Optional: audio (if you add it later)
        if (audioBtn != null)
        {
            audioBtn.clicked += () =>
            {
                TextToSpeech.Instance.StartSpeak(promptLabel.text);
            };
        }

        checkBtn = pageInstance.Q<Button>("CheckButton");
        checkBtn.clicked += () => {
            if (checkBtn.ClassListContains("isDisabled"))
            {
                return;
            }
            string val = answerField.value;
            onCheck(val.Equals(correctAnswer), correctAnswer);
        };
        // Optional: input change hook
        answerField.RegisterValueChangedCallback(evt =>
        {
            string val = answerField?.value ?? "";
            if (string.IsNullOrWhiteSpace(val))
            {
                checkBtn.AddToClassList("isDisabled");
                return;
            }
            checkBtn.RemoveFromClassList("isDisabled");
        });

    }
}
