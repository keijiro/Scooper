using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class Bucket : MonoBehaviour, IStageInitializable
{
    [field:SerializeField] public Vector2 BucketSize { get; set; } = Vector2.one;
    [field:SerializeField] public float WallThickness { get; set; } = 0.05f;
    [field:SerializeField] public Vector2 BucketOffset { get; set; }

    public PhysicsBody BucketBody => _bucketBody;
    public Vector2 BucketOrigin => (Vector2)transform.position + BucketOffset;

    PhysicsBody _bucketBody;
    (PolygonGeometry bottom, PolygonGeometry left, PolygonGeometry right) _bucketGeometry;

    public void InitializeStage(StageManager stage)
      => CreateBucket();

    void OnDestroy()
    {
        if (_bucketBody.isValid)
            _bucketBody.Destroy();
    }

    void Update()
    {
        if (!_bucketBody.isValid)
            return;

        var bucketDebugColor = new Color(0.2f, 0.7f, 0.9f, 1f);
        var xform = _bucketBody.transform;
        var world = StageManager.World;
        world.DrawGeometry(_bucketGeometry.bottom, xform, bucketDebugColor);
        world.DrawGeometry(_bucketGeometry.left, xform, bucketDebugColor);
        world.DrawGeometry(_bucketGeometry.right, xform, bucketDebugColor);
    }

    void CreateBucket()
    {
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Kinematic;
        bodyDef.position = BucketOrigin;

        _bucketBody = StageManager.World.CreateBody(bodyDef);

        var size = BucketSize;
        var half = size * 0.5f;
        var thickness = WallThickness;

        var bottomSize = new Vector2(size.x, thickness);
        var sideSize = new Vector2(thickness, size.y);

        var xformBottom = new PhysicsTransform(new Vector2(0f, -half.y + thickness * 0.5f));
        var xformLeft = new PhysicsTransform(new Vector2(-half.x + thickness * 0.5f, 0f));
        var xformRight = new PhysicsTransform(new Vector2(half.x - thickness * 0.5f, 0f));

        _bucketGeometry.bottom = PolygonGeometry.CreateBox(bottomSize, 0f, xformBottom);
        _bucketGeometry.left = PolygonGeometry.CreateBox(sideSize, 0f, xformLeft);
        _bucketGeometry.right = PolygonGeometry.CreateBox(sideSize, 0f, xformRight);

        _bucketBody.CreateShape(_bucketGeometry.bottom, PhysicsShapeDefinition.defaultDefinition);
        _bucketBody.CreateShape(_bucketGeometry.left, PhysicsShapeDefinition.defaultDefinition);
        _bucketBody.CreateShape(_bucketGeometry.right, PhysicsShapeDefinition.defaultDefinition);
    }
}
