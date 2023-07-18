using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public enum IceCreamType
{
    None,
    Strawberry,
    Chocolate,
    Pistachio
}

[RequireComponent(typeof(Image))]
public class IceCreamSelector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [field: SerializeField, Tooltip("Set the ice cream type/flavor this ui object can send to the ice cream machine")]
    public IceCreamType m_IceCreamType { get; private set; }

    private IceCreamMachine _iceCreamMachine;           //A reference to the ice cream machine, the monobehavior class that is responsible for making any type of ice cream flavor this object gives it
    private static bool _isPressing;                    //A flag that determines if this UI object is being held on by a mouse left click or a single touch

    private void Start() => _iceCreamMachine = IceCreamMachine.Instance;        //Cache the reference to an instance of the ice cream machine

    /// <summary>
    /// Event called when the mouse left click or a finger is held down over this UI object(as long as it has the image component)
    /// </summary>
    /// <param name="eventData">Information from the click or touch input</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isPressing) return;        //This is done to prevent multiple buttons with this component attached from pressing and activating multiple ice cream flavors at the same time
        _iceCreamMachine.BeginPourIcecream(m_IceCreamType);         //Tell the ice cream machine to begin dispensing ice cream
        _isPressing = true;              //Notify the flag state that this button is being held down. This, along with the if statement 2 lines above will only allow this function's functionalities to be executed in one frame
        transform.DOScale(0.9f, 0.2f);      //Use DOTween to reduce the size of this UI object to 0.9 within the duration of 0.2 second
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _iceCreamMachine.StopPourIcecream(m_IceCreamType);          //Tell the ice cream machine to stop dispensing ice cream
        _isPressing = false;                 //Notify the flag state that this button is no longer being held down.
        transform.DOScale(1f, 0.2f);      //Use DOTween to increase the size of this UI object to 1 within the duration of 0.2 second
    }
}