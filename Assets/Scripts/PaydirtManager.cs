using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class PaydirtManager : MonoBehaviour, IStageInitializable
{
    [SerializeField] DirtBodyDefinition _dirtDefinition = null;
    [SerializeField] DirtBodyDefinition _bombDefinition = null;
    [SerializeField] DirtBodyDefinition[] _gemDefinitions = null;
    [field:SerializeField] public float PourRate { get; set; } = 200;
    [field:SerializeField] public int TargetBodyCount { get; set; } = 900;
    [field:SerializeField] public int BombsPerDirt { get; set; } = 30;
    [field:SerializeField] public int GemsPerDirt { get; set; } = 20;
    [field:SerializeField] public Vector2 SpoutCenter { get; set; } = new(0, 12);
    [field:SerializeField] public Vector2 SpoutSize { get; set; } = new(13, 1);
    [field:SerializeField] public float RecycleY { get; set; } = -10;

    public IReadOnlyList<PhysicsBody> BombBodies => _bombBodies;
    public IReadOnlyList<PhysicsBody> GemBodies => _gemBodies;

    readonly List<PhysicsBody> _allBodies = new();
    readonly List<PhysicsBody> _poolBodies = new();
    readonly List<PhysicsBody> _activeBodies = new();
    readonly List<PhysicsBody> _bombBodies = new();
    readonly List<PhysicsBody> _gemBodies = new();
    readonly Queue<PhysicsBody> _pendingBodies = new();
    PhysicsBodyDefinition _bodyDefinition;
    float _spawnAccumulator;
    int _dirtSinceBomb;
    int _dirtSinceGem;
    int _gemIndex;

    enum PaydirtKind
    {
        Dirt,
        Bomb,
        Gem
    }

    public void InitializeStage(StageManager stage)
    {
        _bodyDefinition = PhysicsBodyDefinition.defaultDefinition;
        _bodyDefinition.type = PhysicsBody.BodyType.Dynamic;
        CreatePool();
        RequestInjection();
    }

    void OnDestroy()
    {
        for (var i = 0; i < _allBodies.Count; ++i)
        {
            var body = _allBodies[i];
            if (body.isValid)
                body.Destroy();
        }

        _allBodies.Clear();
        _poolBodies.Clear();
        _pendingBodies.Clear();
        _activeBodies.Clear();
        _bombBodies.Clear();
        _gemBodies.Clear();
    }

    void Update()
    {
        SpawnDirtBodies();
        RecycleDirtBodies();
    }

    void SpawnDirtBodies()
    {
        if (_pendingBodies.Count == 0)
            return;

        _spawnAccumulator += PourRate * Time.deltaTime;

        while (_spawnAccumulator >= 1f && _pendingBodies.Count > 0)
        {
            _spawnAccumulator -= 1f;
            var body = _pendingBodies.Dequeue();
            ActivateBody(body);
            _activeBodies.Add(body);
        }
    }

    void RecycleDirtBodies()
    {
        for (var i = _activeBodies.Count - 1; i >= 0; --i)
        {
            var body = _activeBodies[i];
            if (!body.isValid)
                continue;

            if (body.transform.position.y >= RecycleY)
                continue;

            DeactivateBody(body);
            _activeBodies.RemoveAt(i);
        }
    }

    PhysicsBody CreateDirtBody(Vector2 position, DirtBodyDefinition definition, PaydirtKind kind)
      => definition.CreateBody(PhysicsWorld.defaultWorld, _bodyDefinition, position, GetContactFilter(kind));

    DirtBodyDefinition ChooseDefinition(out PaydirtKind kind)
    {
        kind = PaydirtKind.Dirt;

        if (BombsPerDirt > 0 && _dirtSinceBomb >= BombsPerDirt)
        {
            _dirtSinceBomb = 0;
            kind = PaydirtKind.Bomb;
            return _bombDefinition;
        }

        if (GemsPerDirt > 0 && _gemDefinitions.Length > 0 && _dirtSinceGem >= GemsPerDirt)
        {
            _dirtSinceGem = 0;
            kind = PaydirtKind.Gem;
            var definition = _gemDefinitions[_gemIndex];
            _gemIndex = (_gemIndex + 1) % _gemDefinitions.Length;
            return definition;
        }

        _dirtSinceBomb++;
        _dirtSinceGem++;
        return _dirtDefinition;
    }

    PhysicsShape.ContactFilter GetContactFilter(PaydirtKind kind)
    {
        var contacts = PhysicsMask.All;
        var categories = new PhysicsMask(Categories.Dirt);

        if (kind == PaydirtKind.Bomb)
            categories = new PhysicsMask(Categories.Bomb);
        else if (kind == PaydirtKind.Gem)
            categories = new PhysicsMask(Categories.Gem);

        if (kind == PaydirtKind.Dirt)
            contacts.ResetBit(Categories.Tray);

        return new PhysicsShape.ContactFilter(categories, contacts, 0);
    }

    void ActivateBody(PhysicsBody body)
    {
        body.enabled = true;
        body.SetAndWriteTransform(new PhysicsTransform(GetSpoutPosition()));
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    void DeactivateBody(PhysicsBody body)
    {
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.enabled = false;
        _poolBodies.Add(body);
    }

    Vector2 GetSpoutPosition()
    {
        var halfSize = new Vector2(Mathf.Max(0f, SpoutSize.x), Mathf.Max(0f, SpoutSize.y)) * 0.5f;
        var radius = Mathf.Max(_dirtDefinition.Radius, _bombDefinition.Radius);
        for (var i = 0; i < _gemDefinitions.Length; ++i)
            radius = Mathf.Max(radius, _gemDefinitions[i].Radius);
        var minX = SpoutCenter.x - halfSize.x + radius;
        var maxX = SpoutCenter.x + halfSize.x - radius;
        if (minX > maxX)
            minX = maxX = SpoutCenter.x;
        var x = Random.Range(minX, maxX);

        var minY = SpoutCenter.y - halfSize.y + radius;
        var maxY = SpoutCenter.y + halfSize.y - radius;
        if (minY > maxY)
            minY = maxY = SpoutCenter.y;
        var y = Random.Range(minY, maxY);

        return new Vector2(x, y);
    }

    void CreatePool()
    {
        _allBodies.Clear();
        _poolBodies.Clear();
        _pendingBodies.Clear();
        _activeBodies.Clear();
        _bombBodies.Clear();
        _gemBodies.Clear();
        _spawnAccumulator = 0f;
        _dirtSinceBomb = 0;
        _dirtSinceGem = 0;
        _gemIndex = 0;

        for (var i = 0; i < TargetBodyCount; ++i)
        {
            var definition = ChooseDefinition(out var kind);
            var body = CreateDirtBody(GetSpoutPosition(), definition, kind);
            body.enabled = false;

            _allBodies.Add(body);
            _poolBodies.Add(body);
            if (kind == PaydirtKind.Bomb)
                _bombBodies.Add(body);
            else if (kind == PaydirtKind.Gem)
                _gemBodies.Add(body);
        }
    }

    public void RequestInjection()
    {
        for (var i = 0; i < _poolBodies.Count; ++i)
            _pendingBodies.Enqueue(_poolBodies[i]);

        _poolBodies.Clear();
        ConsoleManager.AddLine("Paydirt injection queued.");
    }
}
