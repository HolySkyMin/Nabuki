using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nabuki
{
    public class DialogueProceeder : MonoBehaviour, IPointerDownHandler
    {
        [HideInInspector] public bool allowInput;
        [HideInInspector] public bool hasInput;

        public void OnPointerDown(PointerEventData eventData)
        {
            if(allowInput)
                hasInput = true;
        }
    }
}