using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
public delegate void NextActionHandler();
public delegate void OnCheckHandler(bool isRight);

[RequireComponent(typeof(UIDocument))]
public class KnowledgeCreator : MonoBehaviour
{

    /// <summary>
    /// Event payload when a new question page is ready inside PageRoot.
    /// </summary>
    public sealed class QuestionPageCreatedEventArgs : EventArgs
    {
        public int Index { get; }
        public string Question { get; }
        public string Answer { get; }
        public VisualElement PageInstance { get; }

        public QuestionPageCreatedEventArgs(int index, string question, string answer, VisualElement pageInstance)
        {
            Index = index;
            Question = question ?? string.Empty;
            Answer = answer ?? string.Empty;
            PageInstance = pageInstance;
        }
    }
    [Header("UXML targets (auto-resolved from attached UIDocument)")]
    [SerializeField] private UIDocument uiDocument;

    private VisualElement _root;
    private VisualElement _pageRoot;
    private VisualElement _progressFill;

    [Header("Data (populate in Inspector or via SetData)")]
    [Tooltip("List of questions (string). Can be plain text or identifiers for audio/pronunciation, etc.")]
    [SerializeField] private List<string> questions = new List<string>();
    private bool[] finishedList;

    [Tooltip("List of answers (string). Keep it 1:1 with Questions.")]
    [SerializeField] private List<string> answers = new List<string>();

    [Tooltip("Optional per-question UXML resource path (Resources/UIDocuments/...). Leave empty to build the page at runtime.")]
    [SerializeField] private List<string> questionUxmlResources = new List<string>();

    [Header("Flow")]
    [Tooltip("Start from this index on enable.")]
    [SerializeField] private int startIndex = 0;

    /// <summary>Zero-based question index currently shown.</summary>
    public int CurrentIndex { get; private set; } = 0;

    /// <summary>Total number of questions considered (min of questions/answers).</summary>
    public int Total => Mathf.Min(questions?.Count ?? 0, answers?.Count ?? 0);

    /// <summary>How many questions have been marked as finished (used for progress).</summary>
    public int FinishedCount { get; private set; } = 0;

    /// <summary>
    /// Raised every time a question page is instantiated and added to PageRoot.
    /// Handlers should subscribe to this to set up their UI & logic, and then
    /// later call RegisterEvaluator(...) or otherwise let the creator know how to listen
    /// for AnswerEvaluated events.
    /// </summary>
    public event EventHandler<QuestionPageCreatedEventArgs> QuestionPageCreated;

    /// <summary>Raised when all questions are completed.</summary>
    // public event EventHandler<QuizCompletedEventArgs> QuizCompleted;

    // We keep a reference to the current evaluator so we can unsubscribe when moving on.
    private QuestionBase _currentEvaluator;

    private void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        finishedList = new bool[questions.Count];
    }

    private void OnEnable()
    {
        _root = uiDocument?.rootVisualElement;
        if (_root == null)
        {
            Debug.LogError("KnowledgeCreator: UIDocument/rootVisualElement not found.");
            return;
        }

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

    private void OnDisable()
    {
        // Clean up event subscription to the current evaluator if any
        if (_currentEvaluator != null)
            _currentEvaluator.onNext -= OnNextAction;
        _currentEvaluator = null;
    }

    // ---------- Public API ----------

    /// <summary>
    /// Replace the internal data at runtime and restart from index 0 (unless otherwise specified).
    /// </summary>
    public void SetData(IList<string> newQuestions, IList<string> newAnswers, IList<string> uxmlPaths = null, int startAtIndex = 0)
    {
        questions = newQuestions != null ? new List<string>(newQuestions) : new List<string>();
        answers   = newAnswers   != null ? new List<string>(newAnswers)   : new List<string>();
        questionUxmlResources = uxmlPaths != null ? new List<string>(uxmlPaths) : new List<string>();

        CurrentIndex  = Mathf.Clamp(startAtIndex, 0, Mathf.Max(0, Total - 1));
        FinishedCount = 0;
        UpdateProgress(0f);
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
            // get out of question page
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

    /// <summary>
    /// Allows external logic to set progress explicitly in [0..1]. Usually not needed,
    /// since progress advances automatically when answers are evaluated.
    /// </summary>
    public void SetProgress(float t)
    {
        UpdateProgress(Mathf.Clamp01(t));
    }

    // ---------- Core flow ----------

    private void LoadQuestion(int index)
    {
        _pageRoot.Clear();

        string q = SafeGet(questions, index);
        string a = SafeGet(answers,   index);
        string path = SafeGet(questionUxmlResources, index); // may be empty

        VisualElement pageInstance;

        if (!string.IsNullOrEmpty(path))
        {
            // Aligns with the pattern used in your TopBarCreator: Resources/UIDocuments/<path>
            // If you pass full path including "UIDocuments/", keep using it consistently.
            var vta = Resources.Load<VisualTreeAsset>(path);
            if (vta != null)
            {
                
                pageInstance = vta.CloneTree();
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

        // Notify handlers: a new page instance is ready with the question+answer.
        QuestionPageCreated?.Invoke(
            this,
            new QuestionPageCreatedEventArgs(index, q, a, pageInstance)
        );
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