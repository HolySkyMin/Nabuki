using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nabuki
{
    public class DialogueLogger : MonoBehaviour, IPointerClickHandler
    {
        public TMP_Text log;
        public string characterColor, playerColor;
        public int indent;
        public int voiceSpriteIndex;

        public void Log(string talker, string text, string voiceKey, bool isPlayer)
        {
            log.text += $"\n\n<color={(isPlayer ? playerColor : characterColor)}>{talker}</color>"
                + (voiceKey == "" ? "" : $" <link=\"{voiceKey}\"><sprite={voiceSpriteIndex}></link>")
                + $"\n<indent={indent}>{text}</indent>";
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var linkIndex = TMP_TextUtilities.FindIntersectingLink(log, Input.mousePosition, Camera.main);
            if(linkIndex != -1)
            {
                var linkInfo = log.textInfo.linkInfo[linkIndex];

            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}