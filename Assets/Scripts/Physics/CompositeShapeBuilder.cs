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

    #region Private Helpers

    PhysicsWorld World => PhysicsWorld.defaultWorld;

    PolygonGeometry BuildRegularPolygonGeometry(in ShapeElement shape)
    {
        var geo = GeometryCache.GetRegularPolygon(shape.Sides);
        var scale = new Vector3(shape.Radius, shape.Radius, 1);
        return geo.Transform(Matrix4x4.Scale(scale), true);
    }

    #endregion

    #region Shape Creation

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
        var geo = new CircleGeometry { center = shape.Center, radius = shape.Radius };
        body.CreateShape(geo, def);
    }

    void CreatePolygon(PhysicsBody body, PhysicsShapeDefinition def, ShapeElement shape)
    {
        var rot = new PhysicsRotate(shape.Rotation * Mathf.Deg2Rad);
        var xform = new PhysicsTransform(shape.Center, rot);
        var geo = BuildRegularPolygonGeometry(shape).Transform(xform);
        body.CreateShape(geo, def);
    }

    void CreateBox(PhysicsBody body, PhysicsShapeDefinition def, ShapeElement shape)
    {
        var rot = new PhysicsRotate(shape.Rotation * Mathf.Deg2Rad);
        var xform = new PhysicsTransform(shape.Center, rot);
        var geo = PolygonGeometry.CreateBox(shape.Scale, 0, xform);
        body.CreateShape(geo, def);
    }

    #endregion

    #region Gizmos

    void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        var xform = PhysicsMath.ToPhysicsTransform
          (transform, World.transformPlane);

        foreach (var shape in _shapes) DrawShapeGizmo(xform, shape);
    }

    void DrawShapeGizmo(PhysicsTransform xform, ShapeElement shape)
    {
        switch (shape.Type)
        {
            case ShapeType.Circle: DrawCircleGizmo(xform, shape); break;
            case ShapeType.Polygon: DrawPolygonGizmo(xform, shape); break;
            case ShapeType.Box: DrawBoxGizmo(xform, shape); break;
        }
    }

    void DrawCircleGizmo(PhysicsTransform xform, ShapeElement shape)
    {
        var geo = new CircleGeometry{ center = shape.Center, radius = shape.Radius };
        World.DrawGeometry(geo, xform, Gizmos.color);
    }

    void DrawPolygonGizmo(PhysicsTransform xform, ShapeElement shape)
    {
        var rot = new PhysicsRotate(shape.Rotation * Mathf.Deg2Rad);
        xform = xform.MultiplyTransform(new PhysicsTransform(shape.Center, rot));
        var geo = BuildRegularPolygonGeometry(shape);
        World.DrawGeometry(geo, xform, Gizmos.color);
    }

    void DrawBoxGizmo(PhysicsTransform xform, ShapeElement shape)
    {
        var rot = new PhysicsRotate(shape.Rotation * Mathf.Deg2Rad);
        xform = xform.MultiplyTransform(new PhysicsTransform(shape.Center, rot));
        World.DrawBox(xform, shape.Scale, 0, Gizmos.color);
    }

    #endregion
}
