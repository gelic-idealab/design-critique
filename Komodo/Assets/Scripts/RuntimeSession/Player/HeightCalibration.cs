using UnityEngine;
using UnityEngine.Events;


namespace Komodo.Runtime
{

    [System.Serializable]
    public class UnityEvent_Float : UnityEvent<float> 
    {
        // this empty declaration is all that's needed.
    }

    [System.Serializable]
    public class UnityEvent_Vector3 : UnityEvent<Vector3>
    { 
        // this empty declaration is all that's needed.
    }

    public class HeightCalibration : MonoBehaviour
    {        
        public GameObject leftHand;

        public GameObject rightHand;

        public LayerMask layerMask;

        public UnityEvent onStartedCalibration;

        public UnityEvent_Vector3 onCalibrationUpdate;

        public UnityEvent_Float onFinishedCalibration;

        public UnityEvent_Float onBumpHeightUp;

        public UnityEvent_Float onBumpHeightDown;

        public float bumpAmount = 0.2f; //meters

        private Transform xrPlayer;

        private Vector3 floorHeightDisplayCenter;

        private bool isCalibratingHeight = false;

        private float minYOfHands;

        public void Awake () 
        {

        }

        public void Start () 
        {
            //Get xr player to change position
            if (!xrPlayer) 
            {
                xrPlayer = GameObject.FindGameObjectWithTag("XRCamera").transform;
            }

            minYOfHands = leftHand.transform.position.y;

            floorHeightDisplayCenter = new Vector3(xrPlayer.position.x, minYOfHands, xrPlayer.position.z);
        }

        public void Update ()
        {
            //TODO delete whole function

            if (Input.GetKeyDown(KeyCode.H) && !isCalibratingHeight) 
            {
                StartCalibration();

                return;
            }

            if (Input.GetKeyDown(KeyCode.H) && isCalibratingHeight)
            {
                
                EndCalibration();

                return;
            }

            if (isCalibratingHeight) {
                minYOfHands = GetMinimumYPositionOfHands(leftHand, rightHand);

                floorHeightDisplayCenter.x = xrPlayer.position.x;
                floorHeightDisplayCenter.y = minYOfHands;
                floorHeightDisplayCenter.z = xrPlayer.position.z;

                onCalibrationUpdate.Invoke(floorHeightDisplayCenter);
            }
        }
        

        public void BumpHeightUp () 
        {
            onBumpHeightUp.Invoke(bumpAmount);
        }

        public void BumpHeightDown ()
        {
            onBumpHeightDown.Invoke(bumpAmount);
        }

        public void StartCalibration () 
        {
            Debug.Log("Beginning player height calibration.");

            ShowHeightCalibrationSafetyWarning();

            bool useKnee = OfferKneeBasedHeightCalibration();

            if (useKnee) 
            {
                return;
            }

            isCalibratingHeight = true;

            onStartedCalibration.Invoke();
        }

        public void ShowHeightCalibrationSafetyWarning ()
        {
            //TODO implement
        }

        public bool OfferKneeBasedHeightCalibration ()
        {
            return false; //TODO -- add option so user doesn't have to bend down to reach the floor
        }

        public void EndCalibration ()
        {
            if (!isCalibratingHeight)
            {
                return;
            }

            Debug.Log("Ending player height calibration");

            var handHeight = minYOfHands;

            var terrainHeight = ComputeGlobalYPositionOfTerrainBelowPlayer();

            var newHeightOffset = terrainHeight - handHeight;

            Debug.Log($"terrain height: {terrainHeight} / handHeight: {handHeight} / newOffset: {newHeightOffset}");

            onFinishedCalibration.Invoke(newHeightOffset);

            minYOfHands = float.MaxValue;

            isCalibratingHeight = false;
        }

        public float ComputeGlobalYPositionOfTerrainBelowPlayer ()
        {
            float globalHeight = 10f;

            if (Physics.Raycast(xrPlayer.position, Vector3.down, out RaycastHit downHitInfo, layerMask))
            {
                return downHitInfo.point.y;
            }
            
            Debug.LogWarning($"Could not find terrain below player. Trying to find it from {globalHeight}m above the player.");

            var bumpedPlayerPosition = xrPlayer.position;

            bumpedPlayerPosition.y += globalHeight;
            
            if (Physics.Raycast(bumpedPlayerPosition, Vector3.down, out RaycastHit downFromAboveHitInfo, layerMask))
            {
                return downFromAboveHitInfo.point.y;
            }

            Debug.LogError($"Could now find terrain below player or below  {globalHeight}m above the player. Make sure your layer mask is valid and that there are objects on that layer. Proceeding anyways and returning '0' for the height offset.");

            return 0.0f;
        }

        public float GetGlobalYPositionOfHand (GameObject hand) 
        {
            return hand.transform.position.y;
        }

        public float GetMinimumYPositionOfHands (GameObject handL, GameObject handR) 
        {
            var curLeftY = handL.transform.position.y;

            var curRightY = handR.transform.position.y;

            if (curLeftY < curRightY && curLeftY < minYOfHands) 
            {
                return curLeftY;
            }

            if (curRightY < curLeftY && curRightY < minYOfHands)
            {
                return curRightY;
            }

            return minYOfHands;
        }

        public float GetGlobalYPositionOfHead (GameObject head)
        {
            return head.transform.position.y;
        }
    }
}