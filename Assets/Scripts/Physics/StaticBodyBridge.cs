using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class StaticBodyBridge : MonoBehaviour
{
    #region Editable Fields

    [SerializeField] bool _isKinematic = false;
    [SerializeField] Categories _category = Categories.Default;
    [SerializeField] Categories _ignore = Categories.None;

    #endregion

    #region Public Properties

    public PhysicsBody Body => _body;

    #endregion

    #region Transform Cache and Checker

    (Vector2 position, float rotation) _lastXform;

    bool IsPositionChanged
      => _lastXform.position != (Vector2)transform.position;

    bool IsRotationChanged
      => !Mathf.Approximately(Mathf.DeltaAngle(_lastXform.rotation, transform.eulerAngles.z), 0);

    void CacheTransform()
    {
        _lastXform.position = transform.position;
        _lastXform.rotation = transform.eulerAngles.z;
    }

    #endregion

    #region Physics Body Management

    CompositeShapeBuilder _shapeBuilder;
    PhysicsBody _body;

    void CreateBody()
    {
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = _isKinematic ? PhysicsBody.BodyType.Kinematic :
                                      PhysicsBody.BodyType.Static;
        bodyDef.position = transform.position;

        _body = PhysicsWorld.defaultWorld.CreateBody(bodyDef);

        var definition = PhysicsShapeDefinition.defaultDefinition;

        var category = new PhysicsMask((int)_category);
        var mask = PhysicsMask.All;
        mask.ResetBit((int)_ignore);
        definition.contactFilter = new PhysicsShape.ContactFilter(category, mask);

        _shapeBuilder.CreateShapes(_body, definition);
    }

    void ApplyTransform()
    {
        var rot = new PhysicsRotate(transform.eulerAngles.z * Mathf.Deg2Rad);
        var xform = new PhysicsTransform(transform.position, rot);
        _body.SetAndWriteTransform(xform);
    }

    #endregion

    #region MonoBehaviour Implementation

    void Awake()
      => _shapeBuilder = GetComponent<CompositeShapeBuilder>();

    void Start()
    {
        if (_shapeBuilder == null)
            return;

        CreateBody();
        ApplyTransform();
        CacheTransform();
    }

    void OnDestroy()
    {
        if (_body.isValid)
            _body.Destroy();
    }

    void FixedUpdate()
    {
        if (!_body.isValid)
            return;

        if (IsPositionChanged || IsRotationChanged)
        {
            ApplyTransform();
            CacheTransform();
        }
    }

    #endregion
}
