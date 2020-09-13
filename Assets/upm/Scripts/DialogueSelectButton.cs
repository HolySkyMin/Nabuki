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

        int index;

        public void Set(int i, string t)
        {
            index = i;
            text.text = t;
        }

        public void Clicked()
        {
            selector.result = index;
            selector.hasResult = true;
        }
    }
}