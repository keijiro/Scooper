using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class LowLevelBucketFill : MonoBehaviour
{
    [field: SerializeField] public Vector2 BucketSize { get; set; } = Vector2.one;
    [field: SerializeField] public float WallThickness { get; set; } = 0.05f;
    [field: SerializeField] public int BallsPerAxis { get; set; } = 8;
    [field: SerializeField] public float BallRadius { get; set; } = 0.04f;
    [field: SerializeField] public Vector2 BallPadding { get; set; } = new(0.01f, 0.01f);
    [field: SerializeField] public Vector2 BucketOffset { get; set; }

    public PhysicsBody BucketBody => _bucketBody;

    PhysicsBody _bucketBody;
    (PolygonGeometry bottom, PolygonGeometry left, PolygonGeometry right) _bucketGeometry;
    CircleGeometry _ballGeometry;
    readonly List<PhysicsBody> _ballBodies = new();

    void Start()
    {
        CreateBucket();
        CreateBalls();
    }

    void OnDestroy()
    {
        if (_bucketBody.isValid)
            _bucketBody.Destroy();

        for (var i = 0; i < _ballBodies.Count; ++i)
        {
            var body = _ballBodies[i];
            if (body.isValid)
                body.Destroy();
        }

        _ballBodies.Clear();
    }

    void CreateBucket()
    {
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Kinematic;
        bodyDef.position = (Vector2)transform.position + BucketOffset;

        _bucketBody = PhysicsWorldManager.World.CreateBody(bodyDef);

        var size = BucketSize;
        var half = size * 0.5f;
        var thickness = WallThickness;

        var bottomSize = new Vector2(size.x, thickness);
        var sideSize = new Vector2(thickness, size.y);

        var xform_b = new PhysicsTransform(new Vector2(0f, -half.y + thickness * 0.5f));
        var xform_l = new PhysicsTransform(new Vector2(-half.x + thickness * 0.5f, 0f));
        var xform_r = new PhysicsTransform(new Vector2(half.x - thickness * 0.5f, 0f));

        _bucketGeometry.bottom = PolygonGeometry.CreateBox(bottomSize, 0f, xform_b);
        _bucketGeometry.left = PolygonGeometry.CreateBox(sideSize, 0f, xform_l);
        _bucketGeometry.right = PolygonGeometry.CreateBox(sideSize, 0f, xform_r);

        _bucketBody.CreateShape(_bucketGeometry.bottom, PhysicsShapeDefinition.defaultDefinition);
        _bucketBody.CreateShape(_bucketGeometry.left, PhysicsShapeDefinition.defaultDefinition);
        _bucketBody.CreateShape(_bucketGeometry.right, PhysicsShapeDefinition.defaultDefinition);
    }

    void CreateBalls()
    {
        var innerSize = BucketSize - new Vector2(WallThickness * 2f, WallThickness);
        var innerHalf = innerSize * 0.5f;
        var start = (Vector2)transform.position + BucketOffset;
        var min = start + new Vector2(-innerHalf.x + BallRadius + BallPadding.x, -innerHalf.y + BallRadius + BallPadding.y);
        var step = new Vector2(
            (innerSize.x - (BallRadius + BallPadding.x) * 2f) / Mathf.Max(1, BallsPerAxis - 1),
            (innerSize.y - (BallRadius + BallPadding.y) * 2f) / Mathf.Max(1, BallsPerAxis - 1));

        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;

        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        _ballGeometry = new CircleGeometry { radius = BallRadius };

        for (var y = 0; y < BallsPerAxis; ++y)
        {
            for (var x = 0; x < BallsPerAxis; ++x)
            {
                bodyDef.position = min + new Vector2(step.x * x, step.y * y);
                var body = PhysicsWorldManager.World.CreateBody(bodyDef);
                body.CreateShape(_ballGeometry, shapeDef);
                _ballBodies.Add(body);
            }
        }
    }

    void Update()
    {
        var world = PhysicsWorldManager.World;

        var bucketDebugColor = new Color(0.2f, 0.7f, 0.9f, 1f);
        var ballDebugColor = new Color(0.95f, 0.9f, 0.2f, 1f);

        if (_bucketBody.isValid)
        {
            var xform = _bucketBody.transform;
            world.DrawGeometry(_bucketGeometry.bottom, xform, bucketDebugColor);
            world.DrawGeometry(_bucketGeometry.left, xform, bucketDebugColor);
            world.DrawGeometry(_bucketGeometry.right, xform, bucketDebugColor);
        }

        for (var i = 0; i < _ballBodies.Count; ++i)
        {
            var body = _ballBodies[i];
            if (!body.isValid) continue;
            world.DrawGeometry(_ballGeometry, body.transform, ballDebugColor);
        }
    }
}
