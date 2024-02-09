using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public interface IDialogueAudio
    {
        public void SetSource(DialogueSource souce);

        public void PlayBGM(string key);

        public void StopBGM();

        public void PlaySE(string key);

        public void PlayVoice(string key);
    }
}
