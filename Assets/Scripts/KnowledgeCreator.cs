#define TEST_UXML
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
public delegate void NextActionHandler();
public delegate void OnCheckHandler(bool isRight, string correctAnswer = "");
[RequireComponent(typeof(UIDocument))]
public class KnowledgeCreator : SequenceDoc
{   
    [Header("UXML targets (auto-resolved from attached UIDocument)")]
    [SerializeField] private UIDocument uiDocument;

    private VisualElement _root;
    private VisualElement _pageRoot;
    private VisualElement PopupRoot;
    private VisualElement PopupBackRoot;
    private VisualElement PopupFrontRoot;
    private Button closeButton;
    private readonly string GeneralTip = "Give it another try.";
    private VisualElement _progressFill;
    private bool[] finishedList;

    [Tooltip("Optional per-question UXML resource path (Resources/UIDocuments/...). Leave empty to build the page at runtime.")]
    [SerializeField] private List<string> questionUxmlResources = new List<string>();
#if TEST_UXML
    #region Test uxml
    public string testUxml = null;
    public Dictionary<string, int> uxmlToType;
    private void InitTestData()
    {
        uxmlToType = new Dictionary<string, int>();
        uxmlToType.Add("ListeningPage", 3);
        uxmlToType.Add("MatchingPage", 0);
        uxmlToType.Add("TrueFalsePage", 1);
        uxmlToType.Add("SelectOnePage", 2);
        uxmlToType.Add("SpellingPage", 4);
        uxmlToType.Add("SpeakingPage", 5);
    }

    #endregion
#endif
    [Header("Flow")]
    [Tooltip("Start from this index on enable.")]
    [SerializeField] private int startIndex = 0;
    JArray questions = null;
    
    public override void OnDocSwitch(OnSwitchArgs args)
    {
        OnJumpToQuestionArgs argsNew = (OnJumpToQuestionArgs)args;
        base.OnDocSwitch(args);
        topic = argsNew.topic;
        difficulty = argsNew.difficulty;
        level = argsNew.level;
        ReadJsonDataFromFile();
        StartQuestion();
    }

    
    private void ReadJsonDataFromFile()
    {
        // 1) Build file name: t1_d1.json
        string fileName = $"t{topic}_d{difficulty}";

        string jsonText;
        try
        {
            var textAsset = Resources.Load<TextAsset>("TestData/" + fileName);
            jsonText = textAsset.text;
        }
        catch (Exception ex)
        {
            return;
        }

        // 3) Parse entire document as JObject (keeps all unknown fields)
        JObject root;
        try
        {
            root = JObject.Parse(jsonText);
        }
        catch (Exception ex)
        {
            return;
        }

        // 4) Validate and find the requested level
        JArray levelsArray = root["levels"] as JArray;
        if (levelsArray == null)
        {
            return;
        }

        JObject matchedLevel = null;
        foreach (var item in levelsArray)
        {
            if (item is JObject lvlObj)
            {
                int lvl = lvlObj.Value<int?>("level") ?? -1;
                if (lvl == level)
                {
                    matchedLevel = lvlObj;
                    break;
                }
            }
        }

        if (matchedLevel == null)
        {
            return;
        }
        questions = matchedLevel["questions"] as JArray;
        Total = questions.Count;
    }

    /// <summary>Zero-based question index currently shown.</summary>
    public int CurrentIndex { get; private set; } = 0;

    private int Total;

    /// <summary>How many questions have been marked as finished (used for progress).</summary>
    public int FinishedCount { get; private set; } = 0;

    // We keep a reference to the current evaluator so we can unsubscribe when moving on.
    private QuestionBase _currentEvaluator;

    public KnowledgeCreator(DocType initId = DocType.Question) : base(initId)
    {
    }

    private void Awake()
    {
        #if TEST_UXML
            InitTestData();
        #endif
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();
    }

    private VisualElement halfwayPopup;
    private VisualElement CloneTree(VisualTreeAsset asset, string templateRoot)
    {
        VisualElement template = asset.CloneTree();
        template.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        template.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        template.pickingMode = PickingMode.Ignore;
        // var root = template.Q<VisualElement>(templateRoot);
        return template;
    }
    private void ShowHalfwayQuitPopup()
    {
        if (halfwayPopup == null)
        {
            var vta = Resources.Load<VisualTreeAsset>(
                "UIDocuments/Popup/PopupHalfwayQuit");

            halfwayPopup = CloneTree(vta, "HalfwayQuitPopup");
            PopupFrontRoot.Add(halfwayPopup);
            halfwayPopup = halfwayPopup.Q<VisualElement>("HalfwayQuitPopup");
            halfwayPopup.Q<Button>("KeepLearningBtn").clicked += HideHalfwayQuitPopup;
            halfwayPopup.Q<Button>("QuitBtn").clicked += () =>
            {
                SetTarget(DocType.Navigation, null);
                OnKnowledgeQuit();
            };
        }

        halfwayPopup.RemoveFromClassList("HalfwayPopupHidden");
        halfwayPopup.AddToClassList("HalfwayPopupShown");
    }

    private void OnKnowledgeQuit()
    {
        closeButton.clicked -= ShowHalfwayQuitPopup;
        HideHalfwayQuitPopup();
        HidePopup(popupCorrect);
        HidePopup(popupInstance);
    }

    private void HideHalfwayQuitPopup()
    {
        if (halfwayPopup == null) return;

        halfwayPopup.RemoveFromClassList("HalfwayPopupShown");
        halfwayPopup.AddToClassList("HalfwayPopupHidden");
    }

    private int topic, difficulty, level;
    
    private void StartQuestion()
    {
        if (questions == null || questions.Count == 0)
        {
            return;
        }
        _root = uiDocument?.rootVisualElement;
        if (_root == null)
        {
            Debug.LogError("KnowledgeCreator: UIDocument/rootVisualElement not found.");
            return;
        }
        if (finishedList == null)
        {
            finishedList = new bool[questions.Count];
        }

        _progressFill = _root.Q<VisualElement>("ProgressFill");
        _pageRoot     = _root.Q<VisualElement>("PageRoot");
        PopupRoot     = _root.Q<VisualElement>("PopupRoot");
        PopupBackRoot = PopupRoot.Q<VisualElement>("BackRoot");
        PopupFrontRoot = PopupRoot.Q<VisualElement>("FrontRoot");
        closeButton   = _root.Q<Button>("CloseButton");
        closeButton.clicked += ShowHalfwayQuitPopup;
        if (_progressFill == null || _pageRoot == null)
        {
            Debug.LogError("KnowledgeCreator: Required elements 'ProgressFill' or 'PageRoot' not found in UXML.");
            return;
        }

        // Initialize state & UI
        CurrentIndex  = Mathf.Clamp(startIndex, 0, Mathf.Max(0, Total - 1));
        FinishedCount = 0;
        UpdateProgress(0f);

        // Show first question
        LoadQuestion(CurrentIndex);
    }


    public void RegisterQuestion(QuestionBase evaluator)
    {
        if (evaluator == null) return;

        // Unsubscribe previous
        if (_currentEvaluator != null)
        {
            _currentEvaluator.onCheck -= OnCheckHdl;
            // _currentEvaluator.LeavePage((e) =>
            // {
            //     _pageRoot.Remove(e);
            // });
            _currentEvaluator = null;
        }

        _currentEvaluator = evaluator;
        // _currentEvaluator.EnterPage();
        _currentEvaluator.onCheck += OnCheckHdl;
    }

    private void OnFinishQuestion()
    {
        if (_currentEvaluator != null)
        {
            _currentEvaluator.onCheck -= OnCheckHdl;
            _currentEvaluator.LeavePage((e) =>
            {
                _pageRoot.Remove(e);
            });
            _currentEvaluator = null;
        }
    }
    private VisualElement popupCorrect;

    private void ShowCorrectPopup()
    {
        if (popupCorrect == null)
        {
            var vta = Resources.Load<VisualTreeAsset>(
                "UIDocuments/Popup/PopupCorrect");

            popupCorrect = CloneTree(vta, "PopupRoot");
            PopupBackRoot.Add(popupCorrect);
            popupCorrect = popupCorrect.Q<VisualElement>("PopupRoot");
            // Button actions
            popupCorrect.Q<Button>("GotItBtn").clicked += () => {
                HidePopup(popupCorrect);
                OnNextAction();
            };
        }

        // Start hidden
        popupCorrect.AddToClassList("PopupShown");
        popupCorrect.RemoveFromClassList("PopupHidden");
    }

    private VisualElement popupInstance;

    private void ShowIncorrectPopup(string correctAnswer)
    {
        if (popupInstance == null)
        {
            var vta = Resources.Load<VisualTreeAsset>(
                "UIDocuments/Popup/PopupIncorrect");

            popupInstance = CloneTree(vta, "PopupRoot");
            PopupBackRoot.Add(popupInstance);
            popupInstance = popupInstance.Q<VisualElement>("PopupRoot");
            // Button actions
            popupInstance.Q<Button>("GotItBtn").clicked += () => {
                HidePopup(popupInstance);
                OnNextAction();
            };
        }

        // var root = popupInstance.Q<VisualElement>("PopupRoot");

        // Set correct answer text
        var answerText = popupInstance.Q<Label>("CorrectAnswerText");
        var correctAnswerLabel = popupInstance.Q<Label>("CorrectAnswerLabel");
        correctAnswerLabel.style.display = DisplayStyle.Flex;
        if (correctAnswer == "")
        {
            correctAnswerLabel.style.display = DisplayStyle.None;
            answerText.text = GeneralTip;
        }
        else
        {
            answerText.text = correctAnswer;
        }


        // Start hidden
        popupInstance.AddToClassList("PopupShown");
        popupInstance.RemoveFromClassList("PopupHidden");
    }

    private void HidePopup(VisualElement popupInstance)
    {
        if (popupInstance == null) return;

        popupInstance.RemoveFromClassList("PopupShown");
        popupInstance.AddToClassList("PopupHidden");
    }

    private void OnCheckHdl(bool isRight, string tipAnswer)
    {
        if (isRight)
        {
            int count = 0;
            for (int i = 0; i < finishedList.Length; i++)
            {
                count = finishedList[i] ? ++count : count;
            }
            FinishedCount = count;
            UpdateProgress(Total > 0 ? (float)FinishedCount / Total : 0f);
            ShowCorrectPopup();
        }
        else
        {
            ShowIncorrectPopup(tipAnswer);
        }
    }

    private void OnNextAction()
    {
        
        if (FinishedCount == Total)
        {
            // get out of question page and show the Congratulation page
            OnFinishQuestion();
            // fade in the course completed.
        }
        else
        {
            int nextIdx = CurrentIndex + 1;
            if (nextIdx > Total - 1)
            {
                for (int i = 0; i < finishedList.Length; i++)
                {
                    nextIdx = !finishedList[i] ? i : nextIdx;
                }
            }
            CurrentIndex = nextIdx;
            LoadQuestion(CurrentIndex);
        }
    }

    private QuestionBase InstanceQuestion(string type, VisualElement page, JToken data)
    {
        switch (type)
        {
            case "drag_match":
                return new DragMatchQuestion(page, data);
            case "speaking":
                return new SpeakingQuestion(page, data);
            case "true_false":
                return new TrueFalseQuestion(page, data);
            case "select_one":
                return new SelectOneQuestion(page, data);
            case "listening":
                return new ListeningQuestion(page, data);
            case "spelling":
                return new SpellingQuestion(page, data);
        }
        return null;
    }

    // ---------- Core flow ----------

    JToken FindQuestionByType(int idx)
    {
        int i = 0;
        foreach (JObject question in questions)
        {
            if (i == idx)
                return question;
            i++;
        }
        return null;
    }

    private void LoadQuestion(int index)
    {
        _pageRoot.Clear();
        string path = SafeGet(questionUxmlResources, index); // may be empty
#if TEST_UXML
        path = testUxml != null ? testUxml : path;
#endif
        VisualElement pageInstance;

        if (!string.IsNullOrEmpty(path))
        {
            // Aligns with the pattern used in your TopBarCreator: Resources/UIDocuments/<path>
            // If you pass full path including "UIDocuments/", keep using it consistently.
            var vta = Resources.Load<VisualTreeAsset>("UIDocuments/Questions/" + path);
            if (vta != null)
            {
                JToken question = FindQuestionByType(index);
#if TEST_UXML
                uxmlToType.TryGetValue(testUxml, out int test_idx);
                question = FindQuestionByType(test_idx);
#endif
                if (question == null)
                {
                    throw new Exception("Can not finid question: " + index);
                }
                pageInstance = CloneTree(vta, "QuestionRoot");
                _pageRoot.Add(pageInstance);
                RegisterQuestion(InstanceQuestion((string)question["type"], pageInstance, question));
            }
            else
            {
                Debug.LogWarning($"KnowledgeCreator: Could not find VisualTreeAsset at Resources path '{path}'. Using an empty container.");
                pageInstance = new VisualElement();
                _pageRoot.Add(pageInstance);
            }
        }
        else
        {
            // No template provided; leave a blank container for the handler to populate.
            pageInstance = new VisualElement();
            _pageRoot.Add(pageInstance);
            Debug.LogError("Please add uxml name in the KnowledgeCreator");
        }
    }

    private void UpdateProgress(float t01)
    {
        if (_progressFill == null) return;
        _progressFill.style.width = new Length(t01 * 100f, LengthUnit.Percent);
    }

    // ---------- Helpers ----------

    private static string SafeGet(List<string> list, int index)
    {
        if (list == null || index < 0 || index >= list.Count) return string.Empty;
        return list[index];
    }
}