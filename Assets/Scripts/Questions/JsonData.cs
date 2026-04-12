using System;
using System.Collections.Generic;

[Serializable]
public class QuizData
{
    public string topic;
    public int difficulty;
    public List<LevelData> levels;
}

[Serializable]
public class LevelData
{
    public int level;
    public List<QuestionData> questions;
}

[Serializable]
public class QuestionData
{
    public string id;
    public string type; // drag_match, select_one, etc.
    public string prompt;
    public List<OptionData> options;
    public string correctOptionId;
}

[Serializable]
public class OptionData
{
    public string id;
    public string text;
}