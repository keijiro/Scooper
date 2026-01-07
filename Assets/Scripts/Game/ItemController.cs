using UnityEngine;

public sealed class ItemController : MonoBehaviour
{
    [field:SerializeField] public ItemType Type { get; set; }
}
