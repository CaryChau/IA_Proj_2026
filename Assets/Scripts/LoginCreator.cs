using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class LoginCreator : SequenceDoc
{
    [Header("UI Document References")]
    public VisualTreeAsset loginPageAsset;
    public VisualTreeAsset inputPageAsset;
    public VisualTreeAsset targetSetupPageAsset; // Placeholder for future use

    private VisualElement root;
    private VisualElement pageContainer;
    private VisualElement loginPage;
    private VisualElement inputPage;
    private VisualElement curPage;
    private UIDocument uiDoc;

    private void Awake()
    {
        uiDoc = GetComponent<UIDocument>();
        root = uiDoc.rootVisualElement;
        pageContainer = root.Q<VisualElement>("PageContainer");
        pageContainer.style.backgroundColor = new StyleColor(Color.black);
        ShowLoginPage();
    }

    private void ShowLoginPage()
    {
        pageContainer.Clear();
        loginPage = loginPageAsset.Instantiate();
        loginPage.style.height = Length.Percent(100f);;
        pageContainer.Add(loginPage);


        var signInBtn = loginPage.Q<Button>("SignInButton");
        var transferBtn = loginPage.Q<Button>("TransferBtn");

        if (signInBtn != null)
            signInBtn.clicked += OnSignInClicked;
        if (transferBtn != null)
            transferBtn.clicked += OnTransferBtnClicked;
    }

    private void OnSignInClicked()
    {
        if (inputPage == null)
        {
            inputPage = inputPageAsset.Instantiate();
            inputPage.style.height = Length.Percent(100f);
        }
        FadeToPage(inputPage, OnInputPageShow);
    }

    private void OnBackToLoginClicked()
    {
        if (loginPage == null)
        {
            loginPage = loginPageAsset.Instantiate();
            loginPage.style.height = Length.Percent(100f);
        }
        FadeToPage(loginPage, () => {
            ShowLoginPage();
            inputPage = null;
        });
    }

    private void OnTransferBtnClicked()
    {
        if (inputPage == null)
        {
            inputPage = inputPageAsset.Instantiate();
            inputPage.style.height = Length.Percent(100f);
        }
        FadeToPage(inputPage, OnTransferPageShow);
    }

    private void FadeToPage(VisualElement page, Action cb = null)
    {
        if (pageContainer.childCount > 0)
        {
            var oldPage = pageContainer[0];
            PageAnimator.FadeTo(oldPage, 1f, 0f, 300, () => {
                pageContainer.Clear();
                pageContainer.Add(page);
                PageAnimator.FadeTo(page, 0f, 1f, 300);
                // curPage = page;
                cb?.Invoke();
            });
        }
        else
        {
            pageContainer.Clear();
            pageContainer.Add(page);
            // curPage = page;

            PageAnimator.FadeTo(page, 0f, 1f, 300);
        }
    }

    private delegate void OnTextFieldValueChangeDel(EventCallback<ChangeEvent<string>> evt);
    private OnTextFieldValueChangeDel onTextFieldValueChange;

    private void UpdateBtnState(ref TextField[] input, Button btn)
    {
        int length = input.Length;
        TextField[] tempArr = input;
        for (int i = 0; i < length; i++)
        {
            TextField temp = tempArr[i];
            // if (onTextFieldValueChange != null)
            // {
            //     temp.UnregisterValueChangedCallback(onTextFieldValueChange);
            // }
            // onTextFieldValueChange = ;
            temp.RegisterValueChangedCallback(evt =>
            {
                string val = temp?.value ?? "";
                bool isAllFill = !string.IsNullOrWhiteSpace(val);
                if (!isAllFill)
                {
                    btn.AddToClassList("isDisabled");
                    return;
                }
                for (int j = 0; j < length; j++)
                {
                    if (i != j)
                    {
                        isAllFill = !string.IsNullOrWhiteSpace(tempArr[j]?.value ?? "");
                        if (!isAllFill)
                        {
                            break;
                        }
                    }
                }
                if (isAllFill)
                {
                    btn.RemoveFromClassList("isDisabled");
                }
            });
        }
    }

    private void OnTransferPageShow()
    {
        if (inputPage == null) return;
        var backBtn = inputPage.Q<Button>("BackBtn");
        backBtn.clicked += OnBackToLoginClicked;

        var signInBtn = inputPage.Q<Button>("SignInButton");
        signInBtn.AddToClassList("isDisabled");
        signInBtn.text = "ACCOUNT TRANSFER";
        var usernameField = inputPage.Q<TextField>("UsernameField");
        usernameField.SetValueWithoutNotify("");
        var transferKeyField = inputPage.Q<VisualElement>("TransferKeyField");
        transferKeyField.style.display = DisplayStyle.Flex;

        transferKeyField = inputPage.Q<TextField>("PasswordField");
        TextField[] arr = new TextField[2]{usernameField, ((TextField)transferKeyField)};
        UpdateBtnState(ref arr, signInBtn);
        usernameField[0][0].style.fontSize = 50;
        transferKeyField[0][0].style.fontSize = 50;

        usernameField[0].style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        transferKeyField[0].style.height = new StyleLength(new Length(100, LengthUnit.Percent));
    

        // Optionally, add a label for error messages
        Label errorLabel = new Label();
        errorLabel.style.fontSize = 40;
        errorLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

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
                string transferKey = ((TextField)transferKeyField)?.value ?? "";

                // Simple validation
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(transferKey))
                {
                    errorLabel.text = "Please enter Account ID and Transfer Key.";
                    errorLabel.style.display = DisplayStyle.Flex;
                    return;
                }

                // signInBtn.SetEnabled(false);
                signInBtn.RemoveFromClassList("isDisabled");
                
                
                StartCoroutine(SetTransferKeyRequest(username, transferKey, (success, statusCode) =>
                {
                    if (success)
                    {
                        errorLabel.text = "Transfer key set successfully.";
                        errorLabel.style.display = DisplayStyle.Flex;
                        Debug.Log("Transfer key set successfully!");
                    }
                    else
                    {
                        Debug.LogError($"Failed to set transfer key. HTTP Status: {statusCode}");
                    }
                }));
            };
        }
    }

    private IEnumerator SetTransferKeyRequest(
    string accountID,
    string transferKey,
    System.Action<bool, int> callback // bool: success/fail, int: HTTP status
    ) {
        string jsonBody = $"{{\"account_id\": \"{accountID}\", \"transfer_key\": \"{transferKey}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        // Replace with your backend endpoint for setting transfer key
        string setTransferKeyUrl = "https://your-backend.com/api/v1/accounts/transfer";

        using (UnityWebRequest req = new UnityWebRequest(setTransferKeyUrl, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            int status = (int)req.responseCode;
    #if UNITY_2020_1_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
    #else
            if (req.isNetworkError || req.isHttpError)
    #endif
            {
                Debug.LogError($"SetTransferKey error: {req.error}, HTTP Status: {status}");
                callback?.Invoke(false, status);
            }
            else
            {
                // 200 = success
                bool success = (status == 200);
                callback?.Invoke(success, status);
            }
        }
    }

    private void OnInputPageShow()
    {
        if (inputPage == null) return;

        var backBtn = inputPage.Q<Button>("BackBtn");
        backBtn.clicked += OnBackToLoginClicked;
        var signInBtn = inputPage.Q<Button>("SignInButton");
        signInBtn.AddToClassList("isDisabled");
        signInBtn.text = "SIGN IN";
        var usernameField = inputPage.Q<TextField>("UsernameField");
        TextField[] arr = new TextField[1]{usernameField};

        UpdateBtnState(ref arr, signInBtn);
        var transferKeyField = inputPage.Q<VisualElement>("TransferKeyField");
        transferKeyField.style.display = DisplayStyle.None;
        usernameField[0][0].style.fontSize = 50;
        // transferKeyField[0][0].style.fontSize = 50;

        usernameField[0].style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        // transferKeyField[0].style.height = new StyleLength(new Length(100, LengthUnit.Percent));
    

        // Optionally, add a label for error messages
        Label errorLabel = new Label();
        errorLabel.style.fontSize = 40;
        errorLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

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

                // Simple validation
                if (string.IsNullOrWhiteSpace(username)) //  || string.IsNullOrWhiteSpace(transferKey)
                {
                    errorLabel.text = "Please enter Account ID.";
                    errorLabel.style.display = DisplayStyle.Flex;
                    return;
                }

                signInBtn.RemoveFromClassList("isDisabled");
                // test jump function
                SetTarget(DocType.Navigation, null);
                Debug.Log("Login success! Jump to app content module.");
                // SignIn(username, SystemInfo.deviceUniqueIdentifier, (success, statusCode) =>
                // {
                    
                //     signInBtn.SetEnabled(true);
                //     if (success)
                //     {
                //         // TODO: Jump to app content module (call your navigation logic here)
                //         Debug.Log("Login success! Jump to app content module.");
                //     }
                //     else if(statusCode == 410)
                //     {
                //         errorLabel.text = "Device ID no longer accessible with this account.";
                //         errorLabel.style.display = DisplayStyle.Flex;
                //     }
                //     else
                //     {
                //         errorLabel.text = "Sign in failed. Please check your credentials or internet connection.";
                //         errorLabel.style.display = DisplayStyle.Flex;
                //     }
                // });
            };
        }
    }

    [SerializeField] private string backendUrl = "https://your-backend.com/api/v1/account/me";

    public LoginCreator(DocType initId = DocType.Login) : base(initId)
    {
    }

    public void SignIn(string accountID, string deviceID, System.Action<bool, int> callback)
    {
        StartCoroutine(SignInRequest(accountID, deviceID, callback));
    }

    private IEnumerator SignInRequest(string accountID, string deviceID, System.Action<bool, int> callback)
    {
        string jsonBody = $"{{\"account_id\": \"{accountID}\", \"device_id\": \"{deviceID}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest req = new UnityWebRequest(backendUrl, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();
            int status = (int)req.responseCode;
#if UNITY_2020_1_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError($"SignIn error: {req.error}, HTTP Status: {status}");

                // Explicitly handle HTTP 410 Gone (device ID no longer valid)
                if (status == 410)
                {
                    Debug.LogWarning("Device ID no longer accessible for this account.");
                    callback?.Invoke(false, status);
                }
                else
                {
                    callback?.Invoke(false, status);
                }
            }
            else
            {

                callback?.Invoke(true, status);
            }
        }
    }
}
