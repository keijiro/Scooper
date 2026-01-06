using UnityEngine;
using UnityEngine.UIElements;

public sealed class Scoreboard : MonoBehaviour
{
    #region Editable Fields

    [Space]
    [SerializeField] ParticleSystem _coinEmitter = null;
    [SerializeField] ParticleSystem _heartEmitter = null;
    [SerializeField] int _scoreSpeed = 500;
    [SerializeField] int _animationDelay = 1;

    #endregion

    #region Private Members

    UIDocument _ui;
    Label _scoreText;

    (int current, int display) _score;
    float _delayTimer;

    #endregion

    #region Public Methods

    public void Award(int amount)
    {
        _heartEmitter.Play();
        _coinEmitter.Emit(amount);
        _score.current += amount;
    }

    public void Tip()
    {
        _coinEmitter.Emit(1);
        _score.current++;
    }

    public void Penalize(int amount)
      => _score.current -= amount;

    #endregion

    #region MonoBehaviour Methods

    void Start()
    {
        _ui = GetComponent<UIDocument>();
        _scoreText = _ui.rootVisualElement.Q<Label>("score-text");
        _score.current = 100;
    }

    void Update()
    {
        if (_score.display == _score.current)
        {
            if (_delayTimer > 0)
            {
                _delayTimer = Mathf.Max(0, _delayTimer - Time.deltaTime);
                if (_delayTimer == 0)
                    _scoreText.RemoveFromClassList("scoreboard__text-hilight");
            }
            return;
        }

        if (_delayTimer == 0)
        {
            _scoreText.AddToClassList("scoreboard__text-hilight");
            _delayTimer = _animationDelay;
        }

        if (_score.display < _score.current)
        {
            _score.display += (int)(_scoreSpeed * Time.deltaTime);
            _score.display = Mathf.Min(_score.display, _score.current);
        }
        else if (_score.display > _score.current)
        {
            _score.display -= (int)(_scoreSpeed * Time.deltaTime);
            _score.display = Mathf.Max(_score.display, _score.current);
        }

        _scoreText.text = $"{_score.display:N0}";
    }

    #endregion
}
