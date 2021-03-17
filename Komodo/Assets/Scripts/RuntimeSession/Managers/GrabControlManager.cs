using Komodo.Runtime;
using Komodo.Utilities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GrabControlManager : SingletonComponent<GrabControlManager>, IUpdatable
{
    public static GrabControlManager Instance
    {
        get { return ((GrabControlManager)_Instance); }
        set { _Instance = value; }
    }
    
    //check for left hand grabbing
   [ShowOnly] public Transform firstObjectGrabbed;
    //get parent if we are switching objects between hands we want to keep track of were to place it back, to avoid hierachy parenting displacement
    [ShowOnly] public Transform originalParentOfFirstHandTransform;


    //do the same as above for the right Hand
    [ShowOnly] public Transform secondObjectGrabbed;
    [ShowOnly] public Transform originalParentOfSecondHandTransform;

    [ShowOnly] public Transform[] hands = new Transform[2];//(2, Allocator.Persistent);

    //away to keep track if we are double grabbing a gameobject to call event;
    public UnityEvent onDoubleAssetGrab;
    public UnityEvent onDoubleAssetRelease;

    // public bool isDoubleGrabbing;

    //Fields to rotate object appropriately
    //Hierarchy used to set correct Pivot points for scalling and rotating objects on DoubleGrab
    [ShowOnly] public Transform firstGrabPoint;             ///PARENT SCALE PIVOT1 CONTAINER
    private static Transform secondGrabPoint;             ///-CHILD SCALE PIVOT2 CONTAINER
    [ShowOnly] public Transform grabMidpoint;       //--Child for rotations

    [ShowOnly] public Transform handParent;

    //coordinate system to use to tilt double grand object appropriately: pulling, pushing, hand lift, and hand lower
    [ShowOnly] public Transform initialCoordinateSystem;

    //initial Data when Double Grabbing -scalling and rotation 
    [ShowOnly] public bool didStartStretching;
    private Quaternion initialRotation;
    private float initialDistance;
    private Vector3 initialScale;
    private Vector3 initialOffsetFromHandToGrabbedObject;
    private Quaternion initialPlayerRotation;
    private float initialScaleRatioBasedOnDistance;
    float initialZCoord;
    float initialYCoord;

    /* DEBUG STUFF */
    public GameObject axesPrefab;
    public GameObject initialTransformDisplay;
    public GameObject currentTransformDisplay;

    private GameObject objectPoseDisplay;

    private Quaternion initialRotationOffset; //the amount it would take to rotate from the current midpoint's rotation to the object's rotation

    public void Awake()
    {
        //used to set our managers alive state to true to detect if it exist within scene
        var initManager = Instance;

        //register our calls
        onDoubleAssetGrab.AddListener(()=>DoubleAssetGrab());
        onDoubleAssetRelease.AddListener(() => DoubleAssetRelease());

        //create hierarchy to rotate double grab objects appropriately
        //create root parent and share it through scripts by setting it to a static field
        var firstGrabPointDisplay = CreateDisplay("FirstGrabPoint");
        firstGrabPoint = firstGrabPointDisplay.transform;
        ////place object one level up from hand to avoid getting our hand rotations
        firstGrabPoint.parent = transform.parent;

        //construct coordinate system to reference for tilting double grab object 
        var initialCoordinateSystemDisplay = CreateDisplay("InitialCoordinateSystem");
        initialCoordinateSystem = initialCoordinateSystemDisplay.transform;
        initialCoordinateSystem.SetParent(transform.root.parent, true);
        initialCoordinateSystem.localPosition = Vector3.zero;

        var secondGrabPointDisplay = CreateDisplay("SecondGrabPoint");
        secondGrabPoint = secondGrabPointDisplay.transform;
        secondGrabPoint.SetParent(firstGrabPoint, true);
        secondGrabPoint.localPosition = Vector3.zero;

        var grabMidPointDisplay = CreateDisplay("GrabMidPoint");
        grabMidpoint = grabMidPointDisplay.transform;
        grabMidpoint.SetParent(secondGrabPoint, true);
        grabMidpoint.localPosition = Vector3.zero;

        objectPoseDisplay = CreateDisplay("ObjectPose");
    }

    public GameObject CreateDisplay (string name) {
        var doDebug = true;
        if (doDebug)
        {
            var result = Instantiate(axesPrefab);
            result.name = name;
            return result;
        }

        return new GameObject(name);
    }

    public void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");

        if (player)
        {
           if(player.TryGetComponent(out PlayerReferences pR))
            {
                hands[0] = pR.handL;
                hands[1] = pR.handR;
            }
        }

        //set references for parent
        handParent = firstGrabPoint.parent;
    }

    //this is used for only running our update loop when necessary
    private void DoubleAssetGrab()
    {
        //register our update loop to be called
        if (GameStateManager.IsAlive)
        {
            GameStateManager.Instance.RegisterUpdatableObject(this);
        }
    }

    private void DoubleAssetRelease()
    {
        if (GameStateManager.IsAlive)
        {
            GameStateManager.Instance.DeRegisterUpdatableObject(this);
        }
    }

    public void OnUpdate(float realltime)
    {
        if (didStartStretching == false)
        {
            InitializeStretchValues();

            InitializeStretchValues2();

            didStartStretching = true;

            return;
        }

        //UpdateScale();
        
        //place our grabbed object and second pivot away from the influeces of scale and rotation at first
        firstObjectGrabbed.SetParent(handParent, true);
        secondGrabPoint.SetParent(handParent, true);

        UpdateGrabPoints();
        
        //set our second pivot as a child of first to have a pivot for each hands
        secondGrabPoint.SetParent(firstGrabPoint, true);

        //set it to parent to modify rotation
        firstObjectGrabbed.SetParent(grabMidpoint, true);

        //UpdateRotation();
        UpdateRotation2();

        //UpdatePosition();
    }

    void InitializeStretchValues () {                
        initialDistance = Vector3.Distance(hands[0].transform.position, hands[1].transform.position);

        initialScale = firstGrabPoint.localScale;

        secondGrabPoint.rotation = handParent.rotation;

        //reset values for our container objects that we use to deform and rotate objects
        grabMidpoint.rotation = Quaternion.identity;

        firstGrabPoint.localScale = Vector3.one;

        //set reference vector to tilt our grabed object on - left hand looks at right and sets tilt according to movement of origin or lookat target 
        initialCoordinateSystem.LookAt((hands[1].transform.position - hands[0].transform.position), Vector3.up);

        //Get the inverse of the initial rotation to use in update loop to avoid moving the object when grabbing   
        initialRotation = Quaternion.Inverse(initialCoordinateSystem.rotation * handParent.rotation);

        //get rotational difference to be able to offset it apropriately in update loop
        var tiltRotation = initialRotation * initialCoordinateSystem.rotation;

        //our initial orientation to use to tilt object, due to the way lookat behavior behaves we have to set x as Z 
        initialZCoord = tiltRotation.eulerAngles.x - grabMidpoint.transform.eulerAngles.x;
        initialYCoord = tiltRotation.eulerAngles.y - grabMidpoint.transform.eulerAngles.y;

        ////to fix parenting scalling down issue between centerpoint of hands and object
        initialOffsetFromHandToGrabbedObject = firstObjectGrabbed.position - ((hands[1].transform.position + hands[0].transform.position) / 2);// - handParentForContainerPlacement.position;

        //pick up the rotation of our client to know when to update our offsets from hands to grab object
        initialPlayerRotation = handParent.rotation;
    }

    void InitializeStretchValues2 ()
    {
        objectPoseDisplay.transform.SetParent(firstObjectGrabbed);
        objectPoseDisplay.transform.localPosition = Vector3.zero;

        //rotate the midpoint so its forward vector's endpoints are the hands.
        //it is already on the line drawn between the hands, so we just need
        //to look at one of the endpoints.
        grabMidpoint.LookAt(secondGrabPoint.position);

        initialRotationOffset = Quaternion.FromToRotation(grabMidpoint.forward, firstObjectGrabbed.forward);
    }

    void UpdateRotation2 () 
    {
        grabMidpoint.LookAt(secondGrabPoint.position);

        //firstObjectGrabbed.rotation = initialRotationOffset * grabMidpoint.rotation;
    }

    void UpdateGrabPoints () {
        //SET PIVOT Location through our parents
        firstGrabPoint.position = hands[1].transform.position;
        
        secondGrabPoint.position = hands[0].transform.position;
        

        //place position of rotations to be in the center of both hands to rotate according to center point of hands not object center
        grabMidpoint.position = ((hands[1].transform.position + hands[0].transform.position) / 2);
    }

    void UpdateScale () {
        //a ratio between our current distance divided by our initial distance
        var currentScaleRatio = GetCurrentScaleRatio();

        if (float.IsNaN(firstObjectGrabbed.localScale.y)) {
            Debug.LogError("First Object Grabbed's' local scale was NaN");
        }

        //we multiply our ratio with our initial scale
        firstGrabPoint.localScale = initialScale * currentScaleRatio;
    }

    float GetCurrentScaleRatio () {
        return Vector3.Distance(hands[0].transform.position, hands[1].transform.position) / initialDistance;
    }

    void UpdatePosition () {
        var currentScaleRatio = GetCurrentScaleRatio();

        //modify object spacing offset when scalling using ratio between initial scale and currentscale
        firstObjectGrabbed.position = ((hands[1].transform.position + hands[0].transform.position) / 2) + (initialOffsetFromHandToGrabbedObject * currentScaleRatio);
    }

    void UpdateRotation () {
        // provides how an object should behave when double grabbing, object looks at one hand point of hand
        initialCoordinateSystem.LookAt((hands[1].transform.position - hands[0].transform.position), Vector3.up);

        var currentScaleRatio = GetCurrentScaleRatio();

        //offset our current rotation from our initial difference to set
        var lookRot = initialRotation * initialCoordinateSystem.rotation;

        //rotate y -> Yaw bring/push objects by pulling or pushing hand towards 
        var quat3 = Quaternion.AngleAxis(FreeFlightController.ClampAngle(lookRot.eulerAngles.y - initialYCoord, -360, 360), initialCoordinateSystem.up);
        //rotate z -> Roll shift objects right and left by lifting and lowering hands 
        var quat4 = Quaternion.AngleAxis(FreeFlightController.ClampAngle(initialZCoord - lookRot.eulerAngles.x, -360, 360), -initialCoordinateSystem.right);

        //add our rotatations
        grabMidpoint.rotation = quat3 * quat4;// Quaternion.RotateTowards(doubleGrabRotationTransform.rotation, quat3 * quat4,60);// * handParentForContainerPlacement.rotation;

        //check for shifting of our player rotation to adjust our offset to prevent us from accumulating offsets that separates our grabbed object from hand
        if (handParent.eulerAngles.y != initialPlayerRotation.eulerAngles.y)
        {
            initialPlayerRotation = handParent.rotation;
            initialOffsetFromHandToGrabbedObject = (firstObjectGrabbed.position) - ((hands[1].transform.position + hands[0].transform.position) / 2);
            initialOffsetFromHandToGrabbedObject /= currentScaleRatio;
        }
    }
}
