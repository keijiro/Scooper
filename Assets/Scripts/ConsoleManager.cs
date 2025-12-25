using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class ConsoleManager : MonoBehaviour
{
    static ConsoleManager _instance;

    [SerializeField] UIDocument _document = null;
    [field:SerializeField] public int MaxLines { get; set; } = 5;

    readonly List<string> _lines = new();
    TextElement _textElement;

    void OnEnable()
    {
        _instance = this;

        _textElement = _document.rootVisualElement.Q<TextElement>("console-text");
        RefreshText();
    }

    void OnDisable()
    {
        if (_instance == this)
            _instance = null;
    }

    public static void AddLine(string message)
      => _instance?.AppendLine(message);

    void AppendLine(string message)
    {
        _lines.Add(message);
        var max = Mathf.Max(1, MaxLines);
        if (_lines.Count > max)
            _lines.RemoveRange(0, _lines.Count - max);

        RefreshText();
    }

    void RefreshText()
    {
        var builder = new StringBuilder();
        for (var i = 0; i < _lines.Count; ++i)
        {
            if (i > 0)
                builder.Append('\n');
            builder.Append(_lines[i]);
        }

        _textElement.text = builder.ToString();
    }
}
