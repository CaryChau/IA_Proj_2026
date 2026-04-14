using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;

public class TrueFalseQuestion : QuestionBase
{
    private Label speechLabel;
    private VisualElement trueCard;
    private VisualElement falseCard;
    private Button checkButton;

    private bool correctAnswer;
    private bool? currentChoice = null;

    public TrueFalseQuestion(VisualElement page, JToken data) : base(page, data)
    {
        InitPage();
    }

    private void InitPage()
    {
        speechLabel = pageInstance.Q<Label>(className: "speech-text");

        var cards = pageInstance.Query<VisualElement>(className: "option-card").ToList();
        
        foreach (var card in cards)
        {
            var label = card.Q<Label>();
            if (label == null) continue;

            if (label.text == "True")
                trueCard = card;
            else if (label.text == "False")
                falseCard = card;

            card.RegisterCallback<ClickEvent>(evt => OnCardClicked(card));
        }

        checkButton = pageInstance.Q<Button>(className: "check-button");

        string prompt = questionData.Value<string>("prompt");
        correctAnswer = questionData.Value<bool>("answer");

        if (speechLabel != null)
            speechLabel.text = prompt;

        if (checkButton != null)
        {
            checkButton.clicked += () =>
            {
                if (currentChoice.HasValue)
                {
                    onCheck?.Invoke(currentChoice.Value == correctAnswer);
                }
                else
                {
                    Debug.Log("Please select one answer");
                }
            };
        }
        
        ResetCardStyles();
    }

    private void OnCardClicked(VisualElement clickedCard)
    {
        ResetCardStyles();

        clickedCard.AddToClassList("selected");

        if (clickedCard == trueCard)
            currentChoice = true;
        else if (clickedCard == falseCard)
            currentChoice = false;
    }

    private void ResetCardStyles()
    {
        trueCard?.RemoveFromClassList("selected");
        falseCard?.RemoveFromClassList("selected");
    }
}