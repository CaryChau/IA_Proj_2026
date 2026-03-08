using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using System.Linq;
public class AchievementData
{
    public string title;
    public string condition;
    public int level;
    public float progress; // 0-1
    public Texture2D icon;
}

public class AchievementCreator : MonoBehaviour
{
    private ScrollView scrollView;
    public VisualTreeAsset achievementCardTemplate;
    private List<Texture2D> cardIconList;
    void Start()
    {
        cardIconList = Resources.LoadAll<Texture2D>("Textures/AchievementIcons").ToList<Texture2D>();
        var root = GetComponent<UIDocument>().rootVisualElement;
        scrollView = root.Q<ScrollView>("AchievementScrollView");

        LoadAchievements();
    }

    void LoadAchievements()
    {
        var achievements = new List<AchievementData>
        {
            new AchievementData
            {
                title = "Legendary",
                condition = "Complete 75 legendary levels",
                progress = 0.5f,
                level = 6,
                icon = cardIconList[0]
            },
            new AchievementData
            {
                title = "Challenger",
                condition = "Earn 5000 XP in timed challenges",
                progress = 0.5f,
                level = 5,
                icon = cardIconList[0]
            }
        };

        foreach (var data in achievements)
        {
            var card = CreateAchievementCard(data);
            scrollView.Add(card);
        }
    }

    VisualElement CreateAchievementCard(AchievementData data)
    {
        VisualElement card = achievementCardTemplate.Instantiate();

        card.AddToClassList("AchievementCard");
        card.style.display = DisplayStyle.Flex;

        var left = card.Q<VisualElement>("LeftLevel");
        var right = card.Q<VisualElement>("RightDesc");

        var icon = card.Q<VisualElement>("IconBg");
        var levelText = card.Q<Label>("LevelText");

        var title = card.Q<Label>("Title");
        var condition = card.Q<Label>("ConditionTxt");

        var progressBar = card.Q<VisualElement>("BarImg");

        /* Populate content */

        levelText.text = "Lv " + data.level;

        title.text = data.title;

        condition.text = data.condition;

        if (data.icon != null)
        {
            icon.style.backgroundImage = new StyleBackground(data.icon);
        }

        progressBar.style.width = Length.Percent(data.progress * 100f);

        return card;
    }
}