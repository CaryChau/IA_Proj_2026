using System;
using UnityEngine;
using UnityEngine.UIElements;
public class Dragger : PointerManipulator
{
    #region Private variables
    int _pointerID;

    bool IsActive => _pointerID >= 0;

    #endregion
    private Action onPointerDownExt;
    private Action onPointerUpExt;
    #region PointerManipulator implementation

    public Dragger(Action down, Action up)
    {
        (_pointerID) = ( -1);
        onPointerDownExt = down;
        onPointerUpExt = up;
        activators.Add(new ManipulatorActivationFilter{button = MouseButton.LeftMouse});
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
    }

    #endregion

    #region Pointer callbacks

    void OnPointerDown(PointerDownEvent e)
    {
        if (IsActive)
        {
            e.StopImmediatePropagation();
        }
        else if (CanStartManipulation(e))
        {
            target.CapturePointer(_pointerID = e.pointerId);
            e.StopPropagation();
            onPointerDownExt?.Invoke();
        }
    }
    void OnPointerMove(PointerMoveEvent e)
    {
        if (!IsActive || !target.HasPointerCapture(_pointerID)) return;
        e.StopPropagation();
    }

    void OnPointerUp(PointerUpEvent e)
    {
        if (!IsActive || !target.HasPointerCapture(_pointerID)) return;

        if (CanStopManipulation(e))
        {
            _pointerID = -1;
            target.ReleaseMouse();
            e.StopPropagation();
            onPointerUpExt?.Invoke();
        }
    }

    #endregion
}