using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public class DialogueDataCollection : IEnumerable<IDialogueData>
    {
        public int CurrentPhase
        {
            get => _currentPhase;
            set
            {
                _currentPhase = value;
                _currentIndex = -1;
            }
        }

        Dictionary<int, List<IDialogueData>> data;

        int _currentPhase, _currentIndex;

        public DialogueDataCollection()
        {
            data = new Dictionary<int, List<IDialogueData>>();
            _currentPhase = 0;
            _currentIndex = -1;
        }

        public DialogueDataCollection(Dictionary<int, List<IDialogueData>> precompiled)
        {
            data = precompiled;
            _currentPhase = 0;
            _currentIndex = -1;
        }

        public void Add(IDialogueData dialogue, int phase)
        {
            if (!data.ContainsKey(phase))
                data.Add(phase, new List<IDialogueData>());

            data[phase].Add(dialogue);
        }

        public IDialogueData GetNext()
        {
            return ++_currentIndex < data[_currentPhase].Count ? data[_currentPhase][_currentIndex] : null;
        }

        public IEnumerator<IDialogueData> GetEnumerator()
        {
            while (++_currentIndex < data[_currentPhase].Count)
                yield return data[_currentPhase][_currentIndex];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            while (++_currentIndex < data[_currentPhase].Count)
                yield return data[_currentPhase][_currentIndex];
        }
    }
}
