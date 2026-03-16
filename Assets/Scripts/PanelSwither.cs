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
    [SerializeField] private List<UIDocument> documents = new();

    [Header("Behavior")]
    [SerializeField] private HideMode hideMode = HideMode.DisplayNone;

    [Tooltip("Refresh every frame (useful if sorting orders change frequently). Otherwise call Refresh() manually.")]
    [SerializeField] private bool refreshEveryFrame = false;

    // Cache: reflection check for UIDocument.sortingOrder if present on this Unity version.
    private static PropertyInfo _uidocSortingProp;

    private void Awake()
    {
        if (autoDiscoverInScene)
        {
            Discover();
        }

        // Probe once for UIDocument.sortingOrder (available in newer Unity)
        _uidocSortingProp = typeof(UIDocument).GetProperty("sortingOrder",
            BindingFlags.Public | BindingFlags.Instance);
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
        var found = FindObjectsOfType<UIDocument>(includeInactive).ToList();
        documents = found;
    }

    /// <summary>
    /// Re-evaluate all documents, show the one with the lowest effective order, hide the rest.
    /// </summary>
    public void Refresh()
    {
        // Filter to valid documents; if you only want enabled ones to compete, keep isActiveAndEnabled
        var candidates = documents
            .Where(d => d != null && d.gameObject.scene.IsValid())
            .ToList();

        if (candidates.Count == 0)
            return;

        // Choose the one with the smallest order; tie-breaker = index in 'documents' list
        var ordered = candidates
            .Select((doc, idx) => new { Doc = doc, Index = idx, Order = GetEffectiveOrder(doc) })
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Index)
            .ToList();

        var winner = ordered.First().Doc;

        foreach (var entry in ordered)
        {
            SetVisible(entry.Doc, entry.Doc == winner);
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
        if (doc.panelSettings != null)
        {
            return (int)doc.panelSettings.sortingOrder;
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