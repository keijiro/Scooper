using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class PhysicsWorldManager : MonoBehaviour
{
    [field:SerializeField] PhysicsWorldDefinition
      _worldDefinition = PhysicsWorldDefinition.defaultDefinition;

    public static PhysicsWorld World { get; private set; }

    void Awake()
    {
        if (!World.isValid) World = PhysicsWorld.Create(_worldDefinition);
    }

    void OnDestroy()
    {
        if (World.isValid) World.Destroy();
    }
}
