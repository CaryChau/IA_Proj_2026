using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    private static UIManager _uiMgr = null;
    public UIManager()
    {
        if (!_uiMgr)
        {
            _uiMgr = new UIManager();
        }
    }
    public static UIManager GetInstance()
    {
        return _uiMgr;
    }
    public Texture2D newHeaderTexture;

    private ProfileViewModel profileVM = null;

    // protocol callback
    private void OnProfileUpdate()
    {

        // simulate updates
        profileVM.SetAll("Matt", "@DCiiieee • Joined August 2014", newHeaderTexture);

    }

    public ProfileViewModel profileViewModel
    {
        get
        {
            if (profileVM == null)
            {
                profileVM = new ProfileViewModel();
            } 
            return profileVM;
        }
    }
}
