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
    [SerializeField] Animation _bucketAnimation = null;
    [Space]
    [SerializeField] Scoreboard _scoreboard = null;
    [SerializeField] BalloonController _balloonController = null;
    [SerializeField] ExplosionEffect _explosionEffect = null;
    [Space]
    [SerializeField] TrayController _trayPrefab = null;

    #endregion

    #region Private Fields

    Button _reloadButton;
    TrayController _tray;

    #endregion

    #region Game Action Methods (async)

    // Initialization
    async Awaitable InitializeStageAsync()
    {
        await Awaitable.WaitForSecondsAsync(0.1f);

        InjectContentsAsync().Forget();

        await Awaitable.WaitForSecondsAsync(1);

        await RunMainGameLoopAsync();
    }

    // Content injection
    async Awaitable InjectContentsAsync()
    {
        _dirtManager.RequestInjection();
        _itemSpawner.StartSpawnBombs(2, 2).Forget();
        _itemSpawner.StartSpawnGems(8, 2).Forget();

        await Awaitable.WaitForSecondsAsync(2.5f);

        _scoopController.SpawnScoopInstance();
    }

    // Reloading action
    async Awaitable ReloadContentsAsync()
    {
        _reloadButton.SetEnabled(false);

        _bucketAnimation.Play("HatchOpen");
        _scoopController.ThrowScoopInstance();

        await Awaitable.WaitForSecondsAsync(3);

        InjectContentsAsync().Forget();
        _bucketAnimation.Play("HatchClose");

        await Awaitable.WaitForSecondsAsync(3);

        _reloadButton.SetEnabled(true);
    }

    // Main game loop
    async Awaitable RunMainGameLoopAsync()
    {
        while (true)
        {
            // Spawn new tray.
            _tray = Instantiate(_trayPrefab);

            // Default message balloon
            await Awaitable.WaitForSecondsAsync(1);
            _balloonController.ShowDefaultMessage();

            // Wait until:
            // - item was detected in tray, or
            // - bomb was detonated
            while (GameState.DetectedItem == null &&
                   GameState.DetonatedBomb == null)
                await Awaitable.FixedUpdateAsync();

            // Bomb was detonated?
            var detonated = GameState.DetonatedBomb != null;

            // Right item was placed in tray?
            var success = GameState.DetectedItem != null && _tray != null &&
                          GameState.DetectedItem?.Type == _tray.TargetItemType;

            if (detonated)
            {
                // Trigger explosion effect.
                _explosionEffect.Explode(GameState.DetonatedBomb);

                // Start reloading if not already reloading.
                if (_reloadButton.enabledSelf) ReloadContentsAsync().Forget();
            }

            // Short base delay
            await Awaitable.WaitForSecondsAsync(0.15f);

            if (detonated)
            {
                // Start tray exit animation and hide balloon message.
                _tray.StartExit();
                _balloonController.HideMessage();

                // Start penalizing score after delay.
                await Awaitable.WaitForSecondsAsync(1.5f);
                _scoreboard.Penalize(20);
            }
            else
            {
                if (success)
                {
                    // Success: Award and good message
                    _scoreboard.Award(20);
                    _balloonController.ShowGoodMessage();
                }
                else
                {
                    // Failure: Tip and bad message
                    _scoreboard.Tip();
                    _balloonController.ShowBadMessage();
                }

                // Start tray exit animation after short delay
                await Awaitable.WaitForSecondsAsync(0.5f);
                _tray.StartExit();

                // Wait for exit animation to complete
                await Awaitable.WaitForSecondsAsync(1);
            }

            // Clean up for next turn.
            Destroy(_tray.gameObject);

            if (GameState.DetectedItem != null)
                Destroy(GameState.DetectedItem.gameObject);

            (GameState.DetectedItem, GameState.DetonatedBomb) = (null, null);

            // Short delay before looping
            await Awaitable.WaitForSecondsAsync(1);
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
