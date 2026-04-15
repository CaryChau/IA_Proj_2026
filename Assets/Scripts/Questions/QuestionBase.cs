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
public abstract class QuestionBase
{
    public OnCheckHandler onCheck;
    protected VisualElement pageInstance;
    protected JToken questionData;

    protected abstract void InitPage();

    public void LeavePage(Action<VisualElement> cb)
    {
        var animRoot = pageInstance.Q<VisualElement>("QuestionRoot");
        animRoot.AddToClassList("QuestionLeave");
        animRoot.RegisterCallback<TransitionEndEvent>(evt =>
        {
            Debug.Log("Remove old page: " + animRoot.name);
            cb?.Invoke(pageInstance);
        });
    }

    public void EnterPage()
    {
        // var animRoot = pageInstance.Q<VisualElement>("QuestionRoot");
        // animRoot.style.opacity = 0;
        // animRoot.RemoveFromClassList("FadeInDefault");
        // animRoot.AddToClassList("FadeTarget");
        PageAnimator.FadeTo(pageInstance, 0, 1, 500);
    }
    
    public QuestionBase(VisualElement page, JToken data)
    {
        pageInstance = page;
        questionData = data;
    }
}
