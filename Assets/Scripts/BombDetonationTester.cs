using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class BombDetonationTester : MonoBehaviour
{
    [SerializeField] PaydirtManager _paydirtManager = null;
    [field:SerializeField] public float DetonationSpeedThreshold { get; set; } = 1f;

    readonly HashSet<PhysicsBody> _detonatedBodies = new();

    void FixedUpdate()
    {
        var bombs = _paydirtManager.BombBodies;
        for (var i = 0; i < bombs.Count; ++i)
        {
            var body = bombs[i];
            if (!body.isValid || _detonatedBodies.Contains(body))
                continue;

            var maxSpeed = GetMaxNormalSpeed(body);
            if (maxSpeed < DetonationSpeedThreshold)
                continue;

            _detonatedBodies.Add(body);
            Debug.Log($"Bomb detonated: normalSpeed={maxSpeed:0.###}", this);
        }
    }

    float GetMaxNormalSpeed(PhysicsBody body)
    {
        var maxSpeed = 0f;
        using var contacts = body.GetContacts(Allocator.Temp);
        for (var i = 0; i < contacts.Length; ++i)
        {
            var manifold = contacts[i].manifold;
            var points = manifold.points;
            for (var p = 0; p < manifold.pointCount; ++p)
                maxSpeed = Mathf.Max(maxSpeed, Mathf.Abs(points[p].normalVelocity));
        }

        return maxSpeed;
    }
}
