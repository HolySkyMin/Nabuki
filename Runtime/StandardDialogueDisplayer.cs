using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Nabuki
{
    [RequireComponent(typeof(CanvasGroup))]
    public class StandardDialogueDisplayer : DialogueDisplayer
    {
        public TMP_Text nameText, bodyText;
        public GameObject nameTag, endIndicator, unskipIndicator;
        public bool removeNametagWhenNull;

        bool visible = false;

        public override IEnumerator ShowText(string talker, string text, int index, bool unskippable = false)
        {
            if (!visible)
                yield return Appear();

            nameTag.SetActive(talker != "" || !removeNametagWhenNull);
            nameText.SetText(talker);
            bodyText.SetText(text);

            var dt = System.DateTime.Now;
            if (animateText)
            {
                unskipIndicator.SetActive(unskippable);
                manager.proceeder.allowInput = !unskippable;

                int i = 0;
                float clock = 0f, spc = 1f / cps;

                bodyText.maxVisibleCharacters = 0;
                yield return null;
                while (i < bodyText.textInfo.characterCount)
                {
                    var next = Mathf.FloorToInt(clock / spc);
                    if (next > i)
                    {
                        i = next;
                        bodyText.maxVisibleCharacters = i;
                    }

                    if (!unskippable && manager.proceeder.hasInput)
                    {
                        manager.proceeder.hasInput = false;
                        break;
                    }

                    clock += Time.deltaTime;
                    yield return null; // loop every frame.
                }
                bodyText.maxVisibleCharacters = bodyText.textInfo.characterCount;
            }

            endIndicator.SetActive(true);
            manager.proceeder.allowInput = true;
            yield return new WaitUntil(() => manager.proceeder.hasInput);
            manager.proceeder.hasInput = false;
            endIndicator.SetActive(false);
        }

        public override IEnumerator Appear()
        {
            var cg = GetComponent<CanvasGroup>();
            cg.alpha = 1;
            visible = true;
            yield break;
        }

        public override IEnumerator Disappear()
        {
            var cg = GetComponent<CanvasGroup>();
            cg.alpha = 0;
            visible = false;
            yield break;
        }
    }
}