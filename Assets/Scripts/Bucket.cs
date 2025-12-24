using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class Bucket : MonoBehaviour, IStageInitializable
{
    [field:SerializeField] public Vector2 BucketSize { get; set; } = Vector2.one;
    [field:SerializeField] public float WallThickness { get; set; } = 0.05f;
    [field:SerializeField] public Vector2 BucketOffset { get; set; }
    [field:SerializeField] public float LeftWallHeightRatio { get; set; } = 1f;

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

    void CreateBucket()
    {
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.position = BucketOrigin;

        _bucketBody = PhysicsWorld.defaultWorld.CreateBody(bodyDef);

        var size = BucketSize;
        var half = size * 0.5f;
        var thickness = WallThickness;
        var leftHeight = size.y * Mathf.Clamp01(LeftWallHeightRatio);
        var leftHalf = leftHeight * 0.5f;

        var bottomSize = new Vector2(size.x, thickness);
        var rightSize = new Vector2(thickness, size.y);
        var leftSize = new Vector2(thickness, leftHeight);

        var xformBottom = new PhysicsTransform(new Vector2(0f, -half.y + thickness * 0.5f));
        var xformLeft = new PhysicsTransform(new Vector2(-half.x + thickness * 0.5f, -half.y + leftHalf));
        var xformRight = new PhysicsTransform(new Vector2(half.x - thickness * 0.5f, 0f));

        _bucketGeometry.bottom = PolygonGeometry.CreateBox(bottomSize, 0f, xformBottom);
        _bucketGeometry.left = PolygonGeometry.CreateBox(leftSize, 0f, xformLeft);
        _bucketGeometry.right = PolygonGeometry.CreateBox(rightSize, 0f, xformRight);

        _bucketBody.CreateShape(_bucketGeometry.bottom, PhysicsShapeDefinition.defaultDefinition);
        _bucketBody.CreateShape(_bucketGeometry.left, PhysicsShapeDefinition.defaultDefinition);
        _bucketBody.CreateShape(_bucketGeometry.right, PhysicsShapeDefinition.defaultDefinition);
    }
}
