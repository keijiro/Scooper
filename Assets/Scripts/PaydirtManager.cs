using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class PaydirtManager : MonoBehaviour, IStageInitializable
{
    [field:SerializeField] public DirtBodyDefinition DirtDefinition { get; set; }
    [field:SerializeField] public DirtBodyDefinition BombDefinition { get; set; }
    [field:SerializeField] public DirtBodyDefinition[] GemDefinitions { get; set; }
    [field:SerializeField] public float PourRate { get; set; } = 200;
    [field:SerializeField] public int TargetBodyCount { get; set; } = 900;
    [field:SerializeField] public int BombsPerDirt { get; set; } = 30;
    [field:SerializeField] public int GemsPerDirt { get; set; } = 20;
    [field:SerializeField] public Vector2 SpoutCenter { get; set; } = new(0, 12);
    [field:SerializeField] public Vector2 SpoutSize { get; set; } = new(13, 1);
    [field:SerializeField] public float RecycleY { get; set; } = -10;

    public IReadOnlyList<PhysicsBody> BombBodies => _bombBodies;
    public IReadOnlyList<PhysicsBody> GemBodies => _gemBodies;

    readonly List<PhysicsBody> _dirtBodies = new();
    readonly List<PhysicsBody> _bombBodies = new();
    readonly List<PhysicsBody> _gemBodies = new();
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
    }

    void OnDestroy()
    {
        for (var i = 0; i < _dirtBodies.Count; ++i)
        {
            var body = _dirtBodies[i];
            if (body.isValid)
                body.Destroy();
        }

        _dirtBodies.Clear();
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
        if (_dirtBodies.Count >= TargetBodyCount)
            return;

        _spawnAccumulator += PourRate * Time.deltaTime;

        while (_spawnAccumulator >= 1f && _dirtBodies.Count < TargetBodyCount)
        {
            _spawnAccumulator -= 1f;
            var definition = ChooseDefinition(out var kind);
            var body = CreateDirtBody(GetSpoutPosition(), definition);
            _dirtBodies.Add(body);
            if (kind == PaydirtKind.Bomb)
                _bombBodies.Add(body);
            else if (kind == PaydirtKind.Gem)
                _gemBodies.Add(body);
        }
    }

    void RecycleDirtBodies()
    {
        for (var i = 0; i < _dirtBodies.Count; ++i)
        {
            var body = _dirtBodies[i];
            if (!body.isValid)
                continue;

            if (body.transform.position.y >= RecycleY)
                continue;

            ResetDirtBody(body);
        }
    }

    PhysicsBody CreateDirtBody(Vector2 position, DirtBodyDefinition definition)
      => definition.CreateBody(PhysicsWorld.defaultWorld, _bodyDefinition, position);

    DirtBodyDefinition ChooseDefinition(out PaydirtKind kind)
    {
        kind = PaydirtKind.Dirt;

        if (BombsPerDirt > 0 && _dirtSinceBomb >= BombsPerDirt)
        {
            _dirtSinceBomb = 0;
            kind = PaydirtKind.Bomb;
            return BombDefinition;
        }

        if (GemsPerDirt > 0 && GemDefinitions.Length > 0 && _dirtSinceGem >= GemsPerDirt)
        {
            _dirtSinceGem = 0;
            kind = PaydirtKind.Gem;
            var definition = GemDefinitions[_gemIndex];
            _gemIndex = (_gemIndex + 1) % GemDefinitions.Length;
            return definition;
        }

        _dirtSinceBomb++;
        _dirtSinceGem++;
        return DirtDefinition;
    }

    void ResetDirtBody(PhysicsBody body)
    {
        body.SetAndWriteTransform(new PhysicsTransform(GetSpoutPosition()));
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    Vector2 GetSpoutPosition()
    {
        var halfSize = new Vector2(Mathf.Max(0f, SpoutSize.x), Mathf.Max(0f, SpoutSize.y)) * 0.5f;
        var radius = Mathf.Max(DirtDefinition.Radius, BombDefinition.Radius);
        for (var i = 0; i < GemDefinitions.Length; ++i)
            radius = Mathf.Max(radius, GemDefinitions[i].Radius);
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
}
