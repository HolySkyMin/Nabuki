using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Nabuki
{
    [RequireComponent(typeof(CanvasGroup))]
    public class StandardDialogueDisplayer : DialogueDisplayer
    {
        [SerializeField] DialogueProceeder proceeder;
        [SerializeField] TMP_Text nameText, bodyText;
        [SerializeField] GameObject nameTag, endIndicator, unskipIndicator;
        [SerializeField] bool removeNametagWhenNull;

        public override void Initialize()
        {
            base.IsVisible = false;
            nameText.SetText(string.Empty);
            bodyText.SetText(string.Empty);
        }

        public override IEnumerator ShowText(string talker, string text, int localCps, bool unskippable = false)
        {
            base.IsVisible = true;

            nameTag.SetActive(talker != "" || !removeNametagWhenNull);
            nameText.SetText(talker);
            bodyText.SetText(text);

            if (AnimatesText)
            {
                unskipIndicator.SetActive(unskippable);
                proceeder.AllowInput = !unskippable;

                int i = 0;
                float clock = 0f, spc = 1f / (localCps > 0 ? localCps : CPS);

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

                    if (proceeder.AllowInput && proceeder.HasInput)
                    {
                        proceeder.AllowInput = false;
                        break;
                    }

                    clock += Time.deltaTime;
                    yield return null; // loop every frame.
                }
                bodyText.maxVisibleCharacters = bodyText.textInfo.characterCount;
            }

            endIndicator.SetActive(true);
            proceeder.AllowInput = true;
            yield return new WaitUntil(() => proceeder.HasInput);
            proceeder.AllowInput = false;
            endIndicator.SetActive(false);
        }

        public override IEnumerator Appear()
        {
            var cg = GetComponent<CanvasGroup>();
            cg.alpha = 1;
            yield break;
        }

        public override IEnumerator Disappear()
        {
            var cg = GetComponent<CanvasGroup>();
            cg.alpha = 0;
            yield break;
        }
    }
}