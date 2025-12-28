using UnityEngine;
using UnityEngine.UIElements;

public class StageManager : MonoBehaviour
{
    [SerializeField] UIDocument _ui = null;
    [SerializeField] PaydirtManager _paydirtManager = null;
    [SerializeField] ScoopController _scoopController = null;
    [Space]
    [SerializeField] Animation _bucketAnimation = null;
    [SerializeField] float _bucketCloseWait = 2;

    Button _flushButton;

    async Awaitable StartInjection()
    {
        _paydirtManager.RequestInjection();

        var spawner = GetComponent<ItemSpawner>();
        spawner.StartSpawnBombs(2, 2).Forget();
        spawner.StartSpawnGems(0, 3, 2).Forget();
        spawner.StartSpawnGems(1, 3, 2).Forget();

        await Awaitable.WaitForSecondsAsync(2);

        _scoopController.SpawnScoopInstance();
    }

    async void Start()
    {
        var root = _ui.rootVisualElement;
        _flushButton = root.Q<Button>("flush-button");
        _flushButton.clicked += OnFlushClicked;

        await Awaitable.WaitForSecondsAsync(0.1f);

        StartInjection().Forget();
    }

    async void OnFlushClicked()
    {
        _bucketAnimation.Play("HatchOpen");
        _scoopController.ThrowScoopInstance();

        await Awaitable.WaitForSecondsAsync(_bucketCloseWait);

        StartInjection().Forget();
        _bucketAnimation.Play("HatchClose");
    }
}
