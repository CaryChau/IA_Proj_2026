using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
public class AchievementData
{
    public string Title;
    public string Description;
    public int Current;
    public int Max;
    public int Level;
    public string IconClass; // LegendaryIcon, ChallengerIcon etc
}

public class AchievementCreator : MonoBehaviour
{
    private ScrollView scrollView;

    void Start()
    {
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
                Title = "Legendary",
                Description = "Complete 75 legendary levels",
                Current = 53,
                Max = 75,
                Level = 6,
                IconClass = "LegendaryIcon"
            },
            new AchievementData
            {
                Title = "Challenger",
                Description = "Earn 5000 XP in timed challenges",
                Current = 4100,
                Max = 5000,
                Level = 5,
                IconClass = "ChallengerIcon"
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
        var card = new VisualElement();
        card.AddToClassList("AchievementCard");

        // ICON
        var icon = new VisualElement();
        icon.AddToClassList("AchievementBigIcon");
        icon.AddToClassList(data.IconClass);

        var levelLabel = new Label($"LEVEL {data.Level}");
        levelLabel.AddToClassList("LevelBadge");
        icon.Add(levelLabel);

        // CONTENT
        var content = new VisualElement();
        content.AddToClassList("AchievementContent");

        var title = new Label(data.Title);
        title.AddToClassList("AchievementName");

        var description = new Label(data.Description);
        description.AddToClassList("AchievementDescription");

        // PROGRESS ROW
        var progressRow = new VisualElement();
        progressRow.AddToClassList("ProgressRow");

        var progressBackground = new VisualElement();
        progressBackground.AddToClassList("ProgressBarBackground");

        var progressFill = new VisualElement();
        progressFill.AddToClassList("ProgressBarFill");

        float percent = (float)data.Current / data.Max * 100f;
        progressFill.style.width = Length.Percent(percent);

        progressBackground.Add(progressFill);

        var progressText = new Label($"{data.Current}/{data.Max}");
        progressText.AddToClassList("ProgressText");

        progressRow.Add(progressBackground);
        progressRow.Add(progressText);

        // Build hierarchy
        content.Add(title);
        content.Add(description);
        content.Add(progressRow);

        card.Add(icon);
        card.Add(content);

        return card;
    }
}