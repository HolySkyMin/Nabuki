using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Nabuki
{
    public abstract class DialogueDisplayer : MonoBehaviour
    {
        public DialogueManager manager;
        public bool animateText;
        public int cps;

        public abstract IEnumerator ShowText(string talker, string text, int index, bool unskippable = false);

        public abstract IEnumerator Appear();

        public abstract IEnumerator Disappear();
    }
}