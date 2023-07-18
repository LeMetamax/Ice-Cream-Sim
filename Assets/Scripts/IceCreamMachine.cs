using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

public class IceCreamMachine : MonoBehaviour
{
    [Header("Machine Parts")]
    [SerializeField, Tooltip("The visible model of this ice cream machine. This is what will be rotated around the ice cream cone in an orbital motion")]
    private Transform _base;
    [SerializeField, Tooltip("The part of the ice cream machine where the ice cream can be dispensed from")]
    private Transform _nozzleEjectPoint;

    [Header("Table Items")]
    [SerializeField, Tooltip("The cone in which to pour the ice cream. It also has a reference to the spline tool")]
    private IceCreamCone _iceCreamCone;

    [Header("Flavor Sticks")]
    [SerializeField, Tooltip("The strawberry flavored ice cream stick located over the ice cream machine. The pink one ;)")]
    private IceCreamFlavor _strawberryFlavor;
    [SerializeField, Tooltip("The chocolate flavored ice cream stick located over the ice cream machine. The dark brown one ;)")]
    private IceCreamFlavor _chocolateFlavor;
    [SerializeField, Tooltip("The pistachio flavored ice cream stick located over the ice cream machine. The greenish one ;)")]
    private IceCreamFlavor _pistachioFlavor;

    [Header("Operation Settings")]
    [SerializeField, Range(0.01f, 2f), Tooltip("How long(in seconds) it will take for any of the flavor sticks to rotate/bend to the degrees specified in '_activatedStickRotationAngle' when it's corresponding button is pressed down")]
    private float _stickActivationTime = 0.25f;
    [SerializeField, Range(1f, 135f), Tooltip("The value in degrees that any of the flavor sticks will rotate/bend towards")]
    private float _activatedStickRotationAngle = 30f;
    [SerializeField, Tooltip("How fast the ice cream machine will orbit around and over the ice cream cone")]
    private float _orbitSpeed = 0.05f;
    [SerializeField, Tooltip("How fast the ice cream will be dispensed over time")]
    private float _pourRate = 50f;
    [SerializeField, Tooltip("How fast the ice cream will fall into the ice cream cone from when it is dispensed")]
    private float _pieceFallDuration = 1.5f;

    [Header("Instantenous Objects")]
    [SerializeField, Tooltip("The little piece of ice cream that will be spawned '_pourRate' amount of time in a second")]
    private GameObject _iceCreamPiece;

    private float _nextPour = 0f;               //A value that is used to control how fast the ice cream will be dispensed. This is used along with _pour rate
    private float _timeStep = 0f;               //A time value that is calculated by increasing its current value by "_orbitSpeed" every second. This is used to dispense and properly place ice cream on the cup and also used to make the ice cream machine orbit the ice cream cone

    private Vector3 _initialPosition;           //The position of the ice cream machine at the first frame
    private IEnumerator _rotateMachineRoutine;      //A coroutine IEnumerator that is used to Start/Stop the RotateMachine coroutine

    public static IceCreamMachine Instance { get; private set; }        //A reference to this class itself, used to create a singleton

    private void Awake()
    {
        Instance = this;                        //Setup the singleton to assume that there is only one instance of this class present in the game's scene
        _initialPosition = _base.position;      //Set the position of the ice cream machine at the first frame
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="iceCreamType">What ice cream type/flavor are we operating on?</param>
    public void BeginPourIcecream(IceCreamType iceCreamType)
    {
        if (_iceCreamCone.IsFilled) return;
        RotateBaseStick(iceCreamType, false);       //Bend a flavor stick towards an angle specified in "_activatedStickRotationAngle"
        _rotateMachineRoutine = RotateMachine(iceCreamType, PourIceCream);          //Setup the coroutine that rotates and dispenses ice cream from the machine by giving it an ice cream flavor and an action to perform when it is ready to dispense ice cream
        StartCoroutine(_rotateMachineRoutine);              //Stop the coroutine that rotates and dispenses ice cream from the machine
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="iceCreamType">What ice cream type/flavor are we operating on?</param>
    public void StopPourIcecream(IceCreamType iceCreamType)
    {
        RotateBaseStick(iceCreamType, true);        //Return the flavor stick to an original rotation
        StopCoroutine(_rotateMachineRoutine);       //Stop the coroutine that rotates and dispenses ice cream from the machine
    }

    /// <summary>
    /// Use DOTween to rotate/bend one of the flavor stick to an angle specified in "_activatedStickRotationAngle" when pushing on a flavor's button, and return it back to its original state if the button is not pushed
    /// </summary>
    /// <param name="iceCreamType">What ice cream type/flavor are we operating on?</param>
    /// <param name="stop">Should we stop the rotation and return the flavor stick back to its original state?</param>
    private void RotateBaseStick(IceCreamType iceCreamType, bool stop)
    {
        Vector3 endRot = stop ? Vector3.zero : new(-_activatedStickRotationAngle, 0f, 0f);
        switch (iceCreamType)
        {
            case IceCreamType.Strawberry:
                _strawberryFlavor.transform.DOLocalRotate(endRot, _stickActivationTime);
                break;
            case IceCreamType.Chocolate:
                _chocolateFlavor.transform.DOLocalRotate(endRot, _stickActivationTime);
                break;
            case IceCreamType.Pistachio:
                _pistachioFlavor.transform.DOLocalRotate(endRot, _stickActivationTime);
                break;
        }
    }

    /// <summary>
    /// Begin to move the ice cream machine in an orbital motion over time until the ice cream cone is full
    /// </summary>
    /// <param name="iceCreamType">The ice cream type/flavor</param>
    /// <param name="onPourIcecream">An event triggered when the machine is ready to pour an ice cream</param>
    /// <returns></returns>
    private IEnumerator RotateMachine(IceCreamType iceCreamType, Action<IceCreamType> onPourIcecream)
    {
        while (true)        //This creates an infinite loop to allow the rest of its functionalities to be executed until when the coroutine is stopped. Because of this, we need the "yield return null" statement to allow the game move to the next frame
        {
            SplineContainer spline = GetSpline();           //Get a reference to the spline

            if (_timeStep >= 1f)                //Has the ice cream machine filled the cone with ice cream?
            {
                _base.position = Vector3.Lerp(_base.position, _initialPosition, 5 * Time.deltaTime);        //Set the ice cream machine back to its original/initial position when the game started
                _iceCreamCone.IsFilled = true;                    //Set the flag that the current cone is filled
                StopPourIcecream(iceCreamType);         //Stop pouring ice cream
            }

            onPourIcecream?.Invoke(iceCreamType);           //Invoke the event to notify any available listener that the machine is ready to dispense an ice cream

            Vector3 newPosition = spline.EvaluatePosition(_timeStep);       //Get a point on the spline using the calculated "_timeStep", this point will be the new position this ice cream machine will move to
            newPosition.y = _base.position.y;           //Maintain the vertical axis of the ice cream machine so that the machine will stay at the same height all the time
            _base.position = Vector3.Lerp(_base.position, newPosition, 5 * Time.deltaTime);         //Use unity's vector-linear interpolation function to smoothly move the ice cream machine to the new position

            _timeStep += _orbitSpeed * Time.deltaTime;      //Increase the value of "_timeStep" by "_orbitSpeed" over time

            yield return null;          //Wait for the next frame
        }
    }

    /// <summary>
    /// Get a reference to the spline tool given by the ice cream cone. When creating the customer logic, the ice cream cone is what should be edited to get the next cone. At this moment, it is setup to only allow just one cone to be filled
    /// </summary>
    /// <returns></returns>
    private SplineContainer GetSpline() => _iceCreamCone.Spline;

    /// <summary>
    /// Begin to spawn the ice cream itself. This is done by spawning multiple pieces of an ice cream many times in several frames to create the illusion of an ice cream being poured from its machine and into the cone
    /// </summary>
    /// <param name="iceCreamType"></param>
    private void PourIceCream(IceCreamType iceCreamType)
    {
        if (Time.time < _nextPour) return;              //This controls the rate at which a single ice cream piece is spawned. It does this by checking when the time at the begining of the current frame is greater than or exactly the time to spawn the next piece...before spawning that next piece
        _nextPour = Time.time + 1f / _pourRate;         //This updates the next time to spawn an ice cream piece by adding the inverse of the value in "_pourRate" to the time at the begining of the current frame

        Transform piece = Instantiate(_iceCreamPiece.transform, _nozzleEjectPoint.position, _nozzleEjectPoint.rotation);        //Spawn an ice cream piece at the point specified in "_nozzleEjectPoint", and get a reference to its transform component for future use
        PlacePiece(piece);                      //Use DOTween to place and orient the ice cream piece to a point on the spline
        UpdateFlavor(iceCreamType, piece);      //Assign a color/material to the ice cream piece
    }

    /// <summary>
    /// Use DOTween to place and orient the ice cream piece to a point on the spline
    /// </summary>
    /// <param name="piece">The spawned ice cream piece</param>
    private void PlacePiece(Transform piece)
    {
        SplineContainer spline = GetSpline();           //Get a reference to the spline

        piece.eulerAngles = new Vector3(-90f, 0f, 0f);              //Set the original rotation of the ice cream piece so that it can be tweened
        Quaternion placementRotationDir = Quaternion.LookRotation(spline.EvaluateTangent(_timeStep));       //Get the rotation direction to a point in the spline's tangent and store it as a quaternion
        piece.DORotateQuaternion(placementRotationDir, 3f);             //Use DOTween to orientt the ice cream piece along the curves of the spline
        piece.DOMove(spline.EvaluatePosition(_timeStep), _pieceFallDuration);       //Use DOTween to move the ice cream piece into a point on the spline
    }

    /// <summary>
    /// This assigns the ice cream with a flavor. Essentially by changing its material to the appropriate material based on the ice cream type/flavor
    /// </summary>
    /// <param name="iceCreamType">The ice cream flavor</param>
    /// <param name="piece">The piece of ice cream that was spawned</param>
    private void UpdateFlavor(IceCreamType iceCreamType, Transform piece)
    {
        if (!piece.GetChild(0).TryGetComponent(out MeshRenderer meshRenderer)) return;      //This makes sure that the piece of ice cream has a renderer so that its material can be updated
        Material mat = null;
        switch (iceCreamType)
        {
            case IceCreamType.Strawberry:
                mat = _strawberryFlavor.m_Material;
                break;
            case IceCreamType.Chocolate:
                mat = _chocolateFlavor.m_Material;
                break;
            case IceCreamType.Pistachio:
                mat = _pistachioFlavor.m_Material;
                break;
        }
        meshRenderer.material = mat;
    }
}