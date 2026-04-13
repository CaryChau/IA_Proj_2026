using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;

public class SpellingQuestion : QuestionBase
{
    public SpellingQuestion(VisualElement page, JToken data) : base(page, data)
    {
    }
}
