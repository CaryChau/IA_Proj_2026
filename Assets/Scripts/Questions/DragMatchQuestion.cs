using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;
using System.Linq;

public class DragMatchQuestion : QuestionBase
{
    private List<Button> leftButtons = new List<Button>();
    private List<Button> rightButtons = new List<Button>();
    
    private Button selectedLeft;
    private Button selectedRight;

    // 儲存正確配對：Key = TargetId, Value = DraggableId
    private Dictionary<string, string> matchMap = new Dictionary<string, string>();
    private int completedMatches = 0;
    private int totalMatchesNeeded = 0;

    public DragMatchQuestion(VisualElement page, JToken data) : base(page, data)
    {
        InitPage();
    }

    private void InitPage()
    {
        // 1. 解析 JSON 數據
        JArray draggables = questionData["draggables"] as JArray;
        JArray targets = questionData["targets"] as JArray;
        JArray correctMatches = questionData["correctMatches"] as JArray;

        totalMatchesNeeded = targets.Count;

        // 建立正確答案字典
        foreach (var m in correctMatches)
        {
            matchMap[m.Value<string>("targetId")] = m.Value<string>("draggableId");
        }

        // 2. 獲取 UI 元件 (假設左右兩欄分別在兩個 .column 中)
        var columns = pageInstance.Query<VisualElement>(className: "column").ToList();
        var leftColumn = columns[0];
        var rightColumn = columns[1];

        // 清空並初始化按鈕
        leftColumn.Clear();
        rightColumn.Clear();

        // 3. 生成左側按鈕 (Targets)
        foreach (var t in targets)
        {
            Button btn = CreateCard(t.Value<string>("text"), t.Value<string>("id"));
            btn.clicked += () => OnLeftSelected(btn);
            leftButtons.Add(btn);
            leftColumn.Add(btn);
        }

        // 4. 生成右側按鈕 (Draggables)
        foreach (var d in draggables)
        {
            Button btn = CreateCard(d.Value<string>("text"), d.Value<string>("id"));
            btn.clicked += () => OnRightSelected(btn);
            rightButtons.Add(btn);
            rightColumn.Add(btn);
        }
    }

    private Button CreateCard(string text, string id)
    {
        Button btn = new Button { text = text };
        btn.AddToClassList("card");
        btn.name = id; // 將 ID 存於 Name 方便辨識
        return btn;
    }

    private void OnLeftSelected(Button btn)
    {
        if (btn.ClassListContains("card-disabled")) return;
        
        ClearSelection(leftButtons);
        selectedLeft = btn;
        btn.AddToClassList("card-selected");
        
        CheckMatch();
    }

    private void OnRightSelected(Button btn)
    {
        if (btn.ClassListContains("card-disabled")) return;

        ClearSelection(rightButtons);
        selectedRight = btn;
        btn.AddToClassList("card-selected");

        CheckMatch();
    }

    private void CheckMatch()
    {
        if (selectedLeft != null && selectedRight != null)
        {
            string targetId = selectedLeft.name;
            string draggableId = selectedRight.name;

            // 檢查配對是否正確
            if (matchMap.ContainsKey(targetId) && matchMap[targetId] == draggableId)
            {
                // 正確：變灰且禁用
                MarkAsMatched(selectedLeft);
                MarkAsMatched(selectedRight);
                completedMatches++;

                // 檢查是否全部完成
                if (completedMatches == totalMatchesNeeded)
                {
                    onCheck?.Invoke(true); // 觸發 KnowledgeCreator 的成功彈窗
                }
            }
            else
            {
                // 錯誤：清除選中狀態
                ClearSelection(leftButtons);
                ClearSelection(rightButtons);
            }

            selectedLeft = null;
            selectedRight = null;
        }
    }

    private void MarkAsMatched(Button btn)
    {
        btn.RemoveFromClassList("card-selected");
        btn.AddToClassList("card-disabled");
        btn.SetEnabled(false);
    }

    private void ClearSelection(List<Button> buttons)
    {
        foreach (var b in buttons) b.RemoveFromClassList("card-selected");
    }
}