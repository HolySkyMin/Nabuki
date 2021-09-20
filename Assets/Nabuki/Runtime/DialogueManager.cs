using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NaughtyAttributes;

namespace Nabuki
{
    public abstract class DialogueManager : MonoBehaviour
    {
        public bool Ended { get; private set; }

        public int Phase => _dialogue.CurrentPhase;

        [Header("Universal Components")]
        public DialogueSource source;
        public DialogueDisplayer displayer;
        public bool enableLog;
        [ShowIf("enableLog")]
        public DialogueLogger logger;

        DialogueDataCollection _dialogue;
        IDialogueParser _parser;

        protected void OnDestroy()
        {
            source.Dispose();
        }

        public void SetParser(IDialogueParser parser)
        {
            _parser = parser;
        }

        public void PlayDirectly(string script)
        {
            Ended = false;

            _dialogue = _parser.Parse(script);

            StartCoroutine(Play_Routine());
        }

        public void Play(string filePath)
        {
            Ended = false;
            StartCoroutine(Play_WithFileLoading(filePath));
        }

        IEnumerator Play_WithFileLoading(string filePath)
        {
            string textData = string.Empty;
            yield return source.GetDialogueAsync(filePath, result => { textData = result; });

            _dialogue = _parser.Parse(textData);

            yield return Play_Routine();
        }
        
        IEnumerator Play_Routine()
        {
            foreach (var data in _dialogue)
                yield return data.Execute(this);

            Ended = true;
        }

        public void SetPhase(int phase)
        {
            _dialogue.CurrentPhase = phase;
        }
    }
}