using System;
using UnityEngine;
using UnityEngine.UIElements;

public class LoginCreator : MonoBehaviour
{
    [Header("UI Document References")]
    public VisualTreeAsset loginPageAsset;
    public VisualTreeAsset inputPageAsset;
    public VisualTreeAsset targetSetupPageAsset; // Placeholder for future use

    private VisualElement root;
    private VisualElement loginPage;
    private VisualElement inputPage;
    private VisualElement targetSetupPage;

    private void Awake()
    {
        var uiDoc = GetComponent<UIDocument>();
        root = uiDoc.rootVisualElement;
        ShowLoginPage();
    }

    private void ShowLoginPage()
    {
        root.Clear();
        loginPage = loginPageAsset.Instantiate();
        root.Add(loginPage);

        var signInBtn = loginPage.Q<Button>("SignInButton");
        var getStartedBtn = loginPage.Q<Button>("GetStartedButton");

        if (signInBtn != null)
            signInBtn.clicked += OnSignInClicked;
        if (getStartedBtn != null)
            getStartedBtn.clicked += OnGetStartedClicked;
    }

    private void OnSignInClicked()
    {
        if (inputPage == null)
            inputPage = inputPageAsset.Instantiate();
        FadeToPage(inputPage, OnInputPageShow);
    }

    private void OnGetStartedClicked()
    {
        if (targetSetupPage == null && targetSetupPageAsset != null)
            targetSetupPage = targetSetupPageAsset.Instantiate();
        if (targetSetupPage != null)
            FadeToPage(targetSetupPage);
    }

    private void FadeToPage(VisualElement page, Action cb = null)
    {
        if (root.childCount > 0)
        {
            var oldPage = root[0];
            PageAnimator.FadeTo(oldPage, 1f, 0f, 300, () => {
                root.Clear();
                root.Add(page);
                PageAnimator.FadeTo(page, 0f, 1f, 300);
                cb?.Invoke();
            });
        }
        else
        {
            root.Clear();
            root.Add(page);
            PageAnimator.FadeTo(page, 0f, 1f, 300);
        }
    }

    private void OnInputPageShow()
    {
        if (inputPage == null) return;

        var signInBtn = inputPage.Q<Button>("SignInButton");
        var usernameField = inputPage.Q<TextField>("UsernameField");
        var passwordField = inputPage.Q<TextField>("PasswordField");

        // Optionally, add a waiting icon (spinner) to the page
        VisualElement waitingIcon = new VisualElement();
        waitingIcon.style.width = 32;
        waitingIcon.style.height = 32;
        waitingIcon.style.backgroundImage = new StyleBackground(/* assign your spinner texture here if available */);
        waitingIcon.style.alignSelf = Align.Center;
        waitingIcon.style.marginTop = 16;
        waitingIcon.style.display = DisplayStyle.None;
        inputPage.Add(waitingIcon);

        // Optionally, add a label for error messages
        Label errorLabel = new Label();
        errorLabel.style.color = new StyleColor(Color.red);
        errorLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        errorLabel.style.marginTop = 8;
        errorLabel.style.display = DisplayStyle.None;
        inputPage.Add(errorLabel);

        if (signInBtn != null)
        {
            signInBtn.clicked += () =>
            {
                errorLabel.style.display = DisplayStyle.None;
                string username = usernameField?.value ?? "";
                string password = passwordField?.value ?? "";

                // Simple validation
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    errorLabel.text = "Please enter username and password.";
                    errorLabel.style.display = DisplayStyle.Flex;
                    return;
                }

                // Show waiting icon
                waitingIcon.style.display = DisplayStyle.Flex;
                signInBtn.SetEnabled(false);

                // Simulate async request (replace with your real async call)
                StartCoroutine(SimulateSignInRequest(username, password, (success) =>
                {
                    waitingIcon.style.display = DisplayStyle.None;
                    signInBtn.SetEnabled(true);
                    if (success)
                    {
                        // TODO: Jump to app content module (call your navigation logic here)
                        Debug.Log("Login success! Jump to app content module.");
                    }
                    else
                    {
                        errorLabel.text = "Wrong username or password.";
                        errorLabel.style.display = DisplayStyle.Flex;
                    }
                }));
            };
        }
    }

    // Simulate an async sign-in request (replace with your real network logic)
    private System.Collections.IEnumerator SimulateSignInRequest(string username, string password, System.Action<bool> callback)
    {
        yield return new WaitForSeconds(1.2f); // Simulate network delay
        // For demo: username == "user", password == "pass" is valid
        bool success = (username == "user" && password == "pass");
        callback?.Invoke(success);
    }
}
