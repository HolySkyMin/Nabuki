using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public abstract class DialogueSourceProvider : MonoBehaviour
    {
        public abstract IEnumerator GetDialogue(string key, Action<string> callback);

        public abstract IEnumerator GetSprite(string key, Action<Sprite> callback);

        public abstract IEnumerator GetSound(string key, Action<AudioClip> callback);

        public abstract IEnumerator GetObject(string key, Action<GameObject> callback);

        public abstract void Dispose();
    }
}
