using UnityEngine;
using UnityEngine.UIElements;

public sealed class StageManager : MonoBehaviour
{
    #region Editable Fields

    [Space]
    [SerializeField] UIDocument _ui = null;
    [SerializeField] DirtManager _dirtManager = null;
    [SerializeField] ItemSpawner _itemSpawner = null;
    [SerializeField] ScoopController _scoopController = null;
    [SerializeField] BalloonController _balloonController = null;
    [SerializeField] Animation _bucketAnimation = null;
    [SerializeField] Scoreboard _scoreboard = null;
    [SerializeField] ExplosionEffect _explosionEffect = null;
    [Space]
    [SerializeField] TrayController _trayPrefab = null;

    #endregion

    #region Private Fields

    Button _reloadButton;
    TrayController _tray;

    #endregion

    #region Game Action Methods (async)

    async Awaitable InitializeStageAsync()
    {
        await Awaitable.WaitForSecondsAsync(0.1f);

        InjectContentsAsync().Forget();

        await Awaitable.WaitForSecondsAsync(1);

        _tray = Instantiate(_trayPrefab);

        await RunItemDetectionLoopAsync();
    }

    async Awaitable InjectContentsAsync()
    {
        _dirtManager.RequestInjection();
        _itemSpawner.StartSpawnBombs(2, 2).Forget();
        _itemSpawner.StartSpawnGems(8, 2).Forget();

        await Awaitable.WaitForSecondsAsync(2);

        _scoopController.SpawnScoopInstance();
    }

    async Awaitable ReloadContentsAsync()
    {
        _reloadButton.SetEnabled(false);

        _bucketAnimation.Play("HatchOpen");
        _scoopController.ThrowScoopInstance();

        await Awaitable.WaitForSecondsAsync(2);

        InjectContentsAsync().Forget();
        _bucketAnimation.Play("HatchClose");

        await Awaitable.WaitForSecondsAsync(3);

        _reloadButton.SetEnabled(true);
    }

    async Awaitable RunItemDetectionLoopAsync()
    {
        while (true)
        {
            await Awaitable.WaitForSecondsAsync(1);
            _balloonController.ShowDefaultMessage();

            while (GameState.DetectedItem == null &&
                   GameState.DetonatedBomb == null)
                await Awaitable.FixedUpdateAsync();

            var detonated = GameState.DetonatedBomb != null;

            if (detonated) ReloadContentsAsync().Forget();

            var success = GameState.DetectedItem != null &&
                          GameState.DetectedItem.Type == _tray.TargetItemType;

            if (detonated)
                _explosionEffect.Explode(GameState.DetonatedBomb);

            await Awaitable.WaitForSecondsAsync(0.15f);

            if (detonated)
            {
                _balloonController.HideMessage();
                _tray.StartExit();
                await Awaitable.WaitForSecondsAsync(1.5f);
                _scoreboard.Penalize(20);
            }
            else
            {
                if (success)
                {
                    _balloonController.ShowGoodMessage();
                    _scoreboard.Award(20);
                }
                else
                {
                    _balloonController.ShowBadMessage();
                    _scoreboard.Tip();
                }

                await Awaitable.WaitForSecondsAsync(0.5f);
                _tray.StartExit();
                await Awaitable.WaitForSecondsAsync(1);
            }

            Destroy(_tray.gameObject);

            await Awaitable.WaitForSecondsAsync(1);

            var item = GameState.DetectedItem;
            if (item != null) Destroy(item.gameObject);

            (GameState.DetectedItem, GameState.DetonatedBomb) = (null, null);

            _tray = Instantiate(_trayPrefab);
        }
    }

    #endregion

    #region UI Controllers

    void OnReloadClicked()
    {
        _scoreboard.Penalize(4);
        ReloadContentsAsync().Forget();
    }

    #endregion

    #region MonoBehaviour Implementation

    void Start()
    {
        var root = _ui.rootVisualElement;
        _reloadButton = root.Q<Button>("reload-button");
        _reloadButton.clicked += OnReloadClicked;

        InitializeStageAsync().Forget();
    }

    #endregion
}
