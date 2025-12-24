using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

[CreateAssetMenu(menuName = "Scooper/Dirt Body Definition")]
public class DirtBodyDefinition : ScriptableObject
{
    [field:SerializeField] public float Radius { get; set; } = 0.2f;
    [field:SerializeField] public int Sides { get; set; }
    [field:SerializeField] public float Density { get; set; } = 1f;

    public PhysicsBody CreateBody(PhysicsWorld world, PhysicsBodyDefinition bodyDefinition, Vector2 position)
    {
        bodyDefinition.position = position;
        var body = world.CreateBody(bodyDefinition);

        var shapeDefinition = PhysicsShapeDefinition.defaultDefinition;
        shapeDefinition.density = Density;

        if (Sides < 3)
        {
            var geometry = new CircleGeometry { radius = Radius };
            body.CreateShape(geometry, shapeDefinition);
        }
        else
        {
            var sides = Mathf.Clamp(Sides, 3, PhysicsConstants.MaxPolygonVertices);
            var vertices = new Vector2[sides];
            var step = Mathf.PI * 2f / sides;
            for (var i = 0; i < sides; ++i)
            {
                var angle = step * i;
                vertices[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Radius;
            }

            var geometry = PolygonGeometry.Create(vertices, 0f);
            body.CreateShape(geometry, shapeDefinition);
        }

        return body;
    }
}
