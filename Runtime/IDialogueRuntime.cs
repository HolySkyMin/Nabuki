using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public interface IDialogueRuntime : IEnumerable<IDialogueData>
    {
        void Parse(string script);

        void ChangePhase(int phase);
    }
}
