using UnityEngine;

public class IceCreamFlavor : MonoBehaviour
{
    /// <summary>
    /// The material attached to the mesh renderer of the child component of this gameobject
    /// </summary>
    public Material m_Material { get; private set; }
    private void Awake()
    {
        if (transform.GetChild(0).TryGetComponent(out MeshRenderer meshRenderer))
            m_Material = meshRenderer.material;
    }
}