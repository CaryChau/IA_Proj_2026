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

    // void GenerateLevelData()
    // {
    //     for (int i = 1; i <= 4; i++)
    //     {
    //         levels.Add(new LevelData
    //         {
    //             LevelIndex = i,
    //             Title = "Level " + i
    //         });
    //     }
    // }

    // void SetupLevelButtons()
    // {
    //     for (int i = 0; i < levels.Count; i++)
    //     {
    //         var button = root.Q<Button>("LevelButton" + (i + 1));

    //         int capturedIndex = i;

    //         button.text = levels[i].LevelIndex.ToString();

    //         button.clicked += () =>
    //         {
    //             OpenLevel(levels[capturedIndex]);
    //         };
    //     }
    // }

    // void OpenLevel(LevelData data)
    // {
    //     // Hide LevelPage
    //     levelPage.EnableInClassList("ActivePage", false);

    //     // Load MC Question UXML
    //     VisualTreeAsset levelAsset =
    //         Resources.Load<VisualTreeAsset>("UIDocuments/MCLevelPage");

    //     VisualElement levelDetail = levelAsset.CloneTree();
    //     levelDetail.name = "MCLevelPage";
    //     levelDetail.AddToClassList("Page");
    //     levelDetail.AddToClassList("ActivePage");

    //     contentRoot.Add(levelDetail);

    //     SetupLevelDetail(levelDetail, data);
    // }

    // void SetupLevelDetail(VisualElement levelDetail, LevelData data)
    // {
    //     var questionLabel = levelDetail.Q<Label>("QuestionLabel");
    //     questionLabel.text = "What is 2 + 2?";

    //     var optionA = levelDetail.Q<Button>("OptionA");
    //     var optionB = levelDetail.Q<Button>("OptionB");
    //     var optionC = levelDetail.Q<Button>("OptionC");

    //     optionA.text = "3";
    //     optionB.text = "4";
    //     optionC.text = "5";

    //     optionB.clicked += () =>
    //     {
    //         Debug.Log("Correct!");
    //     };

    //     var backButton = levelDetail.Q<Button>("BackButton");
    //     backButton.clicked += () =>
    //     {
    //         levelDetail.RemoveFromHierarchy();
    //         levelPage.EnableInClassList("ActivePage", true);
    //     };
    // }

    private ScrollView scrollView;

    private int levelsPerCharacter = 4;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        levelPage = root.Q<VisualElement>("LevelPage");

        scrollView = levelPage.Q<ScrollView>("Root-LevelList-ScrollView");

        GenerateLevelRows(5); // Create 5 character rows (20 levels total)
    }

    void GenerateLevelRows(int characterCount)
    {
        scrollView.Clear();

        int levelNumber = 1;

        for (int i = 0; i < characterCount; i++)
        {
            VisualElement characterRow = CreateCharacterRow(ref levelNumber);
            scrollView.Add(characterRow);
        }
    }

    VisualElement CreateCharacterRow(ref int levelNumber)
    {
        // Root Row
        VisualElement characterRow = new VisualElement();
        characterRow.AddToClassList("CharacterRow");

        // Character Icon
        VisualElement characterIcon = new VisualElement();
        characterIcon.AddToClassList("CharacterIcon");

        // Optional: randomly tint icon
        characterIcon.style.backgroundColor =
            new Color(Random.value, Random.value, Random.value);

        characterRow.Add(characterIcon);

        // Curved Path Container
        VisualElement curvedContainer = new VisualElement();
        curvedContainer.AddToClassList("CurvedPathContainer");

        for (int i = 0; i < levelsPerCharacter; i++)
        {
            VisualElement levelRow = new VisualElement();
            levelRow.AddToClassList("LevelButtonRow");

            if (i % 2 == 0)
                levelRow.AddToClassList("LeftAlign");
            else
                levelRow.AddToClassList("RightAlign");

            Button levelButton = new Button();
            levelButton.text = levelNumber.ToString();
            levelButton.AddToClassList("LevelButton");

            levelButton.style.fontSize = 60;
            levelButton.style.unityFontStyleAndWeight = FontStyle.Bold;

            int capturedLevel = levelNumber; // prevent closure bug
            levelButton.clicked += () =>
            {
                OnLevelClicked(capturedLevel);
            };

            levelRow.Add(levelButton);
            curvedContainer.Add(levelRow);

            levelNumber++;
        }

        characterRow.Add(curvedContainer);

        return characterRow;
    }

    void OnLevelClicked(int level)
    {
        Debug.Log("Clicked Level: " + level);

        // TODO:
        // Load MC Question Page here
        // Example:
        // LoadLevelPage(level);
    }
}