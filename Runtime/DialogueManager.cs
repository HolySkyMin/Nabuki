using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NaughtyAttributes;

namespace Nabuki
{
    public class DialogueManager : MonoBehaviour
    {
        public bool Ended { get; private set; }
        public int Phase { get { return phase; } }

        #region Script Configuration Flags

        [BoxGroup("Script support information")]
        [Label("Character")]
        public bool supportsCharacter;
        [BoxGroup("Script support information")]
        [Label("Character Field")]
        [ShowIf("supportsCharacter")]
        public bool supportsCharacterField;
        [BoxGroup("Script support information")]
        [Label("Selection")]
        public bool supportsSelection;
        [BoxGroup("Script support information")]
        [Label("Effect")]
        public bool supportsEffect;
        [BoxGroup("Script support information")]
        [Label("CG Walls")]
        public bool supportsCGWall;
        [BoxGroup("Script support information")]
        [Label("Variable")]
        public bool supportsVariable;
        [BoxGroup("Script support information")]
        [Label("Audio")]
        public bool supportsAudio;
        [BoxGroup("Script support information")]
        [Label("External Function")]
        public bool supportsExternalFunctions;

        #endregion

        [Header("Universal Components")]
        public DialogueSource source;
        public DialogueDisplayer displayer;
        public bool enableLog;
        [ShowIf("enableLog")]
        public DialogueLogger logger;

        internal int phase, dialogueIndex;

        protected virtual void Awake()
        {
            Ended = false;
            phase = 0;
        }

        protected void OnDestroy()
        {
            source.Dispose();
        }

        // ================

        public virtual void AddCharacter(string key, string cname, int fieldIndex)
        {
            Debug.LogError($"[Nabuki] DialogueManager ({gameObject.name}) supports character, but AddCharacter is not implemented.");
        }

        public virtual bool CharacterExists(string key)
        {
            Debug.LogError($"[Nabuki] DialogueManager ({gameObject.name}) supports character, but CharacterExists is not implemented.");
            return false;
        }

        public virtual bool FindCharacterName(string key, out string cname)
        {
            Debug.LogError($"[Nabuki] DialogueManager ({gameObject.name}) supports character, but FindCharacterName is not implemented.");
            cname = "";
            return false;
        }

        public virtual bool FindCharacterKey(string cname, out string key)
        {
            Debug.LogError($"[Nabuki] DialogueManager ({gameObject.name}) supports character, but FindCharacterKey is not implemented.");
            key = "";
            return false;
        }

        public virtual DialogueCharacter GetCharacter(string key)
        {
            Debug.LogError($"[Nabuki] DialogueManager ({gameObject.name}) supports character field, but GetCharacter is not implemented.");
            return null;
        }

        public virtual IEnumerator PlayEffect(string effect)
        {
            Debug.LogError($"[Nabuki] DialogueManager ({gameObject.name}) supports effect, but PlayEffect is not implemented.");
            yield break;
        }

        public virtual DialogueSelector GetSelector()
        {
            Debug.LogError($"[Nabuki] DialogueManager ({gameObject.name}) supports selection, but GetSelector is not implemented.");
            return null;
        }

        public virtual DialogueBackground GetBackground()
        {
            Debug.LogError($"[Nabuki] DialogueManager ({gameObject.name}) supports CG walls, but GetBackground is not implemented.");
            return null;
        }

        public virtual DialogueBackground GetForeground()
        {
            Debug.LogError($"[Nabuki] DialogueManager ({gameObject.name}) supports CG walls, but GetForeground is not implemented.");
            return null;
        }

        public virtual IEnumerator SceneFadeIn(float time)
        {
            Debug.LogError($"[Nabuki] DialogueManager ({gameObject.name}) supports CG walls, but SceneFadeIn is not implemented.");
            yield break;
        }

        public virtual IEnumerator SceneFadeOut(float time)
        {
            Debug.LogError($"[Nabuki] DialogueManager ({gameObject.name}) supports CG walls, but SceneFadeOut is not implemented.");
            yield break;
        }

        public virtual NbkData GetData()
        {
            Debug.LogError($"[Nabuki] DialogueManager ({gameObject.name}) supports variable, but GetData is not implemented.");
            return null;
        }

        public virtual DialogueAudio GetAudio()
        {
            Debug.LogError($"[Nabuki] DialogueManager ({gameObject.name}) supports audio, but GetAudio is not implemented.");
            return null;
        }

        public virtual void AssignAction(string key, System.Func<IEnumerator> action)
        {
            Debug.LogError($"[Nabuki] DialogueManager ({gameObject.name}) supports external action, but AssignAction is not implemented.");
        }

        public virtual IEnumerator CallAction(string key)
        {
            Debug.LogError($"[Nabuki] DialogueManager ({gameObject.name}) supports external action, but CallAction is not implemented.");
            yield break;
        }

        public void Play(string script)
        {
            // Parse dialogue
            var parser = new DialogueParser(this);
            var dialogue = parser.Parse(script);

            StartCoroutine(Play_Routine(dialogue));
        }
        
        IEnumerator Play_Routine(Dictionary<int, List<IDialogue>> dialogue)
        {
            for (dialogueIndex = 0; dialogueIndex < dialogue[phase].Count; dialogueIndex++)
                yield return dialogue[phase][dialogueIndex].Run(this);

            Ended = true;
        }

        public void PlayBGM(string key) => GetAudio().PlayBGM(key, source);

        public void PlayVoice(string key) => GetAudio().PlayVoice(key, source);

        public void PlaySE(string key) => GetAudio().PlaySE(key, source);
    }
}