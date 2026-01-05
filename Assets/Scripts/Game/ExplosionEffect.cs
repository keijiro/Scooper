using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public sealed class ExplosionEffect : MonoBehaviour
{
    #region Editable Fields

    [Space]
    [SerializeField] float _explosionRadius = 6;
    [SerializeField] float _explosionFalloff = 3;
    [SerializeField] float _explosionImpulse = 4;
    [Space]
    [SerializeField] float _shakeMagnitude = 1;
    [SerializeField] float _shakeDuration = 0.75f;
    [SerializeField] float _shakeExponent = 5;
    [Space]
    [SerializeField] Transform _shakeTarget = null;
    [Space]
    [SerializeField] ParticleSystem[] _emitters = null;

    #endregion

    #region Public Methods

    public void Explode(ItemController bomb)
    {
        var pos = GameState.DetonatedBomb.transform.position;

        var explodeDef = new PhysicsWorld.ExplosionDefinition
        {
            position = pos,
            radius = _explosionRadius,
            falloff = _explosionFalloff,
            impulsePerLength = _explosionImpulse,
            hitCategories = PhysicsMask.All
        };

        PhysicsWorld.defaultWorld.Explode(explodeDef);

        foreach (var fx in _emitters)
        {
            fx.transform.position = pos;
            fx.Play();
        }

        _time = 0;

        Destroy(bomb.gameObject);
    }

    #endregion

    #region Private Members

    float _time = 1000;

    void UpdateShake()
    {
        var t = Mathf.Clamp01(1 - _time / _shakeDuration);
        var a = Mathf.Pow(t, _shakeExponent) * _shakeMagnitude;

        var x = Random.Range(-a, a);
        var y = Random.Range(-a, a);

        _shakeTarget.localPosition = new Vector3(x, y, 0);
    }

    #endregion

    #region MonoBehaviour Implementation

    void Update()
    {
        UpdateShake();
        _time += Time.deltaTime;
    }

    #endregion
}
