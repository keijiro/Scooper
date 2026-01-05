using UnityEngine;

public sealed class CameraShake : MonoBehaviour
{
    [SerializeField] float _magnitude = 1;
    [SerializeField] float _duration = 0.75f;
    [SerializeField] float _exponent = 5;

    float _time = 1000;

    public void Shake()
      => _time = 0;

    void Update()
    {
        var t = Mathf.Clamp01(1 - _time / _duration);
        var a = Mathf.Pow(t, _exponent) * _magnitude;

        var x = Random.Range(-a, a);
        var y = Random.Range(-a, a);
        var z = transform.localPosition.z;

        transform.localPosition = new Vector3(x, y, z);

        _time += Time.deltaTime;
    }
}
