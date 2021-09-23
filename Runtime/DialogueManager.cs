using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NaughtyAttributes;

namespace Nabuki
{
    public abstract class DialogueManager : MonoBehaviour
    {
        public DialogueSource Source => source;

        public DialogueDisplayer Displayer => displayer;

        public DialogueLogger Logger => logger;

        public bool LogEnabled => enableLog;

        public bool Ended { get; private set; }

        public int Phase => _dialogue.CurrentPhase;

        [Header("Universal Components")]
        [SerializeField] DialogueSource source;
        [SerializeField] DialogueDisplayer displayer;
        [SerializeField] bool enableLog;
        [ShowIf("enableLog")]
        [SerializeField] DialogueLogger logger;

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
            OnDialogueStart();

            _dialogue = _parser.Parse(script);

            StartCoroutine(Play_Routine());
        }

        public void Play(string filePath)
        {
            Ended = false;
            OnDialogueStart();

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

            OnDialogueEnd();
            Ended = true;
        }

        public void SetPhase(int phase)
        {
            _dialogue.CurrentPhase = phase;
        }

        protected abstract void OnDialogueStart();

        protected abstract void OnDialogueEnd();
    }
}