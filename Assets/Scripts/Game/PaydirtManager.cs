using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class PaydirtManager : MonoBehaviour, IStageInitializable
{
    [SerializeField] float _radius = 0.2f;
    [SerializeField] float _density = 1;
    [field:SerializeField] public float PourRate { get; set; } = 512;
    [field:SerializeField] public int TargetBodyCount { get; set; } = 1024;
    [SerializeField] SpoutPositionProvider _spout = null;
    [field:SerializeField] public float RecycleY { get; set; } = -10;

    readonly List<PhysicsBody> _poolBodies = new();
    readonly List<PhysicsBody> _activeBodies = new();
    readonly Queue<PhysicsBody> _pendingBodies = new();
    PhysicsBodyDefinition _bodyDefinition;
    float _spawnAccumulator;

    public void InitializeStage(StageManager stage)
    {
        EnsureSpout();
        _bodyDefinition = PhysicsBodyDefinition.defaultDefinition;
        _bodyDefinition.type = PhysicsBody.BodyType.Dynamic;
        CreatePool();
        RequestInjection();
    }

    void Awake()
      => EnsureSpout();

    void EnsureSpout()
    {
        if (_spout == null)
            _spout = GetComponent<SpoutPositionProvider>();
    }

    void OnDestroy()
    {
        DestroyBodies(_poolBodies);
        DestroyBodies(_activeBodies);
        DestroyBodies(_pendingBodies);

        _poolBodies.Clear();
        _pendingBodies.Clear();
        _activeBodies.Clear();
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

    PhysicsBody CreateDirtBody(Vector2 position)
    {
        _bodyDefinition.position = position;
        var body = PhysicsWorld.defaultWorld.CreateBody(_bodyDefinition);

        var shapeDefinition = PhysicsShapeDefinition.defaultDefinition;
        shapeDefinition.density = _density;
        shapeDefinition.triggerEvents = false;
        shapeDefinition.contactFilter = GetContactFilter();

        var geometry = new CircleGeometry { radius = _radius };
        body.CreateShape(geometry, shapeDefinition);

        return body;
    }

    PhysicsShape.ContactFilter GetContactFilter()
    {
        var contacts = PhysicsMask.All;
        var categories = new PhysicsMask((int)Categories.Dirt);
        return new PhysicsShape.ContactFilter(categories, contacts);
    }

    void ActivateBody(PhysicsBody body)
    {
        body.enabled = true;
        body.SetAndWriteTransform(new PhysicsTransform(_spout.GetPosition()));
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

    void CreatePool()
    {
        _poolBodies.Clear();
        _pendingBodies.Clear();
        _activeBodies.Clear();
        _spawnAccumulator = 0f;

        for (var i = 0; i < TargetBodyCount; ++i)
        {
            var body = CreateDirtBody(_spout.GetPosition());
            body.enabled = false;

            _poolBodies.Add(body);
        }
    }

    void DestroyBodies(IEnumerable<PhysicsBody> bodies)
    {
        foreach (var body in bodies)
        {
            if (body.isValid)
                body.Destroy();
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
