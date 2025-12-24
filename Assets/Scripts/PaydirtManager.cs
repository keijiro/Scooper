using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class PaydirtManager : MonoBehaviour, IStageInitializable
{
    [field:SerializeField] public float DirtRadius { get; set; } = 0.2f;
    [field:SerializeField] public float DirtDensity { get; set; } = 1f;
    [field:SerializeField] public float PourRate { get; set; } = 200;
    [field:SerializeField] public int TargetBodyCount { get; set; } = 900;
    [field:SerializeField] public Vector2 SpoutCenter { get; set; } = new(0, 12);
    [field:SerializeField] public Vector2 SpoutSize { get; set; } = new(13, 1);
    [field:SerializeField] public float RecycleY { get; set; } = -10;

    CircleGeometry _dirtGeometry;
    readonly List<PhysicsBody> _dirtBodies = new();
    PhysicsBodyDefinition _bodyDefinition;
    PhysicsShapeDefinition _shapeDefinition;
    float _spawnAccumulator;

    public void InitializeStage(StageManager stage)
    {
        _bodyDefinition = PhysicsBodyDefinition.defaultDefinition;
        _bodyDefinition.type = PhysicsBody.BodyType.Dynamic;

        _shapeDefinition = PhysicsShapeDefinition.defaultDefinition;
        _shapeDefinition.density = DirtDensity;
        _dirtGeometry = new CircleGeometry { radius = DirtRadius };
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
            var body = CreateDirtBody(GetSpoutPosition());
            _dirtBodies.Add(body);
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

    PhysicsBody CreateDirtBody(Vector2 position)
    {
        _bodyDefinition.position = position;
        var body = PhysicsWorld.defaultWorld.CreateBody(_bodyDefinition);
        body.CreateShape(_dirtGeometry, _shapeDefinition);
        return body;
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
        var minX = SpoutCenter.x - halfSize.x + DirtRadius;
        var maxX = SpoutCenter.x + halfSize.x - DirtRadius;
        if (minX > maxX)
            minX = maxX = SpoutCenter.x;
        var x = Random.Range(minX, maxX);

        var minY = SpoutCenter.y - halfSize.y + DirtRadius;
        var maxY = SpoutCenter.y + halfSize.y - DirtRadius;
        if (minY > maxY)
            minY = maxY = SpoutCenter.y;
        var y = Random.Range(minY, maxY);

        return new Vector2(x, y);
    }
}
