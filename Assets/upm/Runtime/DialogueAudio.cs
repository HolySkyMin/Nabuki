﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public class DialogueAudio : MonoBehaviour
    {
        public AudioSource bgm, voice, se;

        public void PlayBGM(string key)
        {
            StartCoroutine(DialogueManager.Source.GetSoundAsync(key, clip =>
            {
                bgm.clip = clip;
                bgm.Play();
            }));
        }

        public void StopBGM() => bgm.Stop();

        public void PlayVoice(string key)
        {
            if (key != "")
                voice.PlayOneShot(DialogueManager.Source.GetSound(key));
        }

        public void PlaySE(string key)
        {
            se.PlayOneShot(DialogueManager.Source.GetSound(key));
        }
    }
}