using UnityEngine;
using UnityEngine.UIElements;

public class BucketController : MonoBehaviour
{
    [field:SerializeField] Bucket _bucket = null;
    [field:SerializeField] UIDocument _ui = null;

    void OnEnable()
    {
        var root = _ui.rootVisualElement;

        var openButton = root.Q<Button>("open-button");
        var closeButton = root.Q<Button>("close-button");

        openButton.clicked += () => _bucket.Open();
        closeButton.clicked += () => _bucket.Close();
    }
}
