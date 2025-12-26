using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class CompositeShapeBuilder : MonoBehaviour
{
    #region Types

    public enum ShapeType
    {
        Circle,
        RegularPolygon,
        ScaledBox
    }

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

    [SerializeField] ShapeElement[] _shapes = new ShapeElement[0];

    #endregion

    #region Public Methods

    public void CreateShapes(PhysicsBody body)
      => CreateShapes(body, PhysicsShapeDefinition.defaultDefinition);

    public void CreateShapes(PhysicsBody body, PhysicsShapeDefinition definition)
    {
        for (var i = 0; i < _shapes.Length; ++i)
            CreateShape(body, definition, _shapes[i]);
    }

    #endregion

    #region Shape Creation

    void CreateShape(PhysicsBody body, PhysicsShapeDefinition definition, ShapeElement element)
    {
        switch (element.Type)
        {
            case ShapeType.Circle:
                var circle = new CircleGeometry
                {
                    center = element.Center,
                    radius = element.Radius
                };
                body.CreateShape(circle, definition);
                break;

            case ShapeType.RegularPolygon:
                var sides = Mathf.Clamp(element.Sides, 3, PhysicsConstants.MaxPolygonVertices);
                var vertices = new Vector2[sides];
                var step = Mathf.PI * 2f / sides;
                var rotation = element.Rotation * Mathf.Deg2Rad;
                for (var i = 0; i < sides; ++i)
                {
                    var angle = step * i;
                    var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * element.Radius;
                    vertices[i] = element.Center + RotateVector(offset, rotation);
                }
                body.CreateShape(PolygonGeometry.Create(vertices, 0f), definition);
                break;

            case ShapeType.ScaledBox:
                var size = new Vector2(Mathf.Abs(element.Scale.x), Mathf.Abs(element.Scale.y));
                var xform = new PhysicsTransform(
                    element.Center,
                    new PhysicsRotate(element.Rotation * Mathf.Deg2Rad));
                body.CreateShape(PolygonGeometry.CreateBox(size, 0f, xform), definition);
                break;
        }
    }

    #endregion

    #region Gizmos

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        var prevMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        for (var i = 0; i < _shapes.Length; ++i)
            DrawShapeGizmo(_shapes[i]);

        Gizmos.matrix = prevMatrix;
    }

    void DrawShapeGizmo(ShapeElement element)
    {
        switch (element.Type)
        {
            case ShapeType.Circle:
                Gizmos.DrawWireSphere(element.Center, element.Radius);
                break;

            case ShapeType.RegularPolygon:
                DrawPolygonGizmo(element.Center, element.Radius, element.Sides, element.Rotation);
                break;

            case ShapeType.ScaledBox:
                var size = new Vector3(
                    Mathf.Abs(element.Scale.x),
                    Mathf.Abs(element.Scale.y),
                    0f);
                var prevMatrix = Gizmos.matrix;
                Gizmos.matrix = Gizmos.matrix * Matrix4x4.TRS(
                    element.Center,
                    Quaternion.Euler(0f, 0f, element.Rotation),
                    Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, size);
                Gizmos.matrix = prevMatrix;
                break;
        }
    }

    void DrawPolygonGizmo(Vector2 center, float radius, int sides, float rotation)
    {
        var count = Mathf.Clamp(sides, 3, PhysicsConstants.MaxPolygonVertices);
        var step = Mathf.PI * 2f / count;
        var rot = rotation * Mathf.Deg2Rad;
        var prev = center + RotateVector(new Vector2(Mathf.Cos(0f), Mathf.Sin(0f)) * radius, rot);

        for (var i = 1; i <= count; ++i)
        {
            var angle = step * i;
            var next = center + RotateVector(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, rot);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }

    Vector2 RotateVector(Vector2 v, float radians)
    {
        var sin = Mathf.Sin(radians);
        var cos = Mathf.Cos(radians);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos);
    }

    #endregion
}
