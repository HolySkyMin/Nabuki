using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Nabuki
{
    public class DialogueDisplayer : MonoBehaviour
    {
        public TMP_Text nameText, bodyText;
        public GameObject nameTag, endIndicator, unskipIndicator;
        [Header("Displayer Setting")]
        public bool animateText;
        public int cps;
        public bool removeNametagWhenNull;

        public IEnumerator ShowText(string talker, string text, bool unskippable = false)
        {
            endIndicator.SetActive(false);

            nameTag.SetActive(talker != "" || !removeNametagWhenNull);
            nameText.SetText(talker);
            bodyText.SetText(text);

            if(animateText)
            {
                unskipIndicator.SetActive(unskippable);
                DialogueManager.Now.proceeder.allowInput = !unskippable;

                int i = 0;
                float clock = 0f, spc = 1f / cps;

                bodyText.maxVisibleCharacters = 0;
                while(i < bodyText.textInfo.characterCount)
                {
                    var next = Mathf.FloorToInt(clock / spc);
                    if (next > i)
                    {
                        i = next;
                        bodyText.maxVisibleCharacters = i;
                    }

                    if(!unskippable && DialogueManager.Now.proceeder.hasInput)
                    {
                        DialogueManager.Now.proceeder.hasInput = false;
                        break;
                    }

                    clock += Time.deltaTime;
                    yield return null; // loop every frame.
                }
                bodyText.maxVisibleCharacters = bodyText.textInfo.characterCount;
            }

            endIndicator.SetActive(true);
            DialogueManager.Now.proceeder.allowInput = true;
            yield return new WaitUntil(() => DialogueManager.Now.proceeder.hasInput);
            DialogueManager.Now.proceeder.hasInput = false;
        }

        public IEnumerator Appear()
        {
            yield break;
        }

        public IEnumerator Disappear()
        {
            yield break;
        }
    }
}