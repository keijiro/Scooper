using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class TrayGemDetector : MonoBehaviour
{
    [field:SerializeField] public Tray Tray { get; set; }
    [field:SerializeField] public PaydirtManager PaydirtManager { get; set; }

    readonly HashSet<PhysicsBody> _gemBodies = new();
    int _gemCount;

    void FixedUpdate()
    {
        if (Tray == null || PaydirtManager == null)
            return;

        var trayBody = Tray.TrayBody;
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
        var gems = PaydirtManager.GemBodies;
        if (_gemCount == gems.Count)
            return;

        _gemBodies.Clear();
        for (var i = 0; i < gems.Count; ++i)
            _gemBodies.Add(gems[i]);

        _gemCount = gems.Count;
    }
}
