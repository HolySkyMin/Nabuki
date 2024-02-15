using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Nabuki
{
    public class DialogueSelectButton : MonoBehaviour
    {
        public TMP_Text text;
        public DialogueSelector selector;

        private int _index;

        public void Set(int i, string t)
        {
            _index = i;
            text.text = t;
        }

        public void Clicked()
        {
            selector.result = _index;
            selector.hasResult = true;
        }
    }
}