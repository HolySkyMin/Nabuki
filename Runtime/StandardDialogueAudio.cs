using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public class StandardDialogueAudio : MonoBehaviour, IDialogueAudio
    {
        public AudioSource bgm, voice, se;

        DialogueSource _source;

        public void SetSource(DialogueSource source)
        {
            _source = source;
        }

        public void PlayBGM(string key)
        {
            StartCoroutine(_source.GetSoundAsync(key, clip =>
            {
                bgm.clip = clip;
                bgm.Play();
            }));
        }

        public void StopBGM() => bgm.Stop();

        public void PlayVoice(string key)
        {
            if (key != "")
            {
                StartCoroutine(_source.GetSoundAsync(key, clip => { voice.PlayOneShot(clip); }));
            }
        }

        public void PlaySE(string key)
        {
            StartCoroutine(_source.GetSoundAsync(key, clip => { se.PlayOneShot(clip); }));
        }
    }
}