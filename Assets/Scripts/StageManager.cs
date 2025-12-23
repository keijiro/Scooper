using System;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public interface IStageInitializable
{
    void InitializeStage(StageManager stage);
}

public class StageManager : MonoBehaviour
{
    [field:SerializeField] PhysicsWorldDefinition _worldDefinition = PhysicsWorldDefinition.defaultDefinition;
    [SerializeField] MonoBehaviour[] _initializers = Array.Empty<MonoBehaviour>();

    public static PhysicsWorld World { get; private set; }

    void Awake()
    {
        if (!World.isValid)
            World = PhysicsWorld.Create(_worldDefinition);

        for (var i = 0; i < _initializers.Length; ++i)
        {
            var initializer = _initializers[i] as IStageInitializable;
            if (initializer == null)
            {
                Debug.LogError("Initializer does not implement IStageInitializable.", _initializers[i]);
                continue;
            }

            initializer.InitializeStage(this);
        }
    }

    void OnDestroy()
    {
        if (World.isValid)
            World.Destroy();
    }
}
