using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class Bucket : MonoBehaviour, IStageInitializable
{
    [field:SerializeField] public Vector2 BucketSize { get; set; } = Vector2.one;
    [field:SerializeField] public float WallThickness { get; set; } = 0.05f;
    [field:SerializeField] public Vector2 BucketOffset { get; set; }
    [field:SerializeField] public float LeftWallHeightRatio { get; set; } = 1f;
    [field:SerializeField] public float BottomOpenAngle { get; set; } = 60f;
    [field:SerializeField] public float BottomOpenSpeed { get; set; } = 180f;

    public PhysicsBody BucketBody => _bucketBody;
    public Vector2 BucketOrigin => (Vector2)transform.position + BucketOffset;

    PhysicsBody _bucketBody;
    PhysicsBody _bottomLeftBody;
    PhysicsBody _bottomRightBody;
    (PolygonGeometry bottom, PolygonGeometry left, PolygonGeometry right) _bucketGeometry;
    Vector2 _bottomLeftHinge;
    Vector2 _bottomRightHinge;
    float _bottomAngle;
    float _bottomTargetAngle;

    public void InitializeStage(StageManager stage)
      => CreateBucket();

    void OnDestroy()
    {
        if (_bucketBody.isValid)
            _bucketBody.Destroy();

        if (_bottomLeftBody.isValid)
            _bottomLeftBody.Destroy();

        if (_bottomRightBody.isValid)
            _bottomRightBody.Destroy();
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

        var bottomSize = new Vector2(size.x * 0.5f, thickness);
        var rightSize = new Vector2(thickness, size.y);
        var leftSize = new Vector2(thickness, leftHeight);
        var bottomHalfWidth = bottomSize.x * 0.5f;

        var xformBottom = new PhysicsTransform(new Vector2(0f, -half.y + thickness * 0.5f));
        var xformLeft = new PhysicsTransform(new Vector2(-half.x + thickness * 0.5f, -half.y + leftHalf));
        var xformRight = new PhysicsTransform(new Vector2(half.x - thickness * 0.5f, 0f));

        _bucketGeometry.left = PolygonGeometry.CreateBox(leftSize, 0f, xformLeft);
        _bucketGeometry.right = PolygonGeometry.CreateBox(rightSize, 0f, xformRight);

        _bucketBody.CreateShape(_bucketGeometry.left, PhysicsShapeDefinition.defaultDefinition);
        _bucketBody.CreateShape(_bucketGeometry.right, PhysicsShapeDefinition.defaultDefinition);

        CreateBottomBodies(xformBottom.position, bottomSize, bottomHalfWidth, thickness);
    }

    void CreateBottomBodies(Vector2 bottomCenter, Vector2 bottomSize, float bottomHalfWidth, float thickness)
    {
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Kinematic;

        var half = BucketSize * 0.5f;
        var hingeY = BucketOrigin.y + bottomCenter.y;
        var leftHingeX = BucketOrigin.x - half.x + thickness * 0.5f;
        var rightHingeX = BucketOrigin.x + half.x - thickness * 0.5f;
        _bottomLeftHinge = new Vector2(leftHingeX, hingeY);
        _bottomRightHinge = new Vector2(rightHingeX, hingeY);

        bodyDef.position = _bottomLeftHinge;
        _bottomLeftBody = PhysicsWorld.defaultWorld.CreateBody(bodyDef);
        var leftOffset = new PhysicsTransform(new Vector2(bottomHalfWidth, -thickness * 0.5f));
        var leftGeometry = PolygonGeometry.CreateBox(bottomSize, 0f, leftOffset);
        _bottomLeftBody.CreateShape(leftGeometry, PhysicsShapeDefinition.defaultDefinition);

        bodyDef.position = _bottomRightHinge;
        _bottomRightBody = PhysicsWorld.defaultWorld.CreateBody(bodyDef);
        var rightOffset = new PhysicsTransform(new Vector2(-bottomHalfWidth, -thickness * 0.5f));
        var rightGeometry = PolygonGeometry.CreateBox(bottomSize, 0f, rightOffset);
        _bottomRightBody.CreateShape(rightGeometry, PhysicsShapeDefinition.defaultDefinition);
    }

    void Update()
    {
        if (!_bottomLeftBody.isValid || !_bottomRightBody.isValid)
            return;

        var nextAngle = Mathf.MoveTowards(_bottomAngle, _bottomTargetAngle, BottomOpenSpeed * Time.deltaTime);
        if (Mathf.Approximately(nextAngle, _bottomAngle))
            return;

        _bottomAngle = nextAngle;
        UpdateBottomTransforms();
    }

    void UpdateBottomTransforms()
    {
        var radians = _bottomAngle * Mathf.Deg2Rad;
        var leftRotation = new PhysicsRotate(-radians);
        var rightRotation = new PhysicsRotate(radians);

        _bottomLeftBody.SetAndWriteTransform(new PhysicsTransform(_bottomLeftHinge, leftRotation));
        _bottomLeftBody.linearVelocity = Vector2.zero;
        _bottomLeftBody.angularVelocity = 0f;

        _bottomRightBody.SetAndWriteTransform(new PhysicsTransform(_bottomRightHinge, rightRotation));
        _bottomRightBody.linearVelocity = Vector2.zero;
        _bottomRightBody.angularVelocity = 0f;
    }

    public void Open()
      => _bottomTargetAngle = BottomOpenAngle;

    public void Close()
      => _bottomTargetAngle = 0f;
}
