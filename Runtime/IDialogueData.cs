using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public interface IDialogueData
    {
        IEnumerator Execute(DialogueManager dialog);
    }
}
