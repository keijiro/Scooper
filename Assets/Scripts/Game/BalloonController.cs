using UnityEngine;
using UnityEngine.UIElements;

public sealed class BalloonController : MonoBehaviour
{
    #region Editable Fields

    [SerializeField] string _defaultMessage = "ONE MORE\nLIKE THIS\nPLEASE!";
    [SerializeField] string[] _goodMessages = null;
    [SerializeField] string[] _badMessages = null;

    #endregion

    #region Private Members

    VisualElement _balloon;
    Label _label;
    bool _blink;
    float _time;

    void StartMessage(string text, bool blink = false)
    {
        _label.text = text;
        _balloon.style.display = DisplayStyle.Flex;
        _blink = blink;
        _time = 0;
    }

    #endregion

    #region Public Methods

    public void ShowDefaultMessage()
      => StartMessage(_defaultMessage, true);

    public void ShowGoodMessage()
      => StartMessage(_goodMessages[Random.Range(0, _goodMessages.Length)]);

    public void ShowBadMessage()
      => StartMessage(_badMessages[Random.Range(0, _badMessages.Length)]);

    public void HideMessage()
    {
        _balloon.style.display = DisplayStyle.None;
        _blink = false;
        _time = 1000;
    }

    #endregion

    #region MonoBehaviour Implementation

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _balloon = root.Q("balloon");
        _label = root.Q<Label>("balloon-text");
        _time = 1000;
    }

    void Update()
    {
        _time += Time.deltaTime;
        var show = _blink ? (_time % 1.5f) < 1 : _time < 1.5f;
        _balloon.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    #endregion
}
