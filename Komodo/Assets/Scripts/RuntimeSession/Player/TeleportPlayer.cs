using System.Collections.Generic;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine;

namespace Komodo.Runtime
{
    /// <summary>
    /// Funcions to move our avatar
    /// </summary>
    [RequireComponent(typeof(CameraOffset))]
    public class TeleportPlayer : MonoBehaviour
    {
        public bool useManualHeightOffset = false;

        public string worldCenterTagName = "WorldCenter";

        private Transform worldCenter;

        private Transform cameraRootTransform;

        //move desktopPlayer
        private Transform desktopCameraTransform;
        //move xrPlayer
        private Transform xrPlayer;

        public CameraOffset cameraOffset;

        [UnityEngine.Serialization.FormerlySerializedAs("lRToAdjustWidth")]
        public List<LineRenderer> lineRenderersToScaleWithPlayer;

        float originalHeight;

        float originalFixedDeltaTime;

        public void Start()
        {
            originalFixedDeltaTime = Time.fixedDeltaTime;

            originalHeight = cameraOffset.cameraYOffset;

            currentScale = 1;

            SetWorldCenter();
        }
        
        public void Awake()
        {
            if (!cameraRootTransform) 
            {
                //get child to transform, we keep the webxrcameraset at origin
                cameraRootTransform = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0);
            }
            
            //Get xr player to change position
            if (!xrPlayer) 
            {
                xrPlayer = GameObject.FindGameObjectWithTag("XRCamera").transform;
            }
            
            if (!desktopCameraTransform)
            {
                desktopCameraTransform = GameObject.FindGameObjectWithTag("DesktopCamera").transform;
            }
        }

        public void TempTest ()
        {
            List<GameObject> allObjects = new List<GameObject>();
            UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            Debug.Log($"world {activeScene.name}");
            activeScene.GetRootGameObjects( allObjects );
            foreach(GameObject obj in allObjects) {
                if(obj.tag == "WorldCenter") {
                    Debug.Log($"World {obj.name}");
                }
            }
        }

        public void SetWorldCenter ()
        {
            TempTest();

            var worldCenters = GameObject.FindGameObjectsWithTag(worldCenterTagName);

            for (int i = 0; i < worldCenters.Length; i += 1) {
                //Debug.Log($"WORLD {worldCenters[i].name}");

                if (worldCenters[i].name != worldCenter.gameObject.name) {

                    //Debug.Log($"New World Center found: {worldCenters[i].name}");

                    worldCenter = worldCenters[i].transform;

                    return;
                }
            }

            if (worldCenters.Length == 0 && worldCenter == null) {
                //Debug.LogError($"No GameObjects with tag {worldCenterTagName} were found. Generating one with position <0, 0, 0>.");

                var generatedWorldCenter = new GameObject("GeneratedWorldCenter");

                generatedWorldCenter.tag = worldCenterTagName;

                generatedWorldCenter.transform.SetParent(null);

                worldCenter = generatedWorldCenter.transform;

                return;
            }

            //Debug.Log($"Using existing World Center: {worldCenter.gameObject.name}");
        }

        /// <summary>
        ///  Used to update the position and rotation of our XR Player
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        public void SetXRPlayerPositionAndLocalRotation(Vector3 pos, Quaternion rot)
        {
            xrPlayer.position = pos;
            xrPlayer.localRotation = rot;
        }
        public void SetXRAndSpectatorRotation(Quaternion rot)
        {
            xrPlayer.localRotation = rot;

            cameraRootTransform.localRotation = rot;
        }

        public void SetPlayerPositionToHome()
        {
            var homePos = (Vector3.up * cameraOffset.cameraYOffset); //SceneManagerExtensions.Instance.anchorPositionInNewScene.position +//defaultPlayerInitialHeight);

            desktopCameraTransform.position = homePos;//UIManager.Instance.anchorPositionInNewScene.position;//Vector3.up * defaultPlayerInitialHeight;

            UpdatePlayerPosition(new Position { pos = homePos });
        }

        public void SetPlayerPositionToHome2 () 
        {
            desktopCameraTransform.position = worldCenter.position;
        }
        
        public void UpdatePlayerPosition(Position newData)
        {
            //used in VR
            var finalPosition = newData.pos;
            finalPosition.y = newData.pos.y + cameraOffset.cameraYOffset;//defaultPlayerInitialHeight; //+ WebXR.WebXRManager.Instance.DefaultHeight;

//#if UNITY_EDITOR
            cameraRootTransform.position = finalPosition;
//#elif UNITY_WEBGL
            xrPlayer.position = finalPosition;
//#endif
            //  mainPlayer_RootTransformData.pos = finalPosition;
        }

        /// <summary>
        /// Update our XRAvatar according to the height a
        /// </summary>
        /// <param name="newHeight"></param>
        public void UpdatePlayerHeight(float newHeight)
        {
            var ratioScale = currentScale / 1;
            var offsetFix = ratioScale * newHeight;// 1.8f;

            cameraOffset.cameraYOffset = offsetFix;//(newHeight);// * currentScale);
        }


        private float currentScale;
        /// <summary>
        /// Scale our player and adjust the line rendering lines we are using with our player transform
        /// </summary>
        /// <param name="newScale">We can only set it at 0.35 since we get near cliping issues any further with 0.01 on the camera </param>
        public void UpdatePlayerScale(float newScale)
        {
            currentScale = newScale;
            var ratioScale = newScale / 1;
            var offsetFix = ratioScale * 1.8f;

            if (!desktopCameraTransform)
                desktopCameraTransform = GameObject.FindGameObjectWithTag("DesktopCamera").transform;

            if (!xrPlayer)
                xrPlayer = GameObject.FindGameObjectWithTag("XRCamera").transform;

            desktopCameraTransform.transform.localScale = Vector3.one * newScale;
            xrPlayer.transform.localScale = Vector3.one * newScale;

            cameraOffset.cameraYOffset = offsetFix;//newScale;


            //adjust the line renderers our player uses to be scalled accordingly
            foreach (var item in lineRenderersToScaleWithPlayer)
            {
                item.widthMultiplier = newScale;
            }

        }
    }
}