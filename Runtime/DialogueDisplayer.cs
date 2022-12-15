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

        [SerializeField] bool animateText;
        [SerializeField] int cps;

        bool _isVisible;

        public IEnumerator SetVisible(bool visible)
        {
            var valueChanged = _isVisible ^ visible;
            _isVisible = visible;

            if(valueChanged)
            {
                if (_isVisible)
                    yield return Appear();
                else
                    yield return Disappear();
            }
            else
                yield break;
        }

        public abstract void Initialize();

        public abstract IEnumerator ShowText(string talker, string text, int localCps, bool unskippable = false);

        public abstract IEnumerator Appear();

        public abstract IEnumerator Disappear();
    }
}