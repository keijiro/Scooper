using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
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
    [SerializeField] ParticleSystem[] _explosionFx = null;
    [SerializeField] CameraShake _cameraShake = null;
    [SerializeField] float _explosionRadius = 1.5f;
    [SerializeField] float _explosionFalloff = 2f;
    [SerializeField] float _explosionImpulse = 8f;
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
                   GameState.DetonatedBomb == null)
                await Awaitable.FixedUpdateAsync();

            if (GameState.DetonatedBomb != null)
                FlushContentsAsync().Forget();

            var success = _itemDetector.DetectedItem != null &&
                          _itemDetector.DetectedItem.Type == _tray.TargetItemType;

            if (GameState.DetonatedBomb != null)
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
                foreach (var fx in _explosionFx)
                {
                    fx.transform.position = pos;
                    fx.Play();
                }
                _cameraShake.Shake();
            }

            await Awaitable.WaitForSecondsAsync(0.15f);

            if (GameState.DetonatedBomb != null)
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
            GameState.DetonatedBomb = null;

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
