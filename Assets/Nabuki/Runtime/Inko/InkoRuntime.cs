using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

namespace Nabuki.Inko
{
    public class InkoRuntime : IDialogueRuntime
    {
        private InkoParser _parser;
        private Story _inkStory;
        private string _entryPath;
        private List<IDialogueData> _globalTags;
        
        public InkoRuntime(DialogueManager manager, string entryPath)
        {
            _parser = new InkoParser(manager);
            _entryPath = entryPath;
        }

        public void Parse(string script)
        {
            // Assume script is Ink-JSON file.
            _inkStory = new Story(script);
            _globalTags = _parser.ParseGlobalTag(_inkStory);
            _inkStory.ChoosePathString(_entryPath);
        }

        public void ChangePhase(int phase)
        {
            // Knot switching (Ink's term for 'changing phase') is being handled by Ink Runtime automatically.
            // This is called when user 'selects' - Ink uses sequential integer index for it.
            _inkStory.ChooseChoiceIndex(phase);
        }

        public void SetEntryPoint(string knotPath)
        {
            _inkStory.ChoosePathString(knotPath);
        }
        
        public IEnumerator<IDialogueData> GetEnumerator()
        {
            foreach (var data in _globalTags)
                yield return data;
            
            while (true)
            {
                var list = _parser.ParseNextLine(ref _inkStory);
                if (list.Count == 0)
                    yield break;
                foreach (var data in list)
                    yield return data;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
