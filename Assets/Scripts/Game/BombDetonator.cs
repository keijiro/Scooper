using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public sealed class BombDetonator : MonoBehaviour
{
    #region Editor Fields

    [SerializeField] float _speedThreshold = 1;
    [SerializeField] float _activationDelay = 3;

    #endregion

    #region Private Members

    PhysicsBody _body;
    float _elapsed;

    float GetMaxNormalSpeed()
    {
        var maxSpeed = 0f;
        var bombSpeed = _body.linearVelocity.sqrMagnitude;

        using var contacts = _body.GetContacts(Allocator.Temp);

        for (var i = 0; i < contacts.Length; ++i)
        {
            var contact = contacts[i];
            var otherShape = contact.shapeA.body == _body ? contact.shapeB : contact.shapeA;
            var otherBody = otherShape.body;
            if (!ShouldConsiderContact(otherShape, otherBody, bombSpeed)) continue;

            var manifold = contact.manifold;
            var points = manifold.points;
            for (var p = 0; p < manifold.pointCount; ++p)
                maxSpeed = Mathf.Max(maxSpeed, Mathf.Abs(points[p].normalVelocity));
        }

        return maxSpeed;
    }

    bool ShouldConsiderContact(PhysicsShape otherShape, PhysicsBody otherBody, float bombSpeed)
    {
        var otherCategories = otherShape.contactFilter.categories;
        var scoopBit = (1UL << (int)Categories.Scoop) | (1UL << (int)Categories.Item);
        if ((otherCategories.bitMask & scoopBit) != 0) return true;
        return bombSpeed > _speedThreshold;
    }

    #endregion

    #region MonoBehaviour Implementation

    void Start()
      => _body = GetComponent<DynamicBodyBridge>().Body;

    void FixedUpdate()
    {
        _elapsed += Time.fixedDeltaTime;
        if (_elapsed < _activationDelay) return;

        if (GetMaxNormalSpeed() > _speedThreshold)
            GameState.IsBombDetonated = true;
    }

    #endregion
}
