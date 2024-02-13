using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nabuki
{
    public class DialogueEvent : MonoBehaviour
    {
        [System.Serializable]
        public class EventData
        {
            public string Key { get { return _key; } }

            [SerializeField] string _key;
            [SerializeField] float _time;
            [SerializeField] UnityEvent _actions;

            public IEnumerator Invoke()
            {
                if (_actions != null)
                {
                    _actions.Invoke();
                    yield return new WaitForSeconds(_time);
                }
            }
        }

        [SerializeField] List<EventData> _events;

        public IEnumerator Call(string action)
        {
            foreach(var data in _events)
            {
                if (data.Key == action)
                    yield return data.Invoke();
            }
        }
    }
}