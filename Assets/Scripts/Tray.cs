using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class Tray : MonoBehaviour, IStageInitializable
{
    [field:SerializeField] public Bucket Bucket { get; set; }
    [field:SerializeField] public Vector2 TraySize { get; set; } = new(2f, 0.4f);
    [field:SerializeField] public float RimWidth { get; set; } = 0.3f;
    [field:SerializeField] public float RimHeight { get; set; } = 0.2f;
    [field:SerializeField] public float Gap { get; set; } = 0.1f;
    [field:SerializeField] public Vector2 TrayOffset { get; set; }

    public PhysicsBody TrayBody => _trayBody;

    PhysicsBody _trayBody;

    public void InitializeStage(StageManager stage)
      => CreateTray();

    void OnDestroy()
    {
        if (_trayBody.isValid)
            _trayBody.Destroy();
    }

    void CreateTray()
    {
        var bucketHalf = Bucket.BucketSize * 0.5f;
        var trayHalf = TraySize * 0.5f;
        var baseOffset = new Vector2(
            -bucketHalf.x - trayHalf.x - Gap,
            -bucketHalf.y + trayHalf.y);

        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Static;
        bodyDef.position = Bucket.BucketOrigin + baseOffset + TrayOffset;
        _trayBody = PhysicsWorld.defaultWorld.CreateBody(bodyDef);

        var shapeDefinition = PhysicsShapeDefinition.defaultDefinition;
        shapeDefinition.contactFilter = new PhysicsShape.ContactFilter
          (Categories.Tray, new PhysicsMask(Categories.Gem, Categories.Bomb), 0);

        var geometry = PolygonGeometry.CreateBox(TraySize, 0f);
        _trayBody.CreateShape(geometry, shapeDefinition);

        CreateRims(shapeDefinition);
    }

    void CreateRims(PhysicsShapeDefinition shapeDefinition)
    {
        if (RimWidth <= 0f || RimHeight <= 0f)
            return;

        var halfSize = TraySize * 0.5f;
        var topY = halfSize.y;
        var width = Mathf.Min(RimWidth, TraySize.x);

        var leftVertices = new[]
        {
            new Vector2(-halfSize.x, topY),
            new Vector2(-halfSize.x + width, topY),
            new Vector2(-halfSize.x, topY + RimHeight)
        };

        var rightVertices = new[]
        {
            new Vector2(halfSize.x, topY),
            new Vector2(halfSize.x - width, topY),
            new Vector2(halfSize.x, topY + RimHeight)
        };

        var leftRim = PolygonGeometry.Create(leftVertices, 0f);
        var rightRim = PolygonGeometry.Create(rightVertices, 0f);
        _trayBody.CreateShape(leftRim, shapeDefinition);
        _trayBody.CreateShape(rightRim, shapeDefinition);
    }
}
