using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Nabuki
{
    public class DialogueManager : MonoBehaviour
    {
        public bool Ended { get; private set; }
        public int Phase { get { return phase; } }

        public DialogueSource source;
        public DialogueCharacter characterTemplate;
        public DialogueField characterField;
        public DialogueField effectField;
        public DialogueDisplayer displayer;
        public DialogueProceeder proceeder;
        public DialogueSelector selector;
        public DialogueLogger logger;
        public new DialogueAudio audio;

        [Header("Dialogue Setting")]
        public bool enableLog;
        public bool supportsBackground;
        public bool supportsForeground;

        [HideInInspector] public NbkData data;
        [HideInInspector] public Dictionary<string, DialogueCharacter> characters;
        [HideInInspector] public Dictionary<string, System.Action> externalAction;
        [HideInInspector] public Dictionary<string, System.Func<NbkTokenizer, IDialogue>> customSyntax;

        internal int phase, dialogueIndex;

        private void Awake()
        {
            // Initialize
            characters = new Dictionary<string, DialogueCharacter>();
            externalAction = new Dictionary<string, System.Action>();
            customSyntax = new Dictionary<string, System.Func<NbkTokenizer, IDialogue>>();
            Ended = false;
            phase = 0;
        }

        private void OnDestroy()
        {
            source.Dispose();
        }

        // ================

        public void AddCharacter(string key, string cname, int fieldIndex)
        {
            if (characters.ContainsKey(key))
                Debug.Log($"From Nabuki: Character {key} already exists.");
            else
            {
                var newCharacter = Instantiate(characterTemplate);
                newCharacter.Set(key, cname, characterField, fieldIndex);
                newCharacter.name = "Character: " + key;
                newCharacter.gameObject.SetActive(true);
                characters.Add(key, newCharacter);
            }
        }

        public bool CharacterExists(string key)
        {
            foreach (var character in characters)
                if (character.Key == key)
                    return true;
            return false;
        }

        public bool FindCharacterName(string key, out string cname)
        {
            foreach (var character in characters)
            {
                if (character.Key == key)
                {
                    cname = character.Value.charaName;
                    return true;
                }
            }
            cname = "";
            return false;
        }

        public bool FindCharacterKey(string cname, out string key)
        {
            foreach(var character in characters)
            {
                if(character.Value.charaName == cname)
                {
                    key = character.Key;
                    return true;
                }
            }
            key = "";
            return false;
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

        public void PlayBGM(string key) => audio.PlayBGM(key, source);

        public void PlayVoice(string key) => audio.PlayVoice(key, source);

        public void PlaySE(string key) => audio.PlaySE(key, source);

        public virtual DialogueBackground GetBackground() { return null; }

        public virtual DialogueBackground GetForeground() { return null; }

        public virtual IEnumerator SceneFadeIn(float time) { yield break; }

        public virtual IEnumerator SceneFadeOut(float time) { yield break; }
    }
}