using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
public class CourseTabCreator : MonoBehaviour
{

    public class CourseData
    {
        public string Id;
        public string DisplayName;
        public string IconClass;   // USS class for icon (e.g. "DrugInteractionIcon")
        public bool IsAddButton;   // for "+" tile if needed
    }

    [Header("Course Data")]
    [SerializeField]
    private List<CourseData> courses = new List<CourseData>
    {
        new CourseData { Id = "drug_interaction", DisplayName = "Drug Interaction", IconClass = "DrugInteractionIcon" },
        new CourseData { Id = "drug_class",        DisplayName = "Drug Class",        IconClass = "DrugClassIcon" },
        new CourseData { Id = "generic_brand",     DisplayName = "Generic Brand",     IconClass = "GenericBrandIcon" },
        new CourseData { Id = "dosage_form",       DisplayName = "Dosage Form",       IconClass = "DosageFormIcon" },
        new CourseData { Id = "abbreviation",      DisplayName = "Abbreviation",      IconClass = "AbbreviationIcon" },
        new CourseData { Id = "more",               DisplayName = "More",               IconClass = "PlusIcon", IsAddButton = true }
    };

    public VisualTreeAsset courseTab;
    private VisualElement root;
    private VisualElement tabRoot;
    private ScrollView courseScroll;
    private Button selectedButton;
    private VisualElement overlayBg;
    private VisualElement courseTabRoot;
    private bool isCourseTabShown;
    void Awake()
    {
        
    }

    private void Start() {
        root = GetComponent<UIDocument>().rootVisualElement;
        tabRoot = root.Q<VisualElement>("TabRoot");
        var template = courseTab.Instantiate();
        template.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        template.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        template.pickingMode = PickingMode.Ignore; 
        tabRoot.Add(template);
        courseScroll = template.Q<ScrollView>("CourseScroll");
        overlayBg = template.Q<VisualElement>("OverlayBackground");
        courseTabRoot = template.Q<VisualElement>("CourseTabRoot");
        BuildCourseList();
    }

    // void OnEnable()
    // {
    //     BuildCourseList();
    // }

    public void ToggleCourseTab()
    {
        if (!isCourseTabShown)
            ShowCourseTab();
        else
            HideCourseTab();
    }

    private void ShowCourseTab()
    {
        if (overlayBg == null || courseTabRoot == null)
        {
            Debug.LogWarning("CourseTabRoot / overlayBg not found. Did you add CourseTab.uxml into the document?");
            return;
        }

        isCourseTabShown = true;

        // Ensure a known start state (hidden) BEFORE triggering transition
        overlayBg.RemoveFromClassList("OverlayShown");
        overlayBg.AddToClassList("OverlayHidden");

        courseTabRoot.RemoveFromClassList("CourseTabShown");
        courseTabRoot.AddToClassList("CourseTabHidden");

        // IMPORTANT: schedule to next repaint/frame so transition has a "previous state"
        // Otherwise it may snap immediately (no animation). 
        courseTabRoot.schedule.Execute(() =>
        {
            overlayBg.RemoveFromClassList("OverlayHidden");
            overlayBg.AddToClassList("OverlayShown");
            overlayBg.pickingMode = PickingMode.Position;

            courseTabRoot.RemoveFromClassList("CourseTabHidden");
            courseTabRoot.AddToClassList("CourseTabShown");
        });
    }

    private void HideCourseTab()
    {
        if (overlayBg == null || courseTabRoot == null) return;

        isCourseTabShown = false;

        // Hiding can be immediate toggle (transition will run because state is already established)
        overlayBg.RemoveFromClassList("OverlayShown");
        overlayBg.AddToClassList("OverlayHidden");
        overlayBg.pickingMode = PickingMode.Ignore;

        courseTabRoot.RemoveFromClassList("CourseTabShown");
        courseTabRoot.AddToClassList("CourseTabHidden");
    }

    private void BuildCourseList()
    {
        courseScroll.Clear();
        if (overlayBg != null)
        {
            overlayBg.RegisterCallback<ClickEvent>(_ => {
                if (isCourseTabShown) HideCourseTab();
            });
        }
        foreach (var course in courses)
        {
            Button tile = new Button();
            tile.AddToClassList("CourseTile");
            tile.style.display = DisplayStyle.Flex;

            // Icon
            var icon = new VisualElement();
            icon.AddToClassList("CourseIcon");
            icon.AddToClassList(course.IconClass);

            // Label
            var label = new Label(course.DisplayName);
            label.AddToClassList("CourseName");

            // Hierarchy
            tile.Add(icon);
            tile.Add(label);

            if (course.IsAddButton)
            {
                tile.AddToClassList("CourseTile--Add");
                label.AddToClassList("CourseName--Muted");
            }

            tile.clicked += () => OnCourseClicked(course, tile);

            courseScroll.Add(tile);

            // Default selection
            if (selectedButton == null && !course.IsAddButton)
            {
                SelectTile(tile);
            }
        }
    }

    private void OnCourseClicked(CourseData course, Button tile)
    {
        if (course.IsAddButton)
        {
            Debug.Log("More courses clicked");
            return;
        }

        SelectTile(tile);

        Debug.Log($"Selected course: {course.Id}");
        // 🔔 Later: fire event → switch course / reload level map
    }

    private void SelectTile(Button tile)
    {
        if (selectedButton != null)
            selectedButton.RemoveFromClassList("isSelected");

        selectedButton = tile;
        selectedButton.AddToClassList("isSelected");
    }
}
