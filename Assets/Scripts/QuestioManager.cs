using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;

public class QuestionManager : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string type;
        public string content;
        public string[] options;
        public int correctAnswer;
        public string questionText;
        public bool isCorrect;
        public string playerAnswer;
    }

    [System.Serializable]
    public class QuestionTypeSetting
    {
        public string typeName;
        public bool isEnabled;
    }

    [Header("UI References")]
    public TMP_Text questionText;
    public Image questionImage;
    public Button[] optionButtons;
    public TMP_Text[] optionTexts;
    public Image[] optionImages;
    public TMP_Text scoreText;
    public TMP_Text levelText;
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public AudioSource audioSource;
    public Image timerFill; 

    [Header("End Panel Buttons")]
    public Button mainMenuButton;
    public Button restartButton;
    public Button reviewButton;
    public Button nextLevelButton;
    
    [Header("Review Panel")]
    public GameObject reviewPanel;
    public Transform reviewContentParent;
    public GameObject reviewItemPrefab;

    [Header("Game Settings")]
    public string name = "Questions.txt";
    public int amount = 8;
    public float times = 10f;
    
    [Header("kind")]
    public QuestionTypeSetting[] questionTypes = new QuestionTypeSetting[]
    {
        new QuestionTypeSetting { typeName = "picture", isEnabled = true },
        new QuestionTypeSetting { typeName = "word", isEnabled = false },
        new QuestionTypeSetting { typeName = "pronounce", isEnabled = false },
        new QuestionTypeSetting { typeName = "abbreviation", isEnabled = false }
    };

    private List<Question> allQuestions = new List<Question>();
    private List<Question> currentRoundQuestions = new List<Question>();
    private List<Question> answeredQuestions = new List<Question>();
    private int currentQuestionIndex = 0;
    private int score = 0;
    private int currentLevel = 1;
    private float timer;
    private bool isAnswered = false;

    void Start()
    {
        LoadQuestions();
        SetupEndButtons();
        StartNewRound();
    }

    void Update()
    {
        if (!isAnswered && currentQuestionIndex < currentRoundQuestions.Count)
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
            float fillAmount = timer / times;
            timerFill.fillAmount = fillAmount;
            
            if (fillAmount > 0.5f)
            {
                timerFill.color = Color.green;
            }
            else if (fillAmount > 0.2f)
            {
                timerFill.color = Color.yellow;
            }
            else
            {
                timerFill.color = Color.red;
            }
        }
    }

    void SetupEndButtons()
    {
        mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene("Main"));
        restartButton.onClick.AddListener(RestartGame);
        reviewButton.onClick.AddListener(ShowReviewPanel);
        nextLevelButton.onClick.AddListener(GoToNextLevel);
        
        nextLevelButton.interactable = false;
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
        answeredQuestions.Clear();
        currentQuestionIndex = 0;
        score = 0;
        scoreText.text = "Score: 0";
        
        List<string> enabledTypes = questionTypes
            .Where(t => t.isEnabled)
            .Select(t => t.typeName)
            .ToList();
        
        for (int i = 0; i < amount; i++)
        {
            string randomType = enabledTypes[Random.Range(0, enabledTypes.Count)];
            List<Question> typeQuestions = allQuestions.FindAll(q => q.type == randomType);
            
            if (typeQuestions.Count > 0)
            {
                int randomIndex = Random.Range(0, typeQuestions.Count);
                Question selectedQuestion = typeQuestions[randomIndex];
                
                Question newQuestion = new Question
                {
                    type = selectedQuestion.type,
                    content = selectedQuestion.content,
                    options = selectedQuestion.options.ToArray(),
                    correctAnswer = selectedQuestion.correctAnswer,
                    questionText = selectedQuestion.questionText,
                    isCorrect = false,
                    playerAnswer = ""
                };
                
                currentRoundQuestions.Add(newQuestion);
            }
        }   
        ShowQuestion();
    }

    void ShowQuestion()
    {
        if (currentQuestionIndex >= currentRoundQuestions.Count)
        {
            EndRound();
            return;
        }

        isAnswered = false;
        timer = times;
        
        Question q = currentRoundQuestions[currentQuestionIndex];
        
        foreach (Button btn in optionButtons)
        {
            btn.interactable = true;
            btn.GetComponent<Image>().color = Color.white;
        }

        bool isImageOption = (q.type == "word");
        
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (isImageOption)
            {
                optionTexts[i].gameObject.SetActive(false);
                optionImages[i].gameObject.SetActive(true);
                StartCoroutine(LoadOptionImage(optionImages[i], q.options[i]));
            }
            else
            {
                optionTexts[i].gameObject.SetActive(true);
                optionImages[i].gameObject.SetActive(false);
                optionTexts[i].text = q.options[i];
            }
        }

        switch (q.type)
        {
            case "picture":
                questionImage.gameObject.SetActive(true);
                StartCoroutine(LoadImageCoroutine(q.content));
                questionText.text = q.questionText;
                break;
                
            case "word":
                questionImage.gameObject.SetActive(false);
                questionText.text = q.questionText + "\n\nword: " + q.content;
                break;
                
            case "pronounce":
                questionImage.gameObject.SetActive(false);
                questionText.text = q.questionText;
                PlayPronunciation(q.content);
                break;
                
            case "abbreviation":
                questionImage.gameObject.SetActive(false);
                questionText.text = q.questionText + "\n\n" + q.content;
                break;
        }
        
        levelText.text = "Level " + currentLevel;
    }

    IEnumerator LoadOptionImage(Image targetImage, string imagePath)
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, imagePath);
        WWW www = new WWW(fullPath);
        yield return www;
        
        if (string.IsNullOrEmpty(www.error))
        {
            Texture2D texture = www.texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            targetImage.sprite = sprite;
        }
    }

    IEnumerator LoadImageCoroutine(string path)
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, path);
        WWW www = new WWW(fullPath);
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
        
        q.playerAnswer = q.options[optionIndex];
        
        if (optionIndex == q.correctAnswer)
        {
            optionButtons[optionIndex].GetComponent<Image>().color = Color.green;
            score += 10;
            q.isCorrect = true;
        }
        else
        {
            optionButtons[optionIndex].GetComponent<Image>().color = Color.red;
            optionButtons[q.correctAnswer].GetComponent<Image>().color = Color.green;
            q.isCorrect = false;
        }

        answeredQuestions.Add(q);

        foreach (Button btn in optionButtons)
        {
            btn.interactable = false;
        }

        scoreText.text = "Score: " + score;
        
        Invoke("NextQuestion", 2f);
    }

    void NextQuestion()
    {
        currentQuestionIndex++;
        ShowQuestion();
    }

    void TimeOut()
    {
        if (isAnswered) return;
        
        isAnswered = true;
        
        Question q = currentRoundQuestions[currentQuestionIndex];
        
        q.playerAnswer = "Timeout";
        q.isCorrect = false;
        answeredQuestions.Add(q);
        
        optionButtons[q.correctAnswer].GetComponent<Image>().color = Color.green;
        
        foreach (Button btn in optionButtons)
        {
            btn.interactable = false;
        }
        
        Invoke("NextQuestion", 2f);
    }

    void EndRound()
    {
        gameOverPanel.SetActive(true);
        finalScoreText.text = "Score: " + score + " / 80";
        nextLevelButton.interactable = (score >= 0);
    }

    public void ShowReviewPanel()
    {
        reviewPanel.SetActive(true);
        
        foreach (Transform child in reviewContentParent)
        {
            Destroy(child.gameObject);
        }
        
        for (int i = 0; i < answeredQuestions.Count; i++)
        {
            Question q = answeredQuestions[i];
            GameObject item = Instantiate(reviewItemPrefab, reviewContentParent);
            
            Text[] texts = item.GetComponentsInChildren<Text>();
            if (texts.Length >= 3)
            {
                texts[0].text = "Question: " + (i + 1);
                texts[1].text = q.questionText;
                texts[2].text = "your answer: " + q.playerAnswer + "\nconrrect answer: " + q.options[q.correctAnswer];
            }
            
            Image bgImage = item.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = q.isCorrect ? Color.green : Color.red;
            }
        }
    }

    public void RestartGame()
    {
        score = 0;
        currentQuestionIndex = 0;
        scoreText.text = "Score: 0";
        gameOverPanel.SetActive(false);
        reviewPanel.SetActive(false);
        StartNewRound();
    }

    public void GoToNextLevel()
    {
        SceneManager.LoadScene("Main");
    }

    public void SetQuestionTypeEnabled(int typeIndex, bool enabled)
    {
        if (typeIndex >= 0 && typeIndex < questionTypes.Length)
        {
            questionTypes[typeIndex].isEnabled = enabled;
        }
    }
}