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
        [SerializeField] DialogueSelector selector;
        [SerializeField] DialogueCharacter characterTemplate;
        [SerializeField] DialogueField characterField;
        [SerializeField] DialogueField effectField;
        [SerializeField] DialogueBackground background;
        [SerializeField] DialogueBackground foreground;
        [SerializeField] DialogueEvent events;
        [SerializeField] bool useUIField;
        [SerializeField, NaughtyAttributes.ShowIf("useUIField")] CanvasGroup fieldDimmerUI;
        [SerializeField, NaughtyAttributes.HideIf("useUIField")] SpriteRenderer fieldDimmerWorld;

        IDialogueAudio _audio;
        NbkData _data;

        Dictionary<string, DialogueCharacter> characters;
        Dictionary<string, string> _characterOverridedName;
        Dictionary<string, System.Func<IEnumerator>> actions;

        protected void Awake()
        {
            characters = new Dictionary<string, DialogueCharacter>();
            actions = new Dictionary<string, System.Func<IEnumerator>>();

            _characterOverridedName = new Dictionary<string, string>();
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
                newCharacter.Initialize(key, cname, characterField, fieldIndex);
                newCharacter.name = "Character: " + key;
                newCharacter.gameObject.SetActive(true);
                characters.Add(key, newCharacter);
            }
        }

        public void OverrideCharacterName(string key, string newName)
        {
            _characterOverridedName.Add(key, newName);
        }

        public void ResetCharacterName(string key)
        {
            _characterOverridedName.Remove(key);
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
                    cname = _characterOverridedName.ContainsKey(key) ? _characterOverridedName[key] : character.Value.Name;
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
                if (character.Value.Name == cname)
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
            if (useUIField)
            {
                fieldDimmerUI.alpha = 1;
                for (var clock = 0f; clock < time; clock += Time.deltaTime)
                {
                    var progress = clock / time;
                    fieldDimmerUI.alpha = 1 - progress;
                    yield return null;
                }
                fieldDimmerUI.alpha = 0;
                fieldDimmerUI.gameObject.SetActive(false);
            }
            else
            {
                fieldDimmerWorld.color = Color.black;
                for (var clock = 0f; clock < time; clock += Time.deltaTime)
                {
                    var progress = clock / time;
                    fieldDimmerWorld.color = Color.Lerp(Color.black, Color.clear, progress);
                    yield return null;
                }
                fieldDimmerWorld.color = Color.clear;
                fieldDimmerWorld.gameObject.SetActive(false);
            }
        }

        public IEnumerator SceneFadeOut(float time)
        {
            if (useUIField)
            {
                fieldDimmerUI.alpha = 0;
                fieldDimmerUI.gameObject.SetActive(true);
                for (var clock = 0f; clock < time; clock += Time.deltaTime)
                {
                    var progress = clock / time;
                    fieldDimmerUI.alpha = progress;
                    yield return null;
                }
                fieldDimmerUI.alpha = 1;
            }
            else
            {
                fieldDimmerWorld.color = Color.clear;
                fieldDimmerWorld.gameObject.SetActive(true);
                for (var clock = 0f; clock < time; clock += Time.deltaTime)
                {
                    var progress = clock / time;
                    fieldDimmerWorld.color = Color.Lerp(Color.clear, Color.black, progress);
                    yield return null;
                }
                fieldDimmerWorld.color = Color.black;
            }
        }

        public void SetAudio(IDialogueAudio audio)
        {
            _audio = audio;

            _audio.SetSource(Source);
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

        protected override void Initialize()
        {
            if (characters == null)
                characters = new Dictionary<string, DialogueCharacter>();
            foreach (var character in characters)
                Destroy(character.Value.gameObject);
            characters.Clear();
        }

        public override string GetPlayerName()
        {
            return _characterOverridedName.ContainsKey(PlayerKeyword)
                ? _characterOverridedName[PlayerKeyword] : _data.playerName;
        }
    }
}