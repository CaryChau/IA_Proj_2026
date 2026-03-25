using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

public class ProfileCreator : MonoBehaviour
{
    public VisualTreeAsset profileTemplate;
    private VisualElement profileView;  // instance created from profileTemplate
    private VisualElement profilePage;  // instance created from profileTemplate
    private ProfileViewModel model;
    private Label nameLabel;
    private Label metaLabel;
    private VisualElement headerIllustration;
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        profilePage = root.Q<VisualElement>("ProfilePage");
        SetupProfilePage();
        BindToModel();
        // send protocol later
    }

    


    private void SetupProfilePage()
    {
        if (profileTemplate == null)
        {
            Debug.LogError("ProfilePageInstaller: profileTemplate is not assigned.");
            return;
        }

        // Instantiate the template
        profileView = profileTemplate.Instantiate();
        profilePage.Add(profileView);
        profileView.name = "ProfilePageView";

        // Ensure it stretches to fill the host page area
        profileView.style.width = Length.Percent(100);
        profileView.style.height = Length.Percent(100);


        // Bind your UI controls (optional)
        BindProfileUI(profileView);
    }

    private void BindProfileUI(VisualElement view)
    {
        // Example: set texts
        nameLabel = view.Q<Label>("UserNameLabel");
        if (nameLabel != null) nameLabel.text = "Matt";

        metaLabel = view.Q<Label>("UserMetaLabel");
        if (metaLabel != null) metaLabel.text = "@DCiiieee • Joined August 2014";

        // Example: settings button callback
        var settingsButton = view.Q<Button>("SettingsButton");
        if (settingsButton != null)
        {
            settingsButton.clicked -= OnSettingsClicked; // prevent double binding
            settingsButton.clicked += OnSettingsClicked;
        }

        // header illustration background
        headerIllustration = view.Q<VisualElement>("HeaderIllustration");
        // header.style.backgroundImage = new StyleBackground(myTexture);
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Settings clicked (Profile Page)");
    }

    
    private void BindToModel()
    {

        // Ensure no duplicate subscriptions
        // UnbindModel();

        model = UIManager.GetInstance().profileViewModel;

        model.NameChanged += UpdateNameLabel;
        model.MetaChanged += UpdateMetaLabel;
        model.HeaderChanged += UpdateHeader;

        // Initial paint (apply current values immediately)
        // UpdateNameLabel(model.Name);
        // UpdateMetaLabel(model.Meta);
        // UpdateHeader(model.HeaderTexture);
    }

    public void UnbindModel()
    {

        model.NameChanged -= UpdateNameLabel;
        model.MetaChanged -= UpdateMetaLabel;
        model.HeaderChanged -= UpdateHeader;
    }

    // ---------------------------
    // Update functions (what you asked for)
    // ---------------------------

    private void UpdateNameLabel(string newName)
    {
        if (nameLabel == null) return;
        nameLabel.text = string.IsNullOrEmpty(newName) ? "-" : newName;
    }

    private void UpdateMetaLabel(string newMeta)
    {
        if (metaLabel == null) return;
        metaLabel.text = string.IsNullOrEmpty(newMeta) ? "" : newMeta;
    }

    private void UpdateHeader(Texture2D headerTexture)
    {
        if (headerIllustration == null) return;

        if (headerTexture == null)
        {
            // Clear background image
            headerIllustration.style.backgroundImage = StyleKeyword.None;
            return;
        }

        headerIllustration.style.backgroundImage = new StyleBackground(headerTexture);
    }

}