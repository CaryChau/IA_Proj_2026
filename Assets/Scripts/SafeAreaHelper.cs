using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class SafeAreaHelper : MonoBehaviour
{
    private RectTransform m_RectTransform;
    private Rect m_LastSafeArea;

    private void Awake()
        => m_RectTransform = transform as RectTransform;

    private void Start()
        => AdjustAnchors();

    private void Update()
    {
        if (Screen.safeArea == m_LastSafeArea)
            return;
        
        AdjustAnchors();
    }

    private void AdjustAnchors()
    {
        var safeArea = Screen.safeArea;

        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        m_RectTransform.anchorMin = anchorMin;
        m_RectTransform.anchorMax = anchorMax;
        m_LastSafeArea = Screen.safeArea;
    }
}
