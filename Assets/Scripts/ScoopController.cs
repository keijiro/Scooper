using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevelPhysics2D;

public class ScoopController : MonoBehaviour, IStageInitializable
{
    [field: SerializeField] public Scoop Scoop { get; set; }
    [field: SerializeField] public Bucket Bucket { get; set; }
    [field: SerializeField] public Camera TargetCamera { get; set; }
    [field: SerializeField] public float MouseSpringFrequency { get; set; } = 8f;
    [field: SerializeField] public float MouseSpringDamping { get; set; } = 0.7f;
    [field: SerializeField] public float RimSpringFrequency { get; set; } = 3f;
    [field: SerializeField] public float RimSpringDamping { get; set; } = 0.9f;

    PhysicsBody _mouseBody;
    PhysicsJoint _mouseJoint;
    PhysicsJoint _rimJoint;

    public void InitializeStage(StageManager stage)
    {
        if (Scoop == null)
        {
            Debug.LogError("Scoop is missing.", this);
            enabled = false;
            return;
        }

        if (Bucket == null)
        {
            Debug.LogError("Bucket is missing.", this);
            enabled = false;
            return;
        }

        if (TargetCamera == null)
            TargetCamera = Camera.main;

        if (!Scoop.ScoopBody.isValid)
        {
            Debug.LogError("Scoop is not initialized.", this);
            enabled = false;
            return;
        }

        if (!Bucket.BucketBody.isValid)
        {
            Debug.LogError("Bucket is not initialized.", this);
            enabled = false;
            return;
        }

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

        var mouse = Mouse.current;
        if (TargetCamera == null || mouse == null)
            return;

        if (mouse.leftButton.isPressed)
        {
            if (!_mouseJoint.isValid)
                CreateMouseJoint();

            var target = (Vector2)TargetCamera.ScreenToWorldPoint(mouse.position.value);
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
        _mouseBody = StageManager.World.CreateBody(bodyDef);
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
        _mouseJoint = StageManager.World.CreateJoint(jointDef);
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
        _rimJoint = StageManager.World.CreateJoint(jointDef);
    }
}
