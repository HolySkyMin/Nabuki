using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Nabuki
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogueDisplayer : MonoBehaviour
    {
        public TMP_Text nameText, bodyText;
        public GameObject nameTag, endIndicator, unskipIndicator;
        [Header("Displayer Setting")]
        public bool animateText;
        public int cps;
        public bool removeNametagWhenNull;

        bool visible = false;

        public IEnumerator ShowText(string talker, string text, bool unskippable = false)
        {
            if (!visible)
                yield return Appear();

            nameTag.SetActive(talker != "" || !removeNametagWhenNull);
            nameText.SetText(talker);
            bodyText.SetText(text);

            var dt = System.DateTime.Now;
            //Debug.Log("Received text...");
            if(animateText)
            {
                unskipIndicator.SetActive(unskippable);
                DialogueManager.Now.proceeder.allowInput = !unskippable;

                int i = 0;
                float clock = 0f, spc = 1f / cps;

                bodyText.maxVisibleCharacters = 0;
                yield return null;
                //Debug.Log("Updated text info. Time consumed: " + (System.DateTime.Now - dt).TotalSeconds);
                while (i < bodyText.textInfo.characterCount)
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
            endIndicator.SetActive(false);
        }

        public IEnumerator Appear()
        {
            var cg = GetComponent<CanvasGroup>();
            cg.alpha = 1;
            visible = true;
            yield break;
        }

        public IEnumerator Disappear()
        {
            var cg = GetComponent<CanvasGroup>();
            cg.alpha = 0;
            visible = false;
            yield break;
        }
    }
}