using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class BallFill : MonoBehaviour, IStageInitializable
{
    [field:SerializeField] public Bucket Bucket { get; set; }
    [field:SerializeField] public int BallsPerAxis { get; set; } = 8;
    [field:SerializeField] public float BallRadius { get; set; } = 0.04f;
    [field:SerializeField] public Vector2 BallPadding { get; set; } = new(0.01f, 0.01f);
    [field:SerializeField] public float BallDensity { get; set; } = 1f;

    CircleGeometry _ballGeometry;
    readonly List<PhysicsBody> _ballBodies = new();

    public void InitializeStage(StageManager stage)
    {
        var innerSize = Bucket.BucketSize - new Vector2(Bucket.WallThickness * 2f, Bucket.WallThickness);
        var innerHalf = innerSize * 0.5f;
        var start = Bucket.BucketOrigin;
        var min = start + new Vector2(-innerHalf.x + BallRadius + BallPadding.x, -innerHalf.y + BallRadius + BallPadding.y);
        var step = new Vector2(
            (innerSize.x - (BallRadius + BallPadding.x) * 2f) / Mathf.Max(1, BallsPerAxis - 1),
            (innerSize.y - (BallRadius + BallPadding.y) * 2f) / Mathf.Max(1, BallsPerAxis - 1));

        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;

        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        shapeDef.density = BallDensity;
        _ballGeometry = new CircleGeometry { radius = BallRadius };

        for (var y = 0; y < BallsPerAxis; ++y)
        {
            for (var x = 0; x < BallsPerAxis; ++x)
            {
                bodyDef.position = min + new Vector2(step.x * x, step.y * y);
                var body = PhysicsWorld.defaultWorld.CreateBody(bodyDef);
                body.CreateShape(_ballGeometry, shapeDef);
                _ballBodies.Add(body);
            }
        }
    }

    void OnDestroy()
    {
        for (var i = 0; i < _ballBodies.Count; ++i)
        {
            var body = _ballBodies[i];
            if (body.isValid)
                body.Destroy();
        }

        _ballBodies.Clear();
    }

}
