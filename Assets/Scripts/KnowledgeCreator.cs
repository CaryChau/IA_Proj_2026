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
    private VisualElement _progressFill;
    private bool[] finishedList;

    [Tooltip("Optional per-question UXML resource path (Resources/UIDocuments/...). Leave empty to build the page at runtime.")]
    [SerializeField] private List<string> questionUxmlResources = new List<string>();

    [Header("Flow")]
    [Tooltip("Start from this index on enable.")]
    [SerializeField] private int startIndex = 0;
    JObject jsonRoot;
    JArray questions;


    int curLev;
    private void SetupKnowledge(string jsonString)
    {
        if (jsonString == null)
        {
            Debug.LogError("json data request failed");
            return;
        }

        try
        {
            jsonRoot = JObject.Parse(jsonString);
            curLev = (int)jsonRoot["level"];
            foreach (var level in (JArray)jsonRoot["levels"])
            {
                if ((int)level["level"] == curLev)
                {
                    questions = (JArray)level["questions"];
                }
            }
            Total = questions.Count;
        }
        catch (System.Exception)
        {
            
            throw;
        }
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

        // UIManager.GetInstance().enableCreator += EnableCreator;
    }

    private void EnableCreator()
    {
        this.enabled = true;
    }

    void OnDisable()
    {
        CurrentIndex = 0;
        // for (int i = 0; i < finishedList.Length; i++)
        // {
        //     finishedList[i] = false;
        // }
        // UIManager.GetInstance().enableCreator -= EnableCreator;
    }

    private void ReadJsonDataFromFile()
    {
        // load json data from Resources/TestData/.json
    }


    private void OnEnable()
    {
        return;
        _root = uiDocument?.rootVisualElement;
        if (_root == null)
        {
            Debug.LogError("KnowledgeCreator: UIDocument/rootVisualElement not found.");
            return;
        }

        SetupKnowledge(UIManager.GetInstance().KnowledgeData);
        finishedList = new bool[questions.Count];

        _progressFill = _root.Q<VisualElement>("ProgressFill");
        _pageRoot     = _root.Q<VisualElement>("PageRoot");

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
            _currentEvaluator.onNext -= OnNextAction;
        }

        _currentEvaluator = evaluator;
        _currentEvaluator.onNext += OnNextAction;
        _currentEvaluator.onCheck += OnCheckHdl;
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
        }
    }

    private void OnNextAction()
    {
        
        if (FinishedCount == Total)
        {
            // get out of question page and show the Congratulation page
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

    private QuestionBase InstanceQuestion(string type, VisualElement page)
    {
        switch (type)
        {
            case "drag_match":
                return new DragMatchQuestion(page);
            case "speaking":
                return new SpeakingQuestion(page);
            case "true_false":
                return new TrueFalseQuestion(page);
            case "select_one":
                return new SelectOneQuestion(page);
            case "listening":
                return new ListeningQuestion(page);
            case "spelling":
                return new SpellingQuestion(page);
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
            var vta = Resources.Load<VisualTreeAsset>(path);
            if (vta != null)
            {
                JToken question = FindQuestionByType(index);
                if (question == null)
                {
                    throw new Exception("Can not finid question: " + index);
                }
                pageInstance = vta.CloneTree();
                RegisterQuestion(InstanceQuestion((string)question["type"], pageInstance));
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