using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public class StandardDialogueRuntime : IDialogueRuntime
    {
        private StandardDialogueParser _parser;
        private DialogueDataCollection _dialogueData;
        
        public StandardDialogueRuntime(DialogueManager manager)
        {
            _parser = new StandardDialogueParser(manager);
        }

        public StandardDialogueRuntime(DialogueManager manager, Dictionary<string, System.Func<NbkTokenizer, IDialogueData>> customSyntax)
        {
            _parser = new StandardDialogueParser(manager, customSyntax);
        }

        public void Parse(string script)
        {
            _dialogueData = _parser.Parse(script);
        }

        public void ChangePhase(int phase)
        {
            _dialogueData.CurrentPhase = phase;
        }
        
        public IEnumerator<IDialogueData> GetEnumerator()
        {
            while (true)
            {
                var data = _dialogueData.GetNext();
                if (data == null)
                    yield break;
                yield return data;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            while (true)
            {
                var data = _dialogueData.GetNext();
                if (data == null)
                    yield break;
                yield return data;
            }
        }
    }
}
