using UnityEngine;

public class SpoutPositionProvider : MonoBehaviour
{
    public Vector2 GetPosition()
    {
        var center = (Vector2)transform.position;
        var halfSize = (Vector2)transform.lossyScale * 0.5f;
        var x = Random.Range(center.x - halfSize.x, center.x + halfSize.x);
        var y = Random.Range(center.y - halfSize.y, center.y + halfSize.y);
        return new Vector2(x, y);
    }
}
