using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    #region Editable Fields

    [Space]
    [SerializeField] SpoutPositionProvider _spout = null;
    [Space]
    [SerializeField] ItemPrefabSet _itemPrefabs = null;

    #endregion

    #region Public Methods

    public async Awaitable StartSpawnBombs(int count, float duration)
      => await StartSpawn(count, duration, isBomb: true);

    public async Awaitable StartSpawnGems(int count, float duration)
      => await StartSpawn(count, duration, isBomb: false);

    #endregion

    #region Spawn Implementation

    async Awaitable StartSpawn(int count, float duration, bool isBomb)
    {
        var step = duration / count;

        for (var i = 0; i < count; ++i)
        {
            var rand = Random.value;
            await Awaitable.WaitForSecondsAsync(step * rand);

            var itemType = isBomb ? ItemType.Bomb : GameState.GetNextGemType();
            var prefab = _itemPrefabs.GetItemPrefab(itemType);
            var go = Instantiate(prefab, _spout.GetPosition(), Quaternion.identity);
            go.AddComponent<ItemController>().Type = itemType;

            await Awaitable.WaitForSecondsAsync(step * (1 - rand));
        }
    }

    #endregion
}
