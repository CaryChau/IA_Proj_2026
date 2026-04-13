using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;

public class ListeningQuestion : QuestionBase
{
    public ListeningQuestion(VisualElement page, JToken data) : base(page, data)
    {
    }
}
