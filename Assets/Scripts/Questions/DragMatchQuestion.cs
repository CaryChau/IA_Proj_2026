using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;

public class DragMatchQuestion : QuestionBase
{
    public DragMatchQuestion(VisualElement page, JToken data) : base(page, data)
    {
    }
}
