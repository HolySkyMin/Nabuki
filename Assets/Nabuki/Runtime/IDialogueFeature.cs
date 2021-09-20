using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public interface IFeatureCharacter
    {
        void AddCharacter(string key, string name);
        bool CharacterExists(string key);
        bool FindCharacterKey(string name, out string key);
        bool FindCharacterName(string key, out string name);
    }

    public interface IFeatureCharacterWithField : IFeatureCharacter
    {
        void AddCharacter(string key, string name, int fieldIndex);
        DialogueCharacter GetCharacter(string key);
    }

    public interface IFeatureEffect
    {
        IEnumerator PlayEffect(string key);
    }

    public interface IFeatureSelection
    {
        DialogueSelector Selector { get; }
    }

    public interface IFeatureTransition
    {
        IEnumerator SceneFadeIn(float time);
        IEnumerator SceneFadeOut(float time);
    }

    public interface IFeatureBackground : IFeatureTransition
    {
        DialogueBackground Background { get; }
    }

    public interface IFeatureForeground
    {
        DialogueBackground Foreground { get; }
    }

    public interface IFeatureVariable
    {
        NbkData VariableData { get; }
    }

    public interface IFeatureAudio
    {
        DialogueAudio Audio { get; }

        void PlayBGM(string key);
        void PlaySE(string key);
        void PlayVoice(string key);
    }

    public interface IFeatureExternalAction
    {
        void AssignAction(string key, System.Func<IEnumerator> action);
        IEnumerator CallAction(string key);
    }
}
