using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Nabuki
{
    public abstract class DialogueDisplayer : MonoBehaviour
    {
        public int CPS => cps;

        public bool AnimatesText => animateText;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                var valueChanged = _isVisible ^ value;
                _isVisible = value;

                if(valueChanged)
                {
                    if (_isVisible)
                        StartCoroutine(Appear());
                    else
                        StartCoroutine(Disappear());
                }
            }
        }

        [SerializeField] bool animateText;
        [SerializeField] int cps;

        bool _isVisible;

        public abstract void Initialize();

        public abstract IEnumerator ShowText(string talker, string text, int localCps, bool unskippable = false);

        public abstract IEnumerator Appear();

        public abstract IEnumerator Disappear();
    }
}