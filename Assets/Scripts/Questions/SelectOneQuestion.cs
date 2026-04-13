using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;

public class SelectOneQuestion : QuestionBase
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private TextAsset jsonFile; // 在 Inspector 中拖入你的 json 檔案

    private VisualElement root;
    private Label promptLabel;
    private VisualElement optionsContainer;
    private Button checkButton;

    private QuizData quizData;
    private int currentLevelIndex = 0;
    private int currentQuestionIndex = 0;
    private int selectedIndex = -1;

    public SelectOneQuestion(VisualElement page, JToken data) : base(page, data)
    {
    }

    void OnEnable()
    {
        root = pageInstance;
        promptLabel = root.Q<Label>(className: "speech-text");
        optionsContainer = root.Q<VisualElement>(className: "options-container");
        checkButton = root.Q<Button>(className: "check-button");

        checkButton.clicked += OnCheckClicked;

        ParseJson();
        LoadQuestion();
    }

    private void ParseJson()
    {
        if (jsonFile != null)
        {
            // 解析 JSON
            quizData = JsonUtility.FromJson<QuizData>(jsonFile.text);
        }
    }

    private void LoadQuestion()
    {
        // 獲取當前題目（這裡以 select_one 類型為範例）
        var currentQuestion = quizData.levels[currentLevelIndex].questions[currentQuestionIndex];
        
        // 更新提示文字
        promptLabel.text = currentQuestion.prompt;

        // 清空現有選項並重新生成
        optionsContainer.Clear();
        selectedIndex = -1;
        checkButton.SetEnabled(false);

        for (int i = 0; i < currentQuestion.options.Count; i++)
        {
            int index = i;
            var option = currentQuestion.options[i];

            // 創建一個新的選項卡片
            VisualElement card = CreateOptionCard(option.text);
            
            // 點擊事件
            card.RegisterCallback<ClickEvent>(evt => SelectOption(index));
            
            optionsContainer.Add(card);
        }
    }

    private VisualElement CreateOptionCard(string content)
    {
        // 這裡你可以使用代碼創建，也可以從另一個 UXML 模板 Instantiate 出來
        var card = new VisualElement();
        card.AddToClassList("option-card");

        var label = new Label(content);
        label.AddToClassList("option-character");
        card.Add(label);

        return card;
    }

    private void SelectOption(int index)
    {
        selectedIndex = index;
        var cards = optionsContainer.Children();
        int i = 0;
        foreach (var card in cards)
        {
            if (i == index) card.AddToClassList("selected");
            else card.RemoveFromClassList("selected");
            i++;
        }
        checkButton.SetEnabled(true);
    }

    private void OnCheckClicked()
    {
        var question = quizData.levels[currentLevelIndex].questions[currentQuestionIndex];
        string selectedId = question.options[selectedIndex].id;

        if (selectedId == question.correctOptionId)
        {
            Debug.Log("回答正確！");
            // 這裡可以切換到下一題
        }
        else
        {
            Debug.Log("回答錯誤，再試一次。");
        }
    }
}