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

    const ulong DetectionMask = (1UL << (int)Categories.Scoop) |
                                (1UL << (int)Categories.Item);

    bool ShouldConsiderContact(PhysicsShape otherShape)
      => (otherShape.contactFilter.categories.bitMask & DetectionMask) != 0;

    float GetMaxNormalSpeed()
    {
        var maxSpeed = 0f;
        var fastBomb = _body.linearVelocity.sqrMagnitude > _speedThreshold;

        using var contacts = _body.GetContacts(Allocator.Temp);

        foreach (var contact in contacts)
        {
            var other = (contact.shapeA.body == _body) ? contact.shapeB : contact.shapeA;
            if (!fastBomb && !ShouldConsiderContact(other)) continue;

            var manifold = contact.manifold;
            if (manifold.pointCount == 0) continue;

            var points = manifold.points;
            maxSpeed = Mathf.Max(maxSpeed, Mathf.Abs(points.contactInfo0.normalVelocity));

            if (manifold.pointCount == 1) continue;

            maxSpeed = Mathf.Max(maxSpeed, Mathf.Abs(points.contactInfo1.normalVelocity));
        }

        return maxSpeed;
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
