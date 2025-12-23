using System;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public interface IStageInitializable
{
    void InitializeStage(StageManager stage);
}

public class StageManager : MonoBehaviour
{
    [SerializeField] MonoBehaviour[] _initializers = null;

    void Start()
    {
        foreach (IStageInitializable initializer in _initializers)
            initializer.InitializeStage(this);
    }
}
