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

        public string playerSpawnCenterTag = "PlayerSpawnCenter";

        private Transform currentSpawnCenter;

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

            SetPlayerSpawnCenter();
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

        /**
        * Finds a gameObject whose Transform represents the center of the circle
        * where players may spawn.  Use this, for example, on each scene load 
        * to set the new additive scene's spawn center correctly.
        * 
        * Importantly, this will help set the y-height 
        * of the floor for an arbitrary scene. To use this, in each additive 
        * scene, create an empty, place it at the floor of the environment 
        * where you want players to spawn, and tag the empty with 
        * <playerSpawnCenterTag>.
        */
        public void SetPlayerSpawnCenter ()
        {
            const string generatedSpawnCenterName = "PlayerSpawnCenter";

            var spawnCentersFound = GameObject.FindGameObjectsWithTag(playerSpawnCenterTag);

            // If we found gameObjects with the right tag,
            // pick the first one that's different from the current one

            for (int i = 0; i < spawnCentersFound.Length; i += 1) {

                if (spawnCentersFound[i] != currentSpawnCenter.gameObject) {

                    //Debug.Log($"[PlayerSpawnCenter] New center found: {spawnCentersFound[i].name}");

                    currentSpawnCenter = spawnCentersFound[i].transform;

                    return;
                }
            }

            // If we didn't find any new gameObjects with the right tag,
            // and there's no existing one, make a new one with default settings

            if (spawnCentersFound.Length == 0 && currentSpawnCenter == null) {
                //Debug.LogWarning($"[PlayerSpawnCenter] No GameObjects with tag {playerSpawnCenterTag} were found. Generating one with position <0, 0, 0>.");

                var generatedSpawnCenter = new GameObject(generatedSpawnCenterName);

                generatedSpawnCenter.tag = playerSpawnCenterTag;

                generatedSpawnCenter.transform.SetParent(null);

                currentSpawnCenter = generatedSpawnCenter.transform;

                return;
            }

            // If no gameObjects with the right tag were found, and there is an 
            // existing one, use the existing one. 

            //Debug.Log($"[PlayerSpawnCenter] Using existing Player Spawn Center: {currentSpawnCenter.gameObject.name}");
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
            desktopCameraTransform.position = currentSpawnCenter.position;
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