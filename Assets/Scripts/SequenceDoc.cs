using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SequenceDoc : MonoBehaviour
{
    public UIDocument uiDoc;
    public DocType id;
    public bool executed = false;
    // jump to what ID?
    public DocType targetId = DocType.None;
    // public void OnSwitchPanel(DocType targetId, )
    public SequenceDoc(DocType initId)
    {
        
    }
    protected void SetTarget(DocType id)
    {
        targetId = id;
        executed = true;
    }
}
