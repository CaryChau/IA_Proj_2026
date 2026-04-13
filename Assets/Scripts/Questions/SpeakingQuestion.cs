using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;

public class SpeakingQuestion : QuestionBase
{
    public SpeakingQuestion(VisualElement page, JToken data) : base(page, data)
    {
    }
}
