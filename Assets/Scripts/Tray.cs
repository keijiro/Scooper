using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class Tray : MonoBehaviour, IStageInitializable
{
    [field:SerializeField] public Bucket Bucket { get; set; }
    [field:SerializeField] public Vector2 TraySize { get; set; } = new(2f, 0.4f);
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
    }
}
