using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class BombDetonationTester : MonoBehaviour
{
    [field:SerializeField] public PaydirtManager PaydirtManager { get; set; }
    [field:SerializeField] public float DetonationImpulseThreshold { get; set; } = 1f;

    readonly HashSet<PhysicsBody> _detonatedBodies = new();

    void FixedUpdate()
    {
        if (PaydirtManager == null)
            return;

        var bombs = PaydirtManager.BombBodies;
        for (var i = 0; i < bombs.Count; ++i)
        {
            var body = bombs[i];
            if (!body.isValid || _detonatedBodies.Contains(body))
                continue;

            var maxImpulse = GetMaxImpulse(body);
            if (maxImpulse < DetonationImpulseThreshold)
                continue;

            _detonatedBodies.Add(body);
            Debug.Log($"Bomb detonated: impulse={maxImpulse:0.###}", this);
        }
    }

    float GetMaxImpulse(PhysicsBody body)
    {
        var maxImpulse = 0f;
        using var contacts = body.GetContacts(Allocator.Temp);
        for (var i = 0; i < contacts.Length; ++i)
        {
            var manifold = contacts[i].manifold;
            var points = manifold.points;
            for (var p = 0; p < manifold.pointCount; ++p)
                maxImpulse = Mathf.Max(maxImpulse, points[p].totalNormalImpulse);
        }

        return maxImpulse;
    }
}
