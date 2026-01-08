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
        foreach (var shape in _shapes)
        {
            switch (shape.Type)
            {
                case ShapeType.Circle:  body.CreateShape(CreateCircle (shape), def); break;
                case ShapeType.Polygon: body.CreateShape(CreatePolygon(shape), def); break;
                case ShapeType.Box:     body.CreateShape(CreateBox    (shape), def); break;
            }
        }
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

    CircleGeometry CreateCircle(in ShapeElement shape)
      => new CircleGeometry { center = shape.Center, radius = shape.Radius };

    PolygonGeometry CreatePolygon(in ShapeElement shape)
    {
        var rot = new PhysicsRotate(shape.Rotation * Mathf.Deg2Rad);
        var xform = new PhysicsTransform(shape.Center, rot);
        return BuildRegularPolygonGeometry(shape).Transform(xform);
    }

    PolygonGeometry CreateBox(ShapeElement shape)
    {
        var rot = new PhysicsRotate(shape.Rotation * Mathf.Deg2Rad);
        var xform = new PhysicsTransform(shape.Center, rot);
        return PolygonGeometry.CreateBox(shape.Scale, 0, xform);
    }

    void DrawGeometry(CircleGeometry geo)
    {
        var xform = PhysicsMath.ToPhysicsTransform(transform, World.transformPlane);
        World.DrawGeometry(geo, xform, Gizmos.color);
    }

    void DrawGeometry(PolygonGeometry geo)
    {
        var xform = PhysicsMath.ToPhysicsTransform(transform, World.transformPlane);
        World.DrawGeometry(geo, xform, Gizmos.color);
    }

    #endregion

    #region Gizmos

    void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        foreach (var shape in _shapes)
        {
            switch (shape.Type)
            {
                case ShapeType.Circle:  DrawGeometry(CreateCircle (shape)); break;
                case ShapeType.Polygon: DrawGeometry(CreatePolygon(shape)); break;
                case ShapeType.Box:     DrawGeometry(CreateBox    (shape)); break;
            }
        }
    }

    #endregion
}
