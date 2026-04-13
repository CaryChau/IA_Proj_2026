using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UIElements;
public enum QuestionType
{
    DragMatch = 0,
    TrueFalse = 1,
    SelectOne = 2,
    Listening = 3,
    Spelling = 4,
    Speaking = 5
}
public class QuestionBase
{
    public NextActionHandler onNext;
    public OnCheckHandler onCheck;
    protected VisualElement pageInstance;
    protected JToken questionData;
    
    public QuestionBase(VisualElement page, JToken data)
    {
        pageInstance = page;
        questionData = data;
    }
}
