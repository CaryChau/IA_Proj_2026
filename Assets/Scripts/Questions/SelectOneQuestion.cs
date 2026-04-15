using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;

public class SelectOneQuestion : QuestionBase
{
    private Label _promptLabel;
    private Button _checkButton;
    private List<VisualElement> _optionCards = new List<VisualElement>();
    
    private string _correctOptionId;
    private string _currentSelectionId;

    public SelectOneQuestion(VisualElement page, JToken data) : base(page, data)
    {
        InitPage();
    }

    protected override void InitPage()
    {
        _promptLabel = pageInstance.Q<Label>(className: "speech-text");
        _checkButton = pageInstance.Q<Button>(className: "check-button");

        _optionCards = pageInstance.Query<VisualElement>(className: "option-card").ToList();

        string prompt = questionData.Value<string>("prompt");
        _correctOptionId = questionData.Value<string>("correctOptionId");
        JArray options = questionData["options"] as JArray;

        if (_promptLabel != null) _promptLabel.text = prompt;

        for (int i = 0; i < _optionCards.Count; i++)
        {
            if (options != null && i < options.Count)
            {
                var optData = options[i];
                string id = optData.Value<string>("id");
                string text = optData.Value<string>("text");

                var label = _optionCards[i].Q<Label>(className: "option-character");
                if (label != null) label.text = text;

                VisualElement capturedCard = _optionCards[i];
                string capturedId = id;

                capturedCard.RegisterCallback<ClickEvent>(evt => OnCardClicked(capturedId, capturedCard));
        
                capturedCard.RemoveFromClassList("selected");
            }
            else
            {
                _optionCards[i].style.display = DisplayStyle.None;
            }
        }

        if (_checkButton != null)
        {
            _checkButton.clicked += () => 
            {
                if (string.IsNullOrEmpty(_currentSelectionId)) return;
                
                onCheck?.Invoke(_currentSelectionId == _correctOptionId, _correctOptionId);
            };
        }
    }

    private void OnCardClicked(string id, VisualElement clickedCard)
    {
        foreach (var card in _optionCards)
        {
            card.RemoveFromClassList("selected");
        }

        clickedCard.AddToClassList("selected");
        _currentSelectionId = id;

        Debug.Log($"Selected option: {id}");
    }
}