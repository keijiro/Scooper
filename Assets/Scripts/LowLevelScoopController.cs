using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevelPhysics2D;

public class LowLevelScoopController : MonoBehaviour
{
    [field: SerializeField] public LowLevelBucketFill BucketFill { get; set; }
    [field: SerializeField] public Camera TargetCamera { get; set; }
    [field: SerializeField] public Vector2 ScoopSize { get; set; } = new(0.7f, 0.25f);
    [field: SerializeField] public float WallThickness { get; set; } = 0.04f;
    [field: SerializeField] public float BackWallHeight { get; set; } = 0.1f;
    [field: SerializeField] public float HandleLength { get; set; } = 0.35f;
    [field: SerializeField] public Vector2 SpawnOffset { get; set; } = new(0f, 0.6f);
    [field: SerializeField] public float MouseSpringFrequency { get; set; } = 8f;
    [field: SerializeField] public float MouseSpringDamping { get; set; } = 0.7f;
    [field: SerializeField] public float RimSpringFrequency { get; set; } = 3f;
    [field: SerializeField] public float RimSpringDamping { get; set; } = 0.9f;

    PhysicsBody _scoopBody;
    PhysicsBody _mouseBody;
    PhysicsJoint _mouseJoint;
    PhysicsJoint _rimJoint;
    (PolygonGeometry bottom, PolygonGeometry left, PolygonGeometry right, PolygonGeometry back, PolygonGeometry handle) _scoopGeometry;

    void Start()
    {
        CreateScoop();
    }

    void OnDestroy()
    {
        if (_scoopBody.isValid) _scoopBody.Destroy();
        if (_mouseBody.isValid) _mouseBody.Destroy();
    }

    void Update()
    {
        var mouse = Mouse.current;
        var target = (Vector2)TargetCamera.ScreenToWorldPoint(mouse.position.value);
        var mouseTransform = _mouseBody.transform;
        mouseTransform.position = target;
        _mouseBody.transform = mouseTransform;
        _mouseBody.linearVelocity = Vector2.zero;
        _mouseBody.angularVelocity = 0f;

        TryCreateRimJoint();

        var scoopDebugColor = new Color(0.9f, 0.6f, 0.2f, 1f);
        var world = PhysicsWorldManager.World;
        var xform = _scoopBody.transform;
        world.DrawGeometry(_scoopGeometry.bottom, xform, scoopDebugColor);
        world.DrawGeometry(_scoopGeometry.left, xform, scoopDebugColor);
        world.DrawGeometry(_scoopGeometry.right, xform, scoopDebugColor);
        world.DrawGeometry(_scoopGeometry.back, xform, scoopDebugColor);
        world.DrawGeometry(_scoopGeometry.handle, xform, scoopDebugColor);
    }

    void CreateScoop()
    {
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;
        bodyDef.position = (Vector2)transform.position + SpawnOffset;

        _scoopBody = PhysicsWorldManager.World.CreateBody(bodyDef);

        var mouseBodyDef = PhysicsBodyDefinition.defaultDefinition;
        mouseBodyDef.type = PhysicsBody.BodyType.Kinematic;
        mouseBodyDef.position = bodyDef.position;
        _mouseBody = PhysicsWorldManager.World.CreateBody(mouseBodyDef);

        var size = ScoopSize;
        var half = size * 0.5f;
        var thickness = WallThickness;

        var bottomSize = new Vector2(size.x, thickness);
        var sideSize = new Vector2(thickness, size.y);
        var backSize = new Vector2(size.x, BackWallHeight);
        var handleSize = new Vector2(HandleLength, thickness);

        var xformBottom = new PhysicsTransform(new Vector2(0f, -half.y + thickness * 0.5f));
        var xformLeft = new PhysicsTransform(new Vector2(-half.x + thickness * 0.5f, 0f));
        var xformRight = new PhysicsTransform(new Vector2(half.x - thickness * 0.5f, 0f));
        var xformBack = new PhysicsTransform(new Vector2(0f, half.y - BackWallHeight * 0.5f));
        var xformHandle = new PhysicsTransform(new Vector2(half.x + HandleLength * 0.5f, 0f));

        _scoopGeometry.bottom = PolygonGeometry.CreateBox(bottomSize, 0f, xformBottom);
        _scoopGeometry.left = PolygonGeometry.CreateBox(sideSize, 0f, xformLeft);
        _scoopGeometry.right = PolygonGeometry.CreateBox(sideSize, 0f, xformRight);
        _scoopGeometry.back = PolygonGeometry.CreateBox(backSize, 0f, xformBack);
        _scoopGeometry.handle = PolygonGeometry.CreateBox(handleSize, 0f, xformHandle);

        _scoopBody.CreateShape(_scoopGeometry.bottom, PhysicsShapeDefinition.defaultDefinition);
        _scoopBody.CreateShape(_scoopGeometry.left, PhysicsShapeDefinition.defaultDefinition);
        _scoopBody.CreateShape(_scoopGeometry.right, PhysicsShapeDefinition.defaultDefinition);
        _scoopBody.CreateShape(_scoopGeometry.back, PhysicsShapeDefinition.defaultDefinition);
        _scoopBody.CreateShape(_scoopGeometry.handle, PhysicsShapeDefinition.defaultDefinition);

        var tipLocal = new Vector2(-half.x, 0f);
        var baseLocal = new Vector2(half.x, 0f);

        var mouseJointDef = PhysicsDistanceJointDefinition.defaultDefinition;
        mouseJointDef.bodyA = _scoopBody;
        mouseJointDef.bodyB = _mouseBody;
        mouseJointDef.localAnchorA = new PhysicsTransform(tipLocal);
        mouseJointDef.localAnchorB = PhysicsTransform.identity;
        mouseJointDef.distance = 0f;
        mouseJointDef.enableSpring = true;
        mouseJointDef.springFrequency = MouseSpringFrequency;
        mouseJointDef.springDamping = MouseSpringDamping;
        _mouseJoint = PhysicsWorldManager.World.CreateJoint(mouseJointDef);
        TryCreateRimJoint();
    }

    void TryCreateRimJoint()
    {
        if (_rimJoint.isValid) return;

        var bucketBody = BucketFill.BucketBody;
        if (!bucketBody.isValid) return;

        var half = ScoopSize * 0.5f;
        var baseLocal = new Vector2(half.x, 0f);
        var bucketHalf = BucketFill.BucketSize * 0.5f;
        var rimLocal = new Vector2(bucketHalf.x - BucketFill.WallThickness * 0.5f, bucketHalf.y - BucketFill.WallThickness * 0.5f);

        var rimJointDef = PhysicsDistanceJointDefinition.defaultDefinition;
        rimJointDef.bodyA = _scoopBody;
        rimJointDef.bodyB = bucketBody;
        rimJointDef.localAnchorA = new PhysicsTransform(baseLocal);
        rimJointDef.localAnchorB = new PhysicsTransform(rimLocal);
        rimJointDef.distance = 0f;
        rimJointDef.enableSpring = true;
        rimJointDef.springFrequency = RimSpringFrequency;
        rimJointDef.springDamping = RimSpringDamping;
        rimJointDef.collideConnected = true;
        _rimJoint = PhysicsWorldManager.World.CreateJoint(rimJointDef);
    }
}
