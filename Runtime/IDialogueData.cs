using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public interface IDialogueData
    {
        bool Accept(DialogueManager dialog);

        IEnumerator Execute(DialogueManager dialog);
    }
}
