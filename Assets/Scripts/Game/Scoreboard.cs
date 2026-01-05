using UnityEngine;

public sealed class Scoreboard : MonoBehaviour
{
    #region Editable Fields

    [Space]
    [SerializeField] ParticleSystem _coinEmitter = null;
    [SerializeField] ParticleSystem _heartEmitter = null;

    #endregion

    #region Public Methods

    public void Award()
    {
        _coinEmitter.Emit(20);
        _heartEmitter.Play();
    }

    public void Tip()
    {
        _coinEmitter.Emit(1);
    }

    #endregion
}
