using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class ScoopController : MonoBehaviour
{
    #region Editable Fields

    [Space]
    [SerializeField] float _pickerSpring = 2f;
    [SerializeField] float _anchorSpring = 2f;
    [SerializeField] float _damping = 1;
    [Space]
    [SerializeField] GameObject _scoopPrefab = null;
    [SerializeField] SpoutPositionProvider _spout = null;
    [Space]
    [SerializeField] StaticBodyBridge _anchorBody = null;
    [SerializeField] Transform _anchorPoint = null;
    [Space]
    [SerializeField] Camera _targetCamera = null;
    [SerializeField] InputHandler _input = null;
    [SerializeField] LineRenderer _anchorLine = null;
    [SerializeField] LineRenderer _pickerLine = null;

    #endregion

    #region Scoop Instance Handling (public)

    (PhysicsBody body, Transform tip, Transform rim) _scoop;

    public void SpawnScoopInstance()
    {
        var pos = _spout.GetPosition();
        var go = Instantiate(_scoopPrefab, pos, Quaternion.identity);

        _scoop.body = go.GetComponent<DynamicBodyBridge>().Body;
        _scoop.tip = go.transform.Find("Anchor Tip");
        _scoop.rim = go.transform.Find("Anchor Rim");

        CreateAnchorJoint();
    }

    public void ThrowScoopInstance()
    {
        if (_anchorJoint.isValid) _anchorJoint.Destroy();
        if (_pickerJoint.isValid) _pickerJoint.Destroy();
        _scoop = default;
    }

    #endregion

    #region MonoBehaviour Implementation

    void Start()
    {
        CreatePickerBody();
        UpdateLines();
    }

    void OnDestroy()
    {
        if (_anchorJoint.isValid) _anchorJoint.Destroy();
        if (_pickerJoint.isValid) _pickerJoint.Destroy();
        _pickerBody.Destroy();
    }

    void Update()
    {
        UpdatePickerBody(_input.Position);

        if (_scoop.body.isValid)
        {
            if (_input.IsPressed)
            {
                if (!_pickerJoint.isValid) CreatePickerJoint();
            }
            else
            {
                if (_pickerJoint.isValid) _pickerJoint.Destroy();
            }
        }

        UpdateLines();
    }

    #endregion

    #region Picker Body Handling

    PhysicsBody _pickerBody;

    void CreatePickerBody()
    {
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Kinematic;
        bodyDef.position = transform.position;
        _pickerBody = PhysicsWorld.defaultWorld.CreateBody(bodyDef);
    }

    void UpdatePickerBody(Vector2 pos)
    {
        var xform = _pickerBody.transform;
        xform.position = _targetCamera.ScreenToWorldPoint(pos);
        _pickerBody.transform = xform;
    }

    #endregion

    #region Physics Object Handling

    PhysicsJoint _pickerJoint;
    PhysicsJoint _anchorJoint;

    void CreatePickerJoint()
    {
        var jointDef = PhysicsDistanceJointDefinition.defaultDefinition;
        jointDef.bodyA = _scoop.body;
        jointDef.bodyB = _pickerBody;

        var scoopTip = jointDef.bodyA.GetLocalPoint(_scoop.tip.position);
        jointDef.localAnchorA = new PhysicsTransform(scoopTip);
        jointDef.localAnchorB = PhysicsTransform.identity;

        jointDef.distance = 0;
        jointDef.enableSpring = true;
        jointDef.springFrequency = _pickerSpring;
        jointDef.springDamping = _damping;

        _pickerJoint = PhysicsWorld.defaultWorld.CreateJoint(jointDef);
    }

    void CreateAnchorJoint()
    {
        var jointDef = PhysicsDistanceJointDefinition.defaultDefinition;
        jointDef.bodyA = _scoop.body;
        jointDef.bodyB = _anchorBody.Body;

        var scoopRim = jointDef.bodyA.GetLocalPoint(_scoop.rim.position);
        var anchor = jointDef.bodyB.GetLocalPoint(_anchorPoint.position);
        jointDef.localAnchorA = new PhysicsTransform(scoopRim);
        jointDef.localAnchorB = new PhysicsTransform(anchor);

        jointDef.distance = 0;
        jointDef.enableSpring = true;
        jointDef.springFrequency = _anchorSpring;
        jointDef.springDamping = _damping;
        jointDef.collideConnected = true;

        _anchorJoint = PhysicsWorld.defaultWorld.CreateJoint(jointDef);
    }

    #endregion

    #region Line Rendering

    void UpdateLines()
    {
        _anchorLine.enabled = _anchorJoint.isValid;
        _pickerLine.enabled = _pickerJoint.isValid;

        if (_anchorLine.enabled)
        {
            _anchorLine.SetPosition(0, _scoop.rim.position);
            _anchorLine.SetPosition(1, _anchorPoint.position);
        }

        if (_pickerLine.enabled)
        {
            _pickerLine.SetPosition(0, _scoop.tip.position);
            _pickerLine.SetPosition(1, _pickerBody.transform.position);
        }
    }

    #endregion
}
