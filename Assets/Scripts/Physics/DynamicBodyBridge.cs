using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class DynamicBodyBridge : MonoBehaviour
{
    #region Editable Fields

    [SerializeField] float _density = 1f;
    [SerializeField] Categories _category = Categories.Default;
    [SerializeField] Categories _ignore = Categories.None;

    #endregion

    #region Public Properties

    public PhysicsBody Body => _body;

    #endregion

    #region Physics Body Management

    CompositeShapeBuilder _shapeBuilder;
    PhysicsBody _body;

    void CreateBody()
    {
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;
        bodyDef.position = transform.position;
        bodyDef.rotation = new PhysicsRotate(transform.eulerAngles.z * Mathf.Deg2Rad);

        _body = PhysicsWorld.defaultWorld.CreateBody(bodyDef);

        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        shapeDef.density = _density;
        shapeDef.triggerEvents = true;

        var category = new PhysicsMask((int)_category);
        var mask = PhysicsMask.All;
        mask.ResetBit((int)_ignore);
        shapeDef.contactFilter = new PhysicsShape.ContactFilter(category, mask);

        _shapeBuilder.CreateShapes(_body, shapeDef);
    }

    void SyncTransform()
    {
        var xform = _body.transform;
        var position = xform.position;
        var angle = xform.rotation.angle * Mathf.Rad2Deg;
        transform.SetPositionAndRotation(
            new Vector3(position.x, position.y, transform.position.z),
            Quaternion.Euler(0f, 0f, angle));
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
        SyncTransform();
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

        SyncTransform();
    }

    #endregion
}
