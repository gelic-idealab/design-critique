using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Komodo.Runtime
{
    [RequireComponent(typeof(Image))]
    public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        public TabManager tabManager;

        public Image background;

        public UnityEvent onTabSelected;

        public UnityEvent onTabDeselected;

        public List<GameObject> objects;

        public void OnPointerClick(PointerEventData eventData)
        {
            tabManager.OnTabToggled(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tabManager.OnTabEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tabManager.OnTabExit(this);
        }

        void Start ()
        {
            background = GetComponent<Image>();

            if (tabManager == null) 
            {
                throw new System.NullReferenceException("tabManager");
            }

            tabManager.Subscribe(this);
        }

        public void Select ()
        {
            onTabSelected.Invoke();
        }

        public void Deselect ()
        {
            onTabDeselected.Invoke();
        }
    }
}
