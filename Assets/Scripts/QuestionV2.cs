using UnityEngine;
// using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UIElements;
using TMPro;

public class Question2 : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string type;
        public string content;
        public string[] options;
        public int correctAnswer;
        public string questionText;
    }

    [Header("UI References")]
    public Label questionText;
    public Image questionImage;
    public Button[] optionButtons;
    public Label[] optionTexts;
    public Label scoreText;
    public Label levelText;
    // public GameObject gameOverPanel;
    public Label finalScoreText;
    public AudioSource audioSource;

    [Header("Timer References")]
    public Image timerFill;

    [Header("Game Settings")]
    public string name = "Questions.txt";
    public int amount = 8;
    public float time = 10f;

    private List<Question> allQuestions = new List<Question>();
    private List<Question> currentRoundQuestions = new List<Question>();
    private int currentQuestionIndex = 0;
    private int score = 0;
    private int currentLevel = 1;
    private float timer;
    private bool isAnswered = false;

    private string[] allQuestionTypes = new string[]
    {
        "picture",
        "word",
        "prononuce",
        "abbreviation"
    };

    private void InitOptionBtns()
    {
        // find four buttons under questionpage.uxml, and store in array optionButtons
    }
    private VisualElement root;
    private VisualElement levelPage;
    private VisualElement topbar;

    void InitUIElements()
    {
        // Assuming 'root' is your root VisualElement
        // and 'levelPage' and 'contentRoot' are already assigned
        root = GetComponent<UIDocument>().rootVisualElement;
        levelPage = root.Q<VisualElement>("OptionButtons");
        topbar = root.Q<VisualElement>("topBar");
        // Initialize question text and image
        questionText = topbar.Q<Label>("QuestionText");
        questionImage = topbar.Q<Image>("QuestionImage");

        // Initialize score and level labels
        scoreText = topbar.Q<Label>("ScoreText");
        levelText = levelPage.Q<Label>("LevelText");
        // finalScoreText = levelPage.Q<Label>("FinalScoreText");

        // Initialize game over panel
        // gameOverPanel = levelPage.Q<VisualElement>("GameOverPanel");

        // Initialize timer fill
        timerFill = levelPage.Q<Image>("TimerFill");
        optionButtons = new Button[4];
        // Initialize option buttons and option texts
        for (int i = 0; i < optionButtons.Length; i++)
        {
            // Assuming your buttons are named "OptionButton1", "OptionButton2", etc.
            optionButtons[i] = levelPage.Q<Button>($"OptionButton{i + 1}");
            optionTexts[i] = optionButtons[i].Q<Label>("unity-text-element");
            int index = i; // capture for closure
            optionButtons[i].clicked += () => OnOptionClicked(index);
        }
    }

    void Start()
    {
        InitUIElements();
        LoadQuestions();
        StartNewRound();
    }

    void Update()
    {
        if (!isAnswered)
        {
            timer -= Time.deltaTime;
            UpdateTimerDisplay();

            if (timer <= 0)
            {
                timer = 0;
                TimeOut();
            }
        }
    }


    void UpdateTimerDisplay()
    {
        if (timerFill != null)
        {

            float fillAmount = Mathf.Clamp01(timer / time);

            // Set the width of the timerFill based on fillAmount
            // Assuming timerFill is a VisualElement with a fixed parent width
            // Example: scale the width by fillAmount

            // If timerFill is an Image with a fixed size
            var parentWidth = timerFill.parent.layout.width; // Get parent width
            float newWidth = parentWidth * fillAmount;

            // Apply new width
            timerFill.style.width = new StyleLength(new Length(newWidth, LengthUnit.Pixel));

            // Change color based on progress
            if (fillAmount > 0.5f)
            {
                timerFill.style.backgroundColor = Color.green;
            }
            else if (fillAmount > 0.2f)
            {
                timerFill.style.backgroundColor = Color.yellow;
            }
            else
            {
                timerFill.style.backgroundColor = Color.red;
            }
        }
    }

    void LoadQuestions()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, name);

        if (Application.platform == RuntimePlatform.Android)
        {
            WWW reader = new WWW(filePath);
            while (!reader.isDone) { }
            string fileContent = reader.text;
            ParseQuestions(fileContent);
        }
        else
        {
            if (File.Exists(filePath))
            {
                string fileContent = File.ReadAllText(filePath);
                ParseQuestions(fileContent);
            }
            else
            {
                Debug.Log("not found: " + filePath);
            }
        }
        CheckQuestionTypes();
    }

    void CheckQuestionTypes()
    {
        foreach (string type in allQuestionTypes)
        {
            int count = allQuestions.FindAll(q => q.type == type).Count;
        }
    }

    void ParseQuestions(string content)
    {
        string[] lines = content.Split('\n');
        foreach (string line in lines)
        {
            if (string.IsNullOrEmpty(line.Trim()) || line.StartsWith("#"))
                continue;

            string[] parts = line.Split('|');
            if (parts.Length >= 7)
            {
                Question q = new Question();
                q.type = parts[0].Trim();
                q.content = parts[1].Trim();

                q.options = new string[4];
                for (int i = 0; i < 4; i++)
                {
                    q.options[i] = parts[i + 2].Trim();
                }

                q.correctAnswer = int.Parse(parts[6].Trim());
                q.questionText = parts[7].Trim();

                allQuestions.Add(q);
            }
        }
    }

    void StartNewRound()
    {
        currentRoundQuestions.Clear();
        currentQuestionIndex = 0;

        GenerateMixedQuestions();

        ShowQuestion();
    }
    void GenerateMixedQuestions()
    {
        for (int i = 0; i < allQuestionTypes.Length; i++)
        {
            AddRandomQuestionOfType(allQuestionTypes[i]);
        }

        for (int i = 0; i < 4; i++)
        {
            string randomType = allQuestionTypes[Random.Range(0, allQuestionTypes.Length)];
            AddRandomQuestionOfType(randomType);
        }
        ShuffleQuestions();
    }

    void AddRandomQuestionOfType(string type)
    {
        List<Question> typeQuestions = allQuestions.FindAll(q => q.type == type);

        if (typeQuestions.Count > 0)
        {
            int randomIndex = Random.Range(0, typeQuestions.Count);
            currentRoundQuestions.Add(typeQuestions[randomIndex]);
        }
        else
        {
            if (allQuestions.Count > 0)
            {
                int randomIndex = Random.Range(0, allQuestions.Count);
                currentRoundQuestions.Add(allQuestions[randomIndex]);
            }
        }
    }

    void ShuffleQuestions()
    {
        for (int i = 0; i < currentRoundQuestions.Count; i++)
        {
            Question temp = currentRoundQuestions[i];
            int randomIndex = Random.Range(i, currentRoundQuestions.Count);
            currentRoundQuestions[i] = currentRoundQuestions[randomIndex];
            currentRoundQuestions[randomIndex] = temp;
        }
    }

    void ShowQuestion()
    {
        if (currentQuestionIndex >= currentRoundQuestions.Count)
        {
            EndRound();
            return;
        }

        isAnswered = false;
        timer = time;

        if (timerFill != null)
        {
            timerFill.style.width = 1f;
            timerFill.style.backgroundColor = Color.green;
        }

        if (levelText != null)
        {
            levelText.text = "Level: " + (currentQuestionIndex + 1);
        }

        Question q = currentRoundQuestions[currentQuestionIndex];

        for (int i = 0; i < optionButtons.Length; i++)
        {
            var btn = optionButtons[i];
            // btn.interactable = true;
            // btn.GetComponent<Image>().color = Color.white;
            optionButtons[i].clicked += (() =>
            {
                OnOptionClicked(i);
            });

        }


        switch (q.type)
        {
            case "picture":
                questionImage.style.display = DisplayStyle.Flex;
                LoadImage(q.content);
                questionText.text = q.questionText;
                break;

            case "word":
                questionImage.style.display = DisplayStyle.None;
                questionText.text = q.questionText + "\n\n" + q.content;
                break;

            case "prononuce":
                questionImage.style.display = DisplayStyle.None;
                questionText.text = q.questionText;
                PlayPronunciation(q.content);
                break;

            case "abbreviation":
                questionImage.style.display = DisplayStyle.None;
                questionText.text = q.questionText + "\n\n" + q.content;
                break;
        }

        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (i < q.options.Length)
            {
                optionTexts[i].text = q.options[i];
            }
        }
    }

    void LoadImage(string imagePath)
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, imagePath);
        StartCoroutine(LoadImageCoroutine(fullPath));
    }

    IEnumerator LoadImageCoroutine(string path)
    {
        WWW www = new WWW(path);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            Texture2D texture = www.texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            questionImage.sprite = sprite;
        }
    }

    void PlayPronunciation(string content)
    {
        string audioPath = Path.Combine(Application.streamingAssetsPath, "Audio", content + ".wav");
        StartCoroutine(LoadAudioCoroutine(audioPath));
    }

    IEnumerator LoadAudioCoroutine(string path)
    {
        WWW www = new WWW(path);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            audioSource.clip = www.GetAudioClip();
            audioSource.Play();
        }
    }

    public void OnOptionClicked(int optionIndex)
    {
        if (isAnswered) return;

        isAnswered = true;
        Question q = currentRoundQuestions[currentQuestionIndex];

        // Reset all button colors to default (if needed)
        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].RemoveFromClassList("correct");
            optionButtons[i].RemoveFromClassList("wrong");
            optionButtons[i].RemoveFromClassList("disabled");
            optionButtons[i].RemoveFromClassList("enabled");
            optionButtons[i].style.backgroundColor = new StyleColor(Color.white);
            optionButtons[i].SetEnabled(true);
        }

        if (optionIndex == q.correctAnswer)
        {
            // Set correct answer button to green
            optionButtons[optionIndex].style.backgroundColor = Color.green;
            optionButtons[optionIndex].AddToClassList("correct");
            score += 10;
        }
        else
        {
            // Set selected button to red
            optionButtons[optionIndex].style.backgroundColor = Color.red;
            optionButtons[optionIndex].AddToClassList("wrong");
            // Highlight correct answer
            optionButtons[q.correctAnswer].style.backgroundColor = Color.green;
            optionButtons[q.correctAnswer].AddToClassList("correct");
        }

        // Disable all buttons
        foreach (var btn in optionButtons)
        {
            btn.SetEnabled(false);
            btn.AddToClassList("disabled");
        }

        scoreText.text = "Score: " + score;

        // Proceed to next question after delay
        Invoke("NextQuestion", 2f);
    }

    void NextQuestion()
    {
        currentQuestionIndex++;
        ShowQuestion();
    }

    void TimeOut()
    {
        isAnswered = true;

        // Reset button styles
        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].RemoveFromClassList("correct");
            optionButtons[i].RemoveFromClassList("wrong");
            optionButtons[i].RemoveFromClassList("disabled");
            optionButtons[i].style.backgroundColor = new StyleColor(Color.white);
            optionButtons[i].SetEnabled(true); // Enable buttons if needed
        }

        // Highlight correct answer
        optionButtons[currentRoundQuestions[currentQuestionIndex].correctAnswer].style.backgroundColor = Color.green;
        optionButtons[currentRoundQuestions[currentQuestionIndex].correctAnswer].AddToClassList("correct");

        // Disable all buttons
        foreach (var btn in optionButtons)
        {
            btn.SetEnabled(false);
        }

        // Proceed after delay
        Invoke("NextQuestion", 2f);
    }

    void EndRound()
    {
        // gameOverPanel.SetActive(true);
        // finalScoreText.text = "GameOver\nScore: " + score;
    }

    public void RestartGame()
    {
        score = 0;
        currentLevel = 1;
        currentQuestionIndex = 0;
        scoreText.text = "Score: 0";
        // gameOverPanel.SetActive(false);
        StartNewRound();
    }
}