using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public interface IDialogueParser
    {
        DialogueDataCollection Parse(string data);
    }
}
