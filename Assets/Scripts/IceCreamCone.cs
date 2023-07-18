using UnityEngine;
using UnityEngine.Splines;

public class IceCreamCone : MonoBehaviour
{
    /// <summary>
    /// The spline component attached to the child game object of this gameobject
    /// </summary>
    public SplineContainer Spline { get; private set; }
    /// <summary>
    /// A flag that shows when/if the ice cream cone is filled with ice cream
    /// </summary>
    public bool IsFilled { get; set; }

    private void Awake() => Spline = transform.GetChild(0).GetComponent<SplineContainer>();
}