using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
public static class PageAnimator
{

    public static void FadeTo(VisualElement ve, float from, float to, int durationMs, System.Action onDone = null)
    {
        ve.style.opacity = from;

        var anim = ValueAnimation<float>.Create(
            ve,
            (start, end, t) => Mathf.Lerp(start, end, t)
        );

        anim.from = from;
        anim.to = to;
        anim.durationMs = durationMs;
        anim.easingCurve = Easing.InOutQuad;
        anim.valueUpdated = (_, v) => ve.style.opacity = v;
        anim.onAnimationCompleted = () => onDone?.Invoke();
        anim.autoRecycle = true;
        anim.Start();
    }

}
