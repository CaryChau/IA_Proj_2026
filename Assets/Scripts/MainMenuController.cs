using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        
        var root = uiDocument.rootVisualElement;

        Button checkButton = root.Q<Button>(className: "primary-button");

        if (checkButton != null)
        {
            checkButton.clicked += () => {
                Debug.Log("檢查按鈕被點擊了！");
            };
        }
    }
}