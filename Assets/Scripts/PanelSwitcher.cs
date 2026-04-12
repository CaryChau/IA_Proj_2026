using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class PanelSwitcher : MonoBehaviour
{
    public enum HideMode
    {
        DisableGameObject, // disables GO => panel destroyed/recreated on toggle
        DisplayNone        // keeps GO enabled, hides via style => preserves state
    }

    [Header("Which UIDocuments?")]
    [Tooltip("If true, finds all UIDocuments in the scene automatically.")]
    [SerializeField] private bool autoDiscoverInScene = true;

    [Tooltip("Include inactive UIDocuments when auto-discovering.")]
    [SerializeField] private bool includeInactive = false;

    [Tooltip("Optional manual list; used if autoDiscoverInScene is false or to limit scope.")]
    [SerializeField] private List<SequenceDoc> documents = new();
    private UIDocument curWinner;

    private string[] jumpMap = {"ab", "bc"};

    [Header("Behavior")]
    [SerializeField] private HideMode hideMode = HideMode.DisplayNone;

    [Tooltip("Refresh every frame (useful if sorting orders change frequently). Otherwise call Refresh() manually.")]
    [SerializeField] private bool refreshEveryFrame = true;

    // Cache: reflection check for UIDocument.sortingOrder if present on this Unity version.
    private static PropertyInfo _uidocSortingProp;

    private void Awake()
    {
        if (autoDiscoverInScene)
        {
            Discover();
        }

        // Probe once for UIDocument.sortingOrder (available in newer Unity)
        // _uidocSortingProp = typeof(UIDocument).GetProperty("sortingOrder",
        //     BindingFlags.Public | BindingFlags.Instance);
    }

    private void OnEnable()
    {
        // Initial application
        Refresh();
    }

    private void Update()
    {
        if (refreshEveryFrame)
            Refresh();
    }

    /// <summary>
    /// Find all UIDocuments in the scene (optionally including inactive).
    /// </summary>
    public void Discover()
    {
        var found = FindObjectsOfType<SequenceDoc>(includeInactive).ToList();
        documents = found;
    }

    /// <summary>
    /// Re-evaluate all documents, show the one with the lowest effective order, hide the rest.
    /// </summary>
    public void Refresh()
    {
        var winner = curWinner;
        // for (int i = 0; i < documents.Count; i++)
        // {
        //     if (documents[i].executed)
        //     {
        //         string executedID = documents[i].id;

        //     }
        //     if (documents[i].uiDoc.sortingOrder == 0 && documents[i] != winner)
        //     {
        //         winner = documents[i];
        //         break;
        //     }
        // }

        if (curWinner != winner)
        {
            curWinner = winner;
        }
        else
        {
            return;
        }

        foreach (var doc in documents)
        {
            SetVisible(doc.uiDoc, doc == winner);
        }
    }

    /// <summary>
    /// Returns the effective sorting order for a UIDocument.
    /// Prefers UIDocument.sortingOrder if available; otherwise PanelSettings.sortingOrder; else 0.
    /// </summary>
    private int GetEffectiveOrder(UIDocument doc)
    {
        // Newer Unity: UIDocument.sortingOrder (reflect to be version-agnostic)
        if (_uidocSortingProp != null)
        {
            object val = _uidocSortingProp.GetValue(doc, null);
            if (val is int so)
                return so;
        }

        // Older Unity: use PanelSettings.sortingOrder
        if (doc != null)
        {
            return (int)doc.sortingOrder;
        }

        return 0;
    }

    /// <summary>
    /// Shows or hides the document.
    /// </summary>
    private void SetVisible(UIDocument doc, bool visible)
    {
        if (hideMode == HideMode.DisableGameObject)
        {
            if (doc.gameObject.activeSelf != visible)
                doc.gameObject.SetActive(visible);
            return;
        }

        // Hide via style => preserves panel, callbacks, and runtime widget state.
        var root = doc.rootVisualElement;
        if (root == null)
        {
            // If root isn't ready yet, try again on the next frame.
            // You can also schedule this via UI Toolkit's scheduler.
            StartCoroutine(ShowNextFrame(doc, visible));
            return;
        }

        root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        root.pickingMode   = visible ? PickingMode.Position : PickingMode.Ignore;
        doc.sortingOrder = visible ? 0 : 1;
    }

    private System.Collections.IEnumerator ShowNextFrame(UIDocument doc, bool visible)
    {
        yield return null;
        var root = doc.rootVisualElement;
        if (root != null)
        {
            root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            root.pickingMode   = visible ? PickingMode.Position : PickingMode.Ignore;
        }
    }
}