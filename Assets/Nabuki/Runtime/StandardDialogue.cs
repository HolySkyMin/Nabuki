using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public class StandardDialogue : DialogueManager, IFeatureCharacterWithField, IFeatureSelection, IFeatureBackground,
        IFeatureForeground, IFeatureEffect, IFeatureVariable, IFeatureAudio, IFeatureExternalAction
    {
        public DialogueSelector Selector => selector;

        public DialogueBackground Background => background;

        public DialogueBackground Foreground => foreground;

        public IDialogueAudio Audio => _audio;

        public NbkData VariableData => _data;

        [Header("Standard Dialogue Components")]
        public DialogueSelector selector;
        public DialogueCharacter characterTemplate;
        public DialogueField characterField;
        public DialogueField effectField;
        public DialogueBackground background;
        public DialogueBackground foreground;
        public SpriteRenderer sceneDimmer;
        public DialogueEvent events;

        IDialogueAudio _audio;
        NbkData _data;
        Dictionary<string, DialogueCharacter> characters;
        Dictionary<string, System.Func<IEnumerator>> actions;

        protected void Awake()
        {
            characters = new Dictionary<string, DialogueCharacter>();
            actions = new Dictionary<string, System.Func<IEnumerator>>();
        }

        public void AddCharacter(string key, string cname)
        {
            AddCharacter(key, cname, 0);
        }

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
            foreach (var character in characters)
            {
                if (character.Value.charaName == cname)
                {
                    key = character.Key;
                    return true;
                }
            }
            key = "";
            return false;
        }

        public DialogueCharacter GetCharacter(string key)
        {
            return characters[key];
        }

        public IEnumerator PlayEffect(string effect)
        {
            yield return null;
        }

        public IEnumerator SceneFadeIn(float time)
        {
            sceneDimmer.color = Color.black;
            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                sceneDimmer.color = Color.Lerp(Color.black, Color.clear, progress);
                yield return null;
            }
            sceneDimmer.color = Color.clear;
            sceneDimmer.gameObject.SetActive(false);
        }

        public IEnumerator SceneFadeOut(float time)
        {
            sceneDimmer.color = Color.clear;
            sceneDimmer.gameObject.SetActive(true);
            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                sceneDimmer.color = Color.Lerp(Color.clear, Color.black, progress);
                yield return null;
            }
            sceneDimmer.color = Color.black;
        }

        public void SetAudio(IDialogueAudio audio)
        {
            _audio = audio;

            _audio.SetSource(source);
        }

        public void SetVariableData(NbkData data)
        {
            _data = data;
        }

        public void AssignAction(string key, System.Func<IEnumerator> action)
        {
            actions.Add(key, action);
        }

        public IEnumerator CallAction(string key)
        {
            yield return events.Call(key);
        }
    }
}