using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public interface IFeatureCharacter
    {
        public void AddCharacter(string key, string name);
        public void OverrideCharacterName(string key, string newName);
        public void ResetCharacterName(string key);
        public bool CharacterExists(string key);
        public bool FindCharacterKey(string name, out string key);
        public bool FindCharacterName(string key, out string name);
    }

    public interface IFeatureCharacterWithField : IFeatureCharacter
    {
        public void AddCharacter(string key, string name, int fieldIndex);
        public DialogueCharacter GetCharacter(string key);
    }

    public interface IFeatureEffect
    {
        public IEnumerator PlayEffect(string key);
    }

    public interface IFeatureSelection
    {
        public DialogueSelector Selector { get; }
    }

    public interface IFeatureTransition
    {
        public IEnumerator SceneFadeIn(float time);
        public IEnumerator SceneFadeOut(float time);
    }

    public interface IFeatureBackground : IFeatureTransition
    {
        public DialogueBackground Background { get; }
    }

    public interface IFeatureForeground
    {
        public DialogueBackground Foreground { get; }
    }

    public interface IFeatureVariable
    {
        public NbkData VariableData { get; }

        public void SetVariableData(NbkData data);
    }

    public interface IFeatureAudio
    {
        public IDialogueAudio Audio { get; }

        public void SetAudio(IDialogueAudio audio);
    }

    public interface IFeatureExternalAction
    {
        public void AssignAction(string key, System.Func<IEnumerator> action);
        public IEnumerator CallAction(string key);
    }
}
