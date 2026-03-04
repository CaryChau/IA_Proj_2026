using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
public class LevelCreator : MonoBehaviour
{
    private VisualElement root;
    private VisualElement levelPage;
    private VisualElement contentRoot;

    private List<VisualElement> pages;
    private List<Button> navButtons;

    private ScrollView scrollView;
    private TopBarCreator topBarCreator;

    private int levelsPerCharacter = 4;

    void Start()
    {
        topBarCreator = GetComponent<TopBarCreator>();
        root = GetComponent<UIDocument>().rootVisualElement;
        levelPage = root.Q<VisualElement>("LevelPage");

        scrollView = levelPage.Q<ScrollView>("Root-LevelList-ScrollView");
        SetupPages();
        InitBottomBar();
        GenerateLevelRows(5); // Create 5 character rows (20 levels total)
    }

    private void InitBottomBar()
    {
        VisualElement bottomBar = root.Q<VisualElement>("Root-BottomBar");

        navButtons = bottomBar.Query<Button>().ToList();

        foreach (var button in navButtons)
        {
            button.clicked += () => {
                OnNavClicked(button);
            };
            Debug.Log("Found button: " + button.name);
        }
    }


    void SetupPages()
    {
        pages = root.Query<VisualElement>(className: "Page").ToList();
        foreach (var page in pages)
        {
            Debug.Log("Found page: " + page.name);
        }
    }

    void OnNavClicked(Button clickedButton)
    {
        // Remove selected state from all nav buttons
        foreach (var btn in navButtons)
            btn.RemoveFromClassList("NavIconSelected");

        // Add selected state to clicked button
        clickedButton.AddToClassList("NavIconSelected");

        // Determine which page to show
        string targetPageName = clickedButton.name.Replace("Nav", "Page");

        // Hide all pages
        foreach (var page in pages)
            page.RemoveFromClassList("ActivePage");

        // Show target page
        var targetPage = root.Q<VisualElement>(targetPageName);
        if (targetPage != null)
        {
            targetPage.AddToClassList("ActivePage");
            topBarCreator.ShowTopBar(targetPageName);
        }
            
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