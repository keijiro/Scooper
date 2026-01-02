using UnityEngine;
using UnityEngine.UIElements;

public class StageManager : MonoBehaviour
{
    #region Editable Fields

    [Space]
    [SerializeField] UIDocument _ui = null;
    [SerializeField] PaydirtManager _paydirtManager = null;
    [SerializeField] ScoopController _scoopController = null;
    [SerializeField] BalloonController _balloonController = null;
    [SerializeField] Animation _bucketAnimation = null;
    [SerializeField] ParticleSystem _coinFountain = null;
    [Space]
    [SerializeField] TrayController _trayPrefab = null;

    #endregion

    #region Private Fields

    ItemDetector _itemDetector;
    ItemSpawner _itemSpawner;
    Button _flushButton;
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
        _paydirtManager.RequestInjection();
        _itemSpawner.StartSpawnBombs(2, 2).Forget();
        _itemSpawner.StartSpawnGems(6, 2).Forget();

        await Awaitable.WaitForSecondsAsync(2);

        _scoopController.SpawnScoopInstance();
    }

    async Awaitable FlushContentsAsync()
    {
        _flushButton.SetEnabled(false);

        _bucketAnimation.Play("HatchOpen");
        _scoopController.ThrowScoopInstance();

        await Awaitable.WaitForSecondsAsync(2);

        InjectContentsAsync().Forget();
        _bucketAnimation.Play("HatchClose");

        await Awaitable.WaitForSecondsAsync(3);

        _flushButton.SetEnabled(true);
    }

    async Awaitable RunItemDetectionLoopAsync()
    {
        while (true)
        {
            await Awaitable.WaitForSecondsAsync(1);
            _balloonController.ShowDefaultMessage();

            while (_itemDetector.DetectedItem == null &&
                   !GameState.IsBombDetonated)
                await Awaitable.FixedUpdateAsync();

            if (GameState.IsBombDetonated)
                FlushContentsAsync().Forget();

            var success = _itemDetector.DetectedItem != null &&
                          _itemDetector.DetectedItem.Type == _tray.TargetItemType;

            await Awaitable.WaitForSecondsAsync(0.15f);

            if (GameState.IsBombDetonated)
            {
                _balloonController.HideMessage();
                _tray.StartExit();
                await Awaitable.WaitForSecondsAsync(1.5f);
            }
            else
            {
                if (success)
                {
                    _balloonController.ShowGoodMessage();
                    _coinFountain.Emit(20);
                }
                else
                {
                    _balloonController.ShowBadMessage();
                    _coinFountain.Emit(1);
                }

                await Awaitable.WaitForSecondsAsync(0.5f);
                _tray.StartExit();
                await Awaitable.WaitForSecondsAsync(1);
            }

            Destroy(_tray.gameObject);

            await Awaitable.WaitForSecondsAsync(1);

            var item = _itemDetector.DetectedItem;
            if (item != null) Destroy(item.gameObject);

            _itemDetector.ResetDetection();
            GameState.IsBombDetonated = false;

            _tray = Instantiate(_trayPrefab);
        }
    }

    #endregion

    #region UI Controllers

    void OnFlushClicked()
      => FlushContentsAsync().Forget();

    #endregion

    #region MonoBehaviour Implementation

    void Start()
    {
        _itemDetector = GetComponent<ItemDetector>();
        _itemSpawner = GetComponent<ItemSpawner>();

        var root = _ui.rootVisualElement;
        _flushButton = root.Q<Button>("flush-button");
        _flushButton.clicked += OnFlushClicked;

        InitializeStageAsync().Forget();
    }

    #endregion
}
