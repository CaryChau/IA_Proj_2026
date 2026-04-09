using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TopBarCreator : MonoBehaviour
{
    private VisualElement root;
    private VisualElement topBarContent;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        topBarContent = root.Q<VisualElement>("TopBarContent");
        ShowTopBar("LevelPage");
    }

    public void ShowTopBar(string pageName)
    {
        switch (pageName)
        {
            case "LevelPage":
            {
                ReplaceTopBar("TopBars/LevelTopBar");
                var btn = topBarContent.Q<Button>("CourseBtn");
                var courseCreator = GetComponent<CourseTabCreator>();
                btn.clicked += courseCreator.ToggleCourseTab;
                break;
            }
            case "AchievementPage":
            {
                ReplaceTopBar("TopBars/AchievementTopBar");
                break;
            }
            case "ProfilePage":
            {
                ReplaceTopBar("TopBars/ProfileTopBar");
                break;
            }
        }
    }

    void ReplaceTopBar(string resourcePath)
    {
        topBarContent.Clear();
        string fullPath = "UIDocuments/" + resourcePath;
        VisualTreeAsset asset =
            Resources.Load<VisualTreeAsset>(fullPath);

        if (asset != null)
        {
            VisualElement content = asset.CloneTree();
            topBarContent.Add(content);
        }
    }
}