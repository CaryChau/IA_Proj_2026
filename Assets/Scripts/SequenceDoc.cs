using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class OnSwitchArgs{

}
public class SequenceDoc : MonoBehaviour
{
    public OnSwitchArgs args;
    public UIDocument uiDoc;
    public DocType id;
    public bool executed = false;
    // jump to what ID?
    public DocType targetId = DocType.None;

    public SequenceDoc(DocType initId)
    {
        
    }
    public virtual void OnDocSwitch(OnSwitchArgs args)
    {
        this.args = args;
    }
    protected void SetTarget(DocType id, OnSwitchArgs args)
    {
        targetId = id;
        executed = true;
        this.args = args;
    }
}
