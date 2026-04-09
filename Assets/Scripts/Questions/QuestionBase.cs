using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    private VisualElement pageInstance;
    
    public QuestionBase(VisualElement page)
    {
        pageInstance = page;
    }
}
