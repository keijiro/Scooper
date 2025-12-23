using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class Scoop : MonoBehaviour, IStageInitializable
{
    [field:SerializeField] public Vector2 ScoopSize { get; set; } = new(0.7f, 0.25f);
    [field:SerializeField] public float WallThickness { get; set; } = 0.04f;
    [field:SerializeField] public float HandleLength { get; set; } = 0.35f;
    [field:SerializeField] public Vector2 SpawnOffset { get; set; } = new(0f, 0.6f);
    [field:SerializeField] public float ScoopDensity { get; set; } = 1f;

    public PhysicsBody ScoopBody => _scoopBody;
    public Vector2 TipLocal => new(-ScoopSize.x * 0.5f, 0f);
    public Vector2 BaseLocal => new(ScoopSize.x * 0.5f, 0f);
    public Vector2 HandleTipLocal => new(ScoopSize.x * 0.5f + HandleLength, 0f);

    PhysicsBody _scoopBody;
    (PolygonGeometry bottom, PolygonGeometry left, PolygonGeometry right, PolygonGeometry handle) _scoopGeometry;

    public void InitializeStage(StageManager stage)
      => CreateScoop();

    void OnDestroy()
    {
        if (_scoopBody.isValid)
            _scoopBody.Destroy();
    }

    void CreateScoop()
    {
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;
        bodyDef.position = (Vector2)transform.position + SpawnOffset;

        _scoopBody = PhysicsWorld.defaultWorld.CreateBody(bodyDef);

        var size = ScoopSize;
        var half = size * 0.5f;
        var thickness = WallThickness;

        var bottomSize = new Vector2(size.x, thickness);
        var sideSize = new Vector2(thickness, size.y);
        var handleSize = new Vector2(HandleLength, thickness);

        var xformBottom = new PhysicsTransform(new Vector2(0f, -half.y + thickness * 0.5f));
        var xformLeft = new PhysicsTransform(new Vector2(-half.x + thickness * 0.5f, 0f));
        var xformRight = new PhysicsTransform(new Vector2(half.x - thickness * 0.5f, 0f));
        var xformHandle = new PhysicsTransform(new Vector2(half.x + HandleLength * 0.5f, 0f));

        _scoopGeometry.bottom = PolygonGeometry.CreateBox(bottomSize, 0f, xformBottom);
        _scoopGeometry.left = PolygonGeometry.CreateBox(sideSize, 0f, xformLeft);
        _scoopGeometry.right = PolygonGeometry.CreateBox(sideSize, 0f, xformRight);
        _scoopGeometry.handle = PolygonGeometry.CreateBox(handleSize, 0f, xformHandle);

        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        shapeDef.density = ScoopDensity;

        _scoopBody.CreateShape(_scoopGeometry.bottom, shapeDef);
        _scoopBody.CreateShape(_scoopGeometry.left, shapeDef);
        _scoopBody.CreateShape(_scoopGeometry.right, shapeDef);
        _scoopBody.CreateShape(_scoopGeometry.handle, shapeDef);
    }
}
