using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
public class LevelData
{
    public int LevelIndex;
    public string Title;
}
public class LevelCreator : MonoBehaviour
{
    private VisualElement root;
    private VisualElement levelPage;
    private VisualElement contentRoot;

    private List<LevelData> levels = new List<LevelData>();

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        levelPage = root.Q<VisualElement>("LevelPage");
        contentRoot = root.Q<VisualElement>("Root-Content");

        GenerateLevelData();
        SetupLevelButtons();
    }

    void GenerateLevelData()
    {
        for (int i = 1; i <= 4; i++)
        {
            levels.Add(new LevelData
            {
                LevelIndex = i,
                Title = "Level " + i
            });
        }
    }

    void SetupLevelButtons()
    {
        for (int i = 0; i < levels.Count; i++)
        {
            var button = root.Q<Button>("LevelButton" + (i + 1));

            int capturedIndex = i;

            button.text = levels[i].LevelIndex.ToString();

            button.clicked += () =>
            {
                OpenLevel(levels[capturedIndex]);
            };
        }
    }

    void OpenLevel(LevelData data)
    {
        // Hide LevelPage
        levelPage.EnableInClassList("ActivePage", false);

        // Load MC Question UXML
        VisualTreeAsset levelAsset =
            Resources.Load<VisualTreeAsset>("LevelDetailPage");

        VisualElement levelDetail = levelAsset.CloneTree();
        levelDetail.name = "LevelDetailPage";
        levelDetail.AddToClassList("Page");
        levelDetail.AddToClassList("ActivePage");

        contentRoot.Add(levelDetail);

        SetupLevelDetail(levelDetail, data);
    }

    void SetupLevelDetail(VisualElement levelDetail, LevelData data)
    {
        var questionLabel = levelDetail.Q<Label>("QuestionLabel");
        questionLabel.text = "What is 2 + 2?";

        var optionA = levelDetail.Q<Button>("OptionA");
        var optionB = levelDetail.Q<Button>("OptionB");
        var optionC = levelDetail.Q<Button>("OptionC");

        optionA.text = "3";
        optionB.text = "4";
        optionC.text = "5";

        optionB.clicked += () =>
        {
            Debug.Log("Correct!");
        };

        var backButton = levelDetail.Q<Button>("BackButton");
        backButton.clicked += () =>
        {
            levelDetail.RemoveFromHierarchy();
            levelPage.EnableInClassList("ActivePage", true);
        };
    }
}