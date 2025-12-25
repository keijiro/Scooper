using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class TrayGemDetector : MonoBehaviour
{
    [SerializeField] Tray _tray = null;
    [SerializeField] PaydirtManager _paydirtManager = null;

    readonly HashSet<PhysicsBody> _gemBodies = new();
    int _gemCount;

    void FixedUpdate()
    {
        var trayBody = _tray.TrayBody;
        if (!trayBody.isValid)
            return;

        RefreshGemBodies();
        if (_gemBodies.Count == 0)
            return;

        using var contacts = trayBody.GetContacts(Allocator.Temp);
        for (var i = 0; i < contacts.Length; ++i)
        {
            var contact = contacts[i];
            var shapeA = contact.shapeA;
            var shapeB = contact.shapeB;

            var otherBody = shapeA.body;
            if (otherBody.Equals(trayBody))
                otherBody = shapeB.body;

            if (!_gemBodies.Contains(otherBody))
                continue;

            Debug.Log("Gem touched tray.", this);
            break;
        }
    }

    void RefreshGemBodies()
    {
        var gems = _paydirtManager.GemBodies;
        if (_gemCount == gems.Count)
            return;

        _gemBodies.Clear();
        for (var i = 0; i < gems.Count; ++i)
            _gemBodies.Add(gems[i]);

        _gemCount = gems.Count;
    }
}
