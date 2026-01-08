using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class CompositeShapeBuilder : MonoBehaviour
{
    #region Types

    public enum ShapeType { Circle, Polygon, Box }

    [System.Serializable]
    public struct ShapeElement
    {
        public ShapeType Type;
        public Vector2 Center;
        public float Rotation;
        public float Radius;
        public int Sides;
        public Vector2 Scale;
    }

    #endregion

    #region Editable Fields

    [SerializeField] ShapeElement[] _shapes = null;

    #endregion

    #region Public Methods

    public void CreateShapes(PhysicsBody body)
      => CreateShapes(body, PhysicsShapeDefinition.defaultDefinition);

    public void CreateShapes(PhysicsBody body, PhysicsShapeDefinition def)
    {
        foreach (var shape in _shapes) CreateShape(body, def, shape);
    }

    #endregion

    #region Shape Creation

    const int MaxPolygonSides = 10;

    void CreateShape(PhysicsBody body, PhysicsShapeDefinition def, ShapeElement shape)
    {
        switch (shape.Type)
        {
            case ShapeType.Circle: CreateCircle(body, def, shape); break;
            case ShapeType.Polygon: CreatePolygon(body, def, shape); break;
            case ShapeType.Box: CreateBox(body, def, shape); break;
        }
    }

    void CreateCircle(PhysicsBody body, PhysicsShapeDefinition def, ShapeElement shape)
    {
        var circle = new CircleGeometry { center = shape.Center, radius = shape.Radius };
        body.CreateShape(circle, def);
    }

    void CreatePolygon(PhysicsBody body, PhysicsShapeDefinition def, ShapeElement shape)
    {
        var geo = BuildRegularPolygonGeometry(shape);
        var rot = new PhysicsRotate(shape.Rotation * Mathf.Deg2Rad);
        var xform = new PhysicsTransform(shape.Center, rot);
        body.CreateShape(geo.Transform(xform), def);
    }

    void CreateBox(PhysicsBody body, PhysicsShapeDefinition def, ShapeElement shape)
    {
        var rot = new PhysicsRotate(shape.Rotation * Mathf.Deg2Rad);
        var xform = new PhysicsTransform(shape.Center, rot);
        body.CreateShape(PolygonGeometry.CreateBox(shape.Scale, 0, xform), def);
    }

    #endregion

    #region Gizmos

    void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        var world = PhysicsWorld.defaultWorld;
        var baseXform = PhysicsMath.ToPhysicsTransform(transform, world.transformPlane);
        var color = Gizmos.color;

        foreach (var shape in _shapes)
            DrawShapeGizmo(world, baseXform, color, shape);
    }

    void DrawShapeGizmo(
        PhysicsWorld world,
        PhysicsTransform baseXform,
        Color color,
        ShapeElement element
    )
    {
        switch (element.Type)
        {
            case ShapeType.Circle: DrawCircleGizmo(world, baseXform, color, element); break;
            case ShapeType.Polygon: DrawPolygonGizmo(world, baseXform, color, element); break;
            case ShapeType.Box: DrawBoxGizmo(world, baseXform, color, element); break;
        }
    }

    void DrawCircleGizmo(
        PhysicsWorld world,
        PhysicsTransform baseXform,
        Color color,
        ShapeElement element
    )
    {
        var localTransform = new PhysicsTransform(
            element.Center,
            new PhysicsRotate(element.Rotation * Mathf.Deg2Rad)
        );
        var worldTransform = baseXform.MultiplyTransform(localTransform);
        var circle = new CircleGeometry { radius = element.Radius };
        world.DrawGeometry(circle, worldTransform, color);
    }

    void DrawBoxGizmo(
        PhysicsWorld world,
        PhysicsTransform baseXform,
        Color color,
        ShapeElement element
    )
    {
        var localTransform = new PhysicsTransform(
            element.Center,
            new PhysicsRotate(element.Rotation * Mathf.Deg2Rad)
        );
        var worldTransform = baseXform.MultiplyTransform(localTransform);
        var size = new Vector2(Mathf.Abs(element.Scale.x), Mathf.Abs(element.Scale.y));
        world.DrawBox(worldTransform, size, 0f, color);
    }

    void DrawPolygonGizmo(
        PhysicsWorld world,
        PhysicsTransform baseXform,
        Color color,
        ShapeElement element
    )
    {
        var geometry = BuildRegularPolygonGeometry(element);
        var localTransform = new PhysicsTransform(
            element.Center,
            new PhysicsRotate(element.Rotation * Mathf.Deg2Rad)
        );
        var worldTransform = baseXform.MultiplyTransform(localTransform);
        world.DrawGeometry(geometry, worldTransform, color);
    }

    PolygonGeometry BuildRegularPolygonGeometry(ShapeElement element)
    {
        var count = Mathf.Clamp(element.Sides, 3, MaxPolygonSides);
        var vertices = new Vector2[count];
        for (var i = 0; i < count; ++i)
        {
            var r = (360f * i / count) * Mathf.Deg2Rad;
            vertices[i] = new Vector2(Mathf.Cos(r), Mathf.Sin(r)) * element.Radius;
        }
        return PolygonGeometry.Create(vertices, 0);
    }

    #endregion
}
