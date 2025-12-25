using UnityEngine;
using UnityEngine.UIElements;

public class BucketController : MonoBehaviour
{
    [field:SerializeField] Bucket _bucket = null;
    [field:SerializeField] PaydirtManager _paydirtManager = null;
    [field:SerializeField] UIDocument _ui = null;
    [field:SerializeField] float _openAngle = 60f;
    [field:SerializeField] float _moveSpeed = 180f;
    [field:SerializeField] float _holdTime = 1f;

    Button _flushButton;
    float _targetAngle;
    float _currentAngle;
    float _holdTimer;

    enum FlushState
    {
        Closed,
        Opening,
        Holding,
        Closing
    }

    FlushState _state;

    void OnEnable()
    {
        var root = _ui.rootVisualElement;

        _flushButton = root.Q<Button>("flush-button");

        _flushButton.clicked += OnFlushClicked;

        _currentAngle = _bucket.BottomAngle;
        _targetAngle = _currentAngle;
        _state = Mathf.Approximately(_currentAngle, 0f) ? FlushState.Closed : FlushState.Closing;
    }

    void OnDisable()
    {
        _flushButton.clicked -= OnFlushClicked;
    }

    void Update()
    {
        if (_state == FlushState.Holding)
        {
            _holdTimer -= Time.deltaTime;
            if (_holdTimer <= 0f)
            {
                _state = FlushState.Closing;
                _targetAngle = 0f;
                _paydirtManager.RequestInjection();
            }
        }

        var nextAngle = Mathf.MoveTowards(_currentAngle, _targetAngle, _moveSpeed * Time.deltaTime);
        if (Mathf.Approximately(nextAngle, _currentAngle))
        {
            if (_state == FlushState.Opening)
            {
                _state = FlushState.Holding;
                _holdTimer = _holdTime;
            }
            else if (_state == FlushState.Closing)
                _state = FlushState.Closed;
            return;
        }

        _currentAngle = nextAngle;
        _bucket.BottomAngle = _currentAngle;
    }

    void OnFlushClicked()
    {
        _state = FlushState.Opening;
        _targetAngle = _openAngle;
        ConsoleManager.AddLine("Flush started.");
    }
}
