using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    }

    [Header("UI References")]
    public TMP_Text questionText;
    public Image questionImage;
    public Button[] optionButtons;
    public TMP_Text[] optionTexts;
    public TMP_Text scoreText;
    public TMP_Text levelText;
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
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

    void Start()
    {
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
            float fillAmount = timer / time;
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
            timerFill.fillAmount = 1f;
            timerFill.color = Color.green;
        }
        
        if (levelText != null)
        {
            levelText.text = "Level: " + (currentQuestionIndex + 1);
        }
        
        Question q = currentRoundQuestions[currentQuestionIndex];
        
        foreach (Button btn in optionButtons)
        {
            btn.interactable = true;
            btn.GetComponent<Image>().color = Color.white;
        }

        switch (q.type)
        {
            case "picture":
                questionImage.gameObject.SetActive(true);
                LoadImage(q.content);
                questionText.text = q.questionText;
                break;
                
            case "word":
                questionImage.gameObject.SetActive(false);
                questionText.text = q.questionText + "\n\n" + q.content;
                break;
                
            case "prononuce":
                questionImage.gameObject.SetActive(false);
                questionText.text = q.questionText;
                PlayPronunciation(q.content);
                break;
                
            case "abbreviation":
                questionImage.gameObject.SetActive(false);
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
        
        if (optionIndex == q.correctAnswer)
        {
            optionButtons[optionIndex].GetComponent<Image>().color = Color.green;
            score += 10;
        }
        else
        {
            optionButtons[optionIndex].GetComponent<Image>().color = Color.red;
            optionButtons[q.correctAnswer].GetComponent<Image>().color = Color.green;
        }

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
        isAnswered = true;
        
        Question q = currentRoundQuestions[currentQuestionIndex];
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
        finalScoreText.text = "GameOver\nScore: " + score;
    }

    public void RestartGame()
    {
        score = 0;
        currentLevel = 1;
        currentQuestionIndex = 0;
        scoreText.text = "Score: 0";
        gameOverPanel.SetActive(false);
        StartNewRound();
    }
}