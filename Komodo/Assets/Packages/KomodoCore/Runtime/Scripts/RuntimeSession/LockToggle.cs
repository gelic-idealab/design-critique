using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Entities;

namespace Komodo.Runtime
{
    [RequireComponent(typeof(Toggle))]
    public class LockToggle : MonoBehaviour, IPointerClickHandler
    {
        private EntityManager entityManager;

        private int index;

        private Toggle toggle;

        public GameObject lockedIcon;

        public GameObject unlockedIcon;

        public void Toggle (bool doLock)
        {
            UpdateComponentData(doLock);

            UpdateUI(doLock);

            SendNetworkUpdate(doLock);
        }

        public void UpdateUI (bool doLock)
        {
            if (doLock)
            {
                lockedIcon.SetActive(true);

                unlockedIcon.SetActive(false);

                return;
            }

            lockedIcon.SetActive(false);

            unlockedIcon.SetActive(true);
        }

        public void Initialize (int index)
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            toggle = GetComponent<Toggle>();

            if (!toggle)
            {
                throw new MissingComponentException("Toggle on object with LockToggle script on it");
            }

            if (lockedIcon == null || unlockedIcon == null)
            {
                throw new UnassignedReferenceException("lockedIcon or unlockedIcon on LockToggle component");
            }

            this.index = index;

            Toggle(false);
        }

        public void OnPointerClick (PointerEventData data)
        {
            Toggle(this.toggle.isOn); // The value of toggle should be changed by the time this event handler fires, so we should be able to use its updated value here.
        }

        public void SendNetworkUpdate (bool doLock)
        {
            int lockState = 0;

            //SETUP and send network lockstate
            if (doLock)
            {
                lockState = (int)INTERACTIONS.LOCK;
            }
            else
            {
                lockState = (int)INTERACTIONS.UNLOCK;
            }

            int entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(NetworkedObjectsManager.Instance.GetNetworkedSubObjectList(this.index)[0].Entity).entityID;

            NetworkUpdateHandler.Instance.SendSyncInteractionMessage(new Interaction
            {
                sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
                targetEntity_id = entityID,
                interactionType = lockState,
            });
        }

        public void UpdateComponentData (bool doLock)
        {
            UpdateComponentData(doLock, this.index);
        }

        public void UpdateComponentData (bool doLock, int id)
        {
            foreach (NetworkedGameObject item in NetworkedObjectsManager.Instance.GetNetworkedSubObjectList(id))
            {
                if (doLock)
                {
                    if (!entityManager.HasComponent<TransformLockTag>(item.Entity))
                    {
                        entityManager.AddComponentData(item.Entity, new TransformLockTag());
                    }
                }
                else
                {
                    if (entityManager.HasComponent<TransformLockTag>(item.Entity))
                    {
                        entityManager.RemoveComponent<TransformLockTag>(item.Entity);
                    }
                }
            }
        }

        public void ProcessNetworkToggle (bool doLock, int id)
        {
            UpdateComponentData(doLock, id);

            UpdateUI(doLock);

            toggle.isOn = doLock;
        }
    }
}