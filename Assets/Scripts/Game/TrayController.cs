using UnityEngine;

public sealed class TrayController : MonoBehaviour
{
    #region Editable Fields

    [SerializeField] Vector2 _inPoint = Vector2.zero;
    [SerializeField] Vector2 _outPoint = Vector2.one;
    [SerializeField] float _moveDuration = 1;
    [SerializeField] ItemPrefabSet _itemPrefabs = null;
    [SerializeField] Transform _targetSpawnPoint = null;

    #endregion

    #region Public Methods

    public ItemType TargetItemType { get; private set; }

    public void StartExit() => _direction = -1;

    #endregion

    #region Motion

    float _parameter;
    float _direction = 1;

    void UpdatePosition()
    {
        var delta = _direction * Time.fixedDeltaTime / _moveDuration;
        _parameter = Mathf.Clamp01(_parameter + delta);

        var cosParam = Mathf.Cos(_parameter * Mathf.PI) * 0.5f + 0.5f;
        var position = Vector2.Lerp(_inPoint, _outPoint, cosParam);

        position += Vector2.up * Mathf.Max(0, Mathf.Sin(Time.time * 10)) * 0.3f;

        transform.position = position;
    }

    #endregion

    #region MonoBehaviour Implementation

    void Start()
    {
        TargetItemType = GameState.GetNextTrayItemType();
        var prefab = _itemPrefabs.GetItemPrefab(TargetItemType);
        Instantiate(prefab, _targetSpawnPoint.position, Quaternion.identity);
    }

    void FixedUpdate()
      => UpdatePosition();

    #endregion
}
