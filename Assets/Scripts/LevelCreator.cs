using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.VisualScripting;
public class LevelCreator : MonoBehaviour
{
    private VisualElement root;
    private VisualElement levelPage;
    private VisualElement contentRoot;
    public VisualTreeAsset characterRowTemplate;
    private List<VisualElement> pages;
    private List<Button> navButtons;

    private ScrollView scrollView;
    private TopBarCreator topBarCreator;
    private string curNavName = null;
    private int levelsPerCharacter = 4;
    void Start()
    {
        topBarCreator = GetComponent<TopBarCreator>();
        root = GetComponent<UIDocument>().rootVisualElement;
        levelPage = root.Q<VisualElement>("LevelPage");
        // characterRowTemplate = root.Q<VisualElement>("CharacterRowTemplate");
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
            button.clicked += () =>
            {
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
        if (curNavName == clickedButton.name)
        {
            return;
        }
        curNavName = clickedButton.name;
        // Remove selected state from all nav buttons
        foreach (var btn in navButtons)
            btn.RemoveFromClassList("NavIconSelected");

        // Add selected state to clicked button
        clickedButton.AddToClassList("NavIconSelected");

        // Determine which page to show
        string targetPageName = clickedButton.name.Replace("Nav", "Page");
        VisualElement lastPage = null;
        // Hide all pages
        foreach (var page in pages)
        {
            if (page.ClassListContains("ActivePage"))
            {
                page.RemoveFromClassList("ActivePage");
                lastPage = page;
            }
        }

        // Show target page
        var targetPage = root.Q<VisualElement>(targetPageName);
        if (targetPage != null)
        {
            targetPage.AddToClassList("ActivePage");
            topBarCreator.ShowTopBar(targetPageName);
            PageAnimator.FadeTo(lastPage, 1, 0, 500, () => {
                lastPage.style.display = DisplayStyle.None;
                lastPage.style.opacity = 0;
                targetPage.style.opacity = 1;
                targetPage.style.display = DisplayStyle.Flex;
            });
        }

    }

    void GenerateLevelRows(int characterCount)
    {
        scrollView.Clear();
        // scrollView.contentContainer.style.alignItems = new StyleEnum<Align>(Align.FlexStart);
        int levelNumber = 1;

        for (int i = 0; i < characterCount; i++)
        {
            // TODO: upgrade the generation with rule in level and difficulty
            VisualElement characterRow = CreateCharacterRow(ref levelNumber, i);
            // characterRow.SetSize()
            scrollView.Add(characterRow);
        }
    }

    VisualElement CreateCharacterRow(ref int levelNumber, int rowNum)
    {
        // Clone template
        VisualElement row = characterRowTemplate.Instantiate(); 
        row.style.height = new StyleLength(new Length(1560f, LengthUnit.Pixel)); 

        row.style.display = DisplayStyle.Flex; // make visible

        // Character icon
        var characterIcon = row.Q<VisualElement>("CharacterIcon");

        characterIcon.style.backgroundColor =
            new Color(Random.value, Random.value, Random.value);

        // Get container
        var curvedContainer = row.Q<VisualElement>("CurvedPathContainer");

        // Get buttons
        Button btn1 = row.Q<Button>("LevelButton1");
        Button btn2 = row.Q<Button>("LevelButton2");
        Button btn3 = row.Q<Button>("LevelButton3");
        Button btn4 = row.Q<Button>("LevelButton4");
        Button btn5 = row.Q<Button>("LevelButton5");

        Button[] buttons = { btn1, btn2, btn3, btn4, btn5 };

        foreach (var button in buttons)
        {
            int capturedLevel = levelNumber;

            button.text = capturedLevel.ToString();

            button.clicked += () =>
            {
                OnLevelClicked(capturedLevel);
            };

            levelNumber++;
        }

        return row;
    }
    void OnLevelClicked(int level, int difficulty = 0)
    {
        Debug.Log("Difficulty: " + difficulty + ", Clicked Level: " + level);

        // TODO:
        // Load MC Question Page here
        // Example:
        // LoadLevelPage(level);
    }
}