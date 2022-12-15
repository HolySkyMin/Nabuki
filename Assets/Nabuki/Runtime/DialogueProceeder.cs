﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nabuki
{
    public class DialogueProceeder : MonoBehaviour, IPointerDownHandler
    {
        public bool AllowInput
        {
            get => _allowInput;
            set
            {
                _allowInput = value;
                _hasInput = false;
            }
        }

        public bool HasInput => _hasInput;

        bool _allowInput, _hasInput;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_allowInput)
                _hasInput = true;
        }
    }
}