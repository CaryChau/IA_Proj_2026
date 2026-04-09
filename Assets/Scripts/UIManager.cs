using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UIManager
{

    private static UIManager _uiMgr = null;
    private string knowledgeData;
    public string KnowledgeData
    {
        get
        {
            return knowledgeData;
        }
    }
    public static UIManager GetInstance()
    {
        if (_uiMgr == null)
        {
            _uiMgr = new UIManager();
        }
        return _uiMgr;
    }
    public Texture2D newHeaderTexture;

    private ProfileViewModel profileVM = null;
    public delegate void EnableCreator();
    public EnableCreator enableCreator;

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

    public void RequestProfileData()
    {
        
    }

    // public void OnProfileData(NetworkMessage msg) // json obj?
    // {
    //     // profileVM.SetAll(jsonObj)
    // }

    public void RequestCourseData()
    {
        
    }

    public void OnCourseData(string data)
    {
        // get current studying course, so that level creator can setup level content and topbar content
    }

    public void RequestionKnowledge(string topic, int difficulty)
    {
        
    }

    public void OnKnowledgeData(string data)
    {
        knowledgeData = data;
        enableCreator?.Invoke();
    }
}
