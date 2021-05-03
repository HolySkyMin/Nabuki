using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nabuki
{
    public class DialogueSelector : MonoBehaviour
    {
        public DialogueManager manager;
        public DialogueSelectButton buttonTemplate;
        public RectTransform buttonParent;

        [HideInInspector] public bool hasResult;
        [HideInInspector] public int result;

        List<GameObject> buttons = new List<GameObject>();

        public IEnumerator ShowSelect(Dictionary<int, string> selects, Action<int> callback)
        {
            hasResult = false;
            manager.proceeder.allowInput = false;

            foreach (var obj in buttons)
                Destroy(obj);
            buttons.Clear();

            foreach(var select in selects)
            {
                Debug.Log(select.Key + ": " + select.Value);
                var newButton = Instantiate(buttonTemplate);
                newButton.Set(select.Key, select.Value);
                newButton.transform.SetParent(buttonParent);
                newButton.transform.localScale = Vector3.one;
                newButton.gameObject.SetActive(true);
                buttons.Add(newButton.gameObject);
            }
            LayoutRebuilder.MarkLayoutForRebuild(buttonParent);
            LayoutRebuilder.ForceRebuildLayoutImmediate(buttonParent);

            gameObject.SetActive(true);
            yield return new WaitUntil(() => hasResult);
            gameObject.SetActive(false);
            callback(result);
        }
    }
}