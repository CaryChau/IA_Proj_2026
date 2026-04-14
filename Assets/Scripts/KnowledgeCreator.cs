using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;
public delegate void NextActionHandler();
public delegate void OnCheckHandler(bool isRight);

[RequireComponent(typeof(UIDocument))]
public class KnowledgeCreator : SequenceDoc
{   
    [Header("UXML targets (auto-resolved from attached UIDocument)")]
    [SerializeField] private UIDocument uiDocument;

    private VisualElement _root;
    private VisualElement _pageRoot;
    private VisualElement PopupRoot;
    private VisualElement _progressFill;
    private bool[] finishedList;

    [Tooltip("Optional per-question UXML resource path (Resources/UIDocuments/...). Leave empty to build the page at runtime.")]
    [SerializeField] private List<string> questionUxmlResources = new List<string>();

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
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();
    }

    void OnDisable()
    {
        CurrentIndex = 0;
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
        finishedList = new bool[questions.Count];

        _progressFill = _root.Q<VisualElement>("ProgressFill");
        _pageRoot     = _root.Q<VisualElement>("PageRoot");
        PopupRoot     = _root.Q<VisualElement>("PopupRoot");
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
            // _currentEvaluator.onNext -= OnNextAction;
        }

        _currentEvaluator = evaluator;
        // _currentEvaluator.onNext += OnNextAction;
        _currentEvaluator.onCheck += OnCheckHdl;
    }
    private VisualElement popupCorrect;

    private void ShowCorrectPopup()
    {
        if (popupCorrect == null)
        {
            var vta = Resources.Load<VisualTreeAsset>(
                "UIDocuments/Popup/PopupCorrect");

            popupCorrect = vta.CloneTree();
            PopupRoot.Add(popupCorrect);
        }

        var root = popupCorrect.Q<VisualElement>("PopupRoot");


        // Button actions
        popupCorrect.Q<Button>("GotItBtn").clicked += HidePopup;

        // Start hidden
        root.RemoveFromClassList("PopupShown");
        root.AddToClassList("PopupHidden");

        // Trigger animation next frame
        root.schedule.Execute(() =>
        {
            root.RemoveFromClassList("PopupHidden");
            root.AddToClassList("PopupShown");
        });
    }

    private VisualElement popupInstance;

    private void ShowIncorrectPopup(string correctAnswer = "")
    {
        if (popupInstance == null)
        {
            var vta = Resources.Load<VisualTreeAsset>(
                "UIDocuments/Popup/PopupIncorrect");

            popupInstance = vta.CloneTree();
            PopupRoot.Add(popupInstance);
        }

        var root = popupInstance.Q<VisualElement>("PopupRoot");

        // Set correct answer text
        var answerText = popupInstance.Q<Label>("CorrectAnswerText");
        answerText.style.visibility = Visibility.Visible;

        if (correctAnswer == "")
        {
            answerText.style.visibility = Visibility.Hidden;
        }
        answerText.text = correctAnswer;

        // Button actions
        popupInstance.Q<Button>("GotItBtn").clicked += HidePopup;

        // Start hidden
        root.RemoveFromClassList("PopupShown");
        root.AddToClassList("PopupHidden");

        // Trigger animation next frame
        root.schedule.Execute(() =>
        {
            root.RemoveFromClassList("PopupHidden");
            root.AddToClassList("PopupShown");
        });
    }

    private void HidePopup()
    {
        if (popupInstance == null) return;

        var root = popupInstance.Q<VisualElement>("PopupRoot");
        root.RemoveFromClassList("PopupShown");
        root.AddToClassList("PopupHidden");
        OnNextAction();
    }

    private void OnCheckHdl(bool isRight)
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
            ShowIncorrectPopup();
        }
    }

    private void OnNextAction()
    {
        
        if (FinishedCount == Total)
        {
            // get out of question page and show the Congratulation page
            CurrentIndex = 0;

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

        VisualElement pageInstance;

        if (!string.IsNullOrEmpty(path))
        {
            // Aligns with the pattern used in your TopBarCreator: Resources/UIDocuments/<path>
            // If you pass full path including "UIDocuments/", keep using it consistently.
            var vta = Resources.Load<VisualTreeAsset>("UIDocuments/" + path);
            if (vta != null)
            {
                JToken question = FindQuestionByType(3);
                if (question == null)
                {
                    throw new Exception("Can not finid question: " + index);
                }
                pageInstance = vta.CloneTree();
                RegisterQuestion(InstanceQuestion((string)question["type"], pageInstance, question));
                _pageRoot.Add(pageInstance);
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