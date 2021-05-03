using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public class StandardDialogue : DialogueManager
    {
        public DialogueBackground background;
        public DialogueBackground foreground;
        public SpriteRenderer sceneDimmer;

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
    }
}