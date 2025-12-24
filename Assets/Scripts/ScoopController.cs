using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevelPhysics2D;
public class ScoopController : MonoBehaviour, IStageInitializable
{
    [field:SerializeField] public Scoop Scoop { get; set; }
    [field:SerializeField] public Bucket Bucket { get; set; }
    [field:SerializeField] public Camera TargetCamera { get; set; }
    [field:SerializeField] public float MouseSpringFrequency { get; set; } = 8f;
    [field:SerializeField] public float MouseSpringDamping { get; set; } = 0.7f;
    [field:SerializeField] public float RimSpringFrequency { get; set; } = 3f;
    [field:SerializeField] public float RimSpringDamping { get; set; } = 0.9f;

    PhysicsBody _mouseBody;
    PhysicsJoint _mouseJoint;
    PhysicsJoint _rimJoint;

    public void InitializeStage(StageManager stage)
    {
        if (TargetCamera == null)
            TargetCamera = Camera.main;

        CreateMouseBody();
        CreateRimJoint();
    }

    void OnDestroy()
    {
        if (_mouseJoint.isValid)
            _mouseJoint.Destroy();

        if (_mouseBody.isValid)
            _mouseBody.Destroy();
    }

    void Update()
    {
        if (!_mouseBody.isValid)
            return;

        var pointer = Pointer.current;
        if (TargetCamera == null || pointer == null)
            return;

        if (pointer.press.isPressed)
        {
            if (!_mouseJoint.isValid)
                CreateMouseJoint();

            var target = (Vector2)TargetCamera.ScreenToWorldPoint(pointer.position.value);
            var mouseTransform = _mouseBody.transform;
            mouseTransform.position = target;
            _mouseBody.transform = mouseTransform;
            _mouseBody.linearVelocity = Vector2.zero;
            _mouseBody.angularVelocity = 0f;
        }
        else if (_mouseJoint.isValid)
            _mouseJoint.Destroy();
    }

    void CreateMouseBody()
    {
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Kinematic;
        bodyDef.position = Scoop.ScoopBody.transform.position;
        _mouseBody = PhysicsWorld.defaultWorld.CreateBody(bodyDef);
    }

    void CreateMouseJoint()
    {
        var jointDef = PhysicsDistanceJointDefinition.defaultDefinition;
        jointDef.bodyA = Scoop.ScoopBody;
        jointDef.bodyB = _mouseBody;
        jointDef.localAnchorA = new PhysicsTransform(Scoop.TipLocal);
        jointDef.localAnchorB = PhysicsTransform.identity;
        jointDef.distance = 0f;
        jointDef.enableSpring = true;
        jointDef.springFrequency = MouseSpringFrequency;
        jointDef.springDamping = MouseSpringDamping;
        _mouseJoint = PhysicsWorld.defaultWorld.CreateJoint(jointDef);
    }

    void CreateRimJoint()
    {
        var bucketHalf = Bucket.BucketSize * 0.5f;
        var rimLocal = new Vector2(
            bucketHalf.x - Bucket.WallThickness * 0.5f,
            bucketHalf.y - Bucket.WallThickness * 0.5f);
        var bucketBody = Bucket.BucketBody;

        var jointDef = PhysicsDistanceJointDefinition.defaultDefinition;
        jointDef.bodyA = Scoop.ScoopBody;
        jointDef.bodyB = bucketBody;
        jointDef.localAnchorA = new PhysicsTransform(Scoop.HandleTipLocal);
        jointDef.localAnchorB = new PhysicsTransform(rimLocal);
        jointDef.distance = 0f;
        jointDef.enableSpring = true;
        jointDef.springFrequency = RimSpringFrequency;
        jointDef.springDamping = RimSpringDamping;
        jointDef.collideConnected = true;
        _rimJoint = PhysicsWorld.defaultWorld.CreateJoint(jointDef);
    }
}
