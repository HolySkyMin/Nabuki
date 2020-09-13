﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Nabuki
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Now { get; private set; }

        public static DialogueSource Source;

        public DialogueCharacter characterTemplate;
        public DialogueField[] characterField;
        public DialogueField[] effectField;
        public DialogueDisplayer[] displayer;
        public DialogueProceeder proceeder;
        public DialogueSelector selector;
        public DialogueLogger logger;
        public new DialogueAudio audio;

        [Header("Dialogue Setting")]
        public bool enableLog;

        [HideInInspector] public int phase, dialogueIndex;
        [HideInInspector] public NbkData data;
        [HideInInspector] public Dictionary<string, DialogueCharacter> characters;

        public static NbkVariable GetVariable(string key) => Now.data.GetVariable(key);

        public static dynamic GetVariableValue(string key) => Now.data.GetVariableValue(key);

        public static T GetVariableValue<T>(string key) => Now.data.GetVariableValue<T>(key);

        public static void SetVariable(string key, dynamic value) => Now.data.SetVariable(key, value);

        public static void CreateVariable(string key, NbkVariableType type, dynamic value) => Now.data.CreateVariable(key, type, value);

        private void Awake()
        {
            if (Now != null)
            {
                Destroy(gameObject);
                return;
            }

            Now = this;

            // Initialize
            characters = new Dictionary<string, DialogueCharacter>();

            // Data load
            data = new NbkData() { variables = new Dictionary<string, NbkVariable>() };
        }

        private void OnDestroy()
        {
            Now = null;
        }

        public void AddCharacter(string key, string cname, int fieldIndex)
        {
            if (characters.ContainsKey(key))
                Debug.Log($"From Nabuki: Character {key} already exists.");
            else
            {
                var newCharacter = Instantiate(characterTemplate);
                newCharacter.Set(key, cname, characterField[fieldIndex]);
                //newCharacter.gameObject.SetActive(true);
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
            var parser = new DialogueParser();
            var dialogue = parser.Parse(script);

            StartCoroutine(Play_Routine(dialogue));
        }
        
        IEnumerator Play_Routine(Dictionary<int, List<IDialogue>> dialogue)
        {
            phase = 0;

            for (dialogueIndex = 0; dialogueIndex < dialogue[phase].Count; dialogueIndex++)
                yield return dialogue[phase][dialogueIndex].Run(this);
        }
    }
}