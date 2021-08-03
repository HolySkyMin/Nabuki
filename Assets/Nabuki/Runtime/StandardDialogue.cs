using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public class StandardDialogue : DialogueManager
    {
        [Header("Standard Dialogue Components")]
        public DialogueSelector selector;
        public DialogueCharacter characterTemplate;
        public DialogueField characterField;
        public DialogueField effectField;
        public new DialogueAudio audio;
        public DialogueBackground background;
        public DialogueBackground foreground;
        public SpriteRenderer sceneDimmer;
        public DialogueEvent events;

        [HideInInspector] public NbkData data;

        Dictionary<string, DialogueCharacter> characters;
        Dictionary<string, System.Func<IEnumerator>> actions;

        protected override void Awake()
        {
            base.Awake();

            characters = new Dictionary<string, DialogueCharacter>();
            actions = new Dictionary<string, System.Func<IEnumerator>>();
        }

        public override void AddCharacter(string key, string cname, int fieldIndex)
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

        public override bool CharacterExists(string key)
        {
            foreach (var character in characters)
                if (character.Key == key)
                    return true;
            return false;
        }

        public override bool FindCharacterName(string key, out string cname)
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

        public override bool FindCharacterKey(string cname, out string key)
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

        public override DialogueCharacter GetCharacter(string key)
        {
            return characters[key];
        }

        public override IEnumerator PlayEffect(string effect)
        {
            return base.PlayEffect(effect);
        }

        public override DialogueSelector GetSelector()
        {
            return selector;
        }

        public override DialogueBackground GetBackground()
        {
            return background;
        }

        public override DialogueBackground GetForeground()
        {
            return foreground;
        }

        public override IEnumerator SceneFadeIn(float time)
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

        public override IEnumerator SceneFadeOut(float time)
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

        public override NbkData GetData()
        {
            return data;
        }

        public override DialogueAudio GetAudio()
        {
            return audio;
        }

        public override void AssignAction(string key, System.Func<IEnumerator> action)
        {
            actions.Add(key, action);
        }

        public override IEnumerator CallAction(string key)
        {
            yield return events.Call(key);
        }
    }
}