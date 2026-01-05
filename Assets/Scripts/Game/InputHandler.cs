using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public sealed class InputHandler : MonoBehaviour
{
    #region Public Properties

    public Vector2 Position => Pointer.current.position.value;
    public bool IsPressed { get; private set; }

    #endregion

    #region MonoBehaviour Implementation

    VisualElement _area;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _area = root.Q<VisualElement>("game-area");
        _area.RegisterCallback<PointerDownEvent>(OnPointerDown);
    }

    void OnDisable()
      => _area.UnregisterCallback<PointerDownEvent>(OnPointerDown);

    void LateUpdate()
      => IsPressed &= !Mouse.current.leftButton.wasReleasedThisFrame;

    #endregion

    #region UI Event Handlers

    void OnPointerDown(PointerDownEvent evt)
      => IsPressed = true;

    #endregion
}
