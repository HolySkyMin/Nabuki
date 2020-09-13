using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public class DialogueCharacter : MonoBehaviour
    {
        public SpriteRenderer image;
        public Transform body;
        
        [HideInInspector] public string key;
        [HideInInspector] public string charaName;
        [HideInInspector] public DialogueField field;

        Vector2 position;

        public void Set(string k, string n, DialogueField f)
        {
            key = k;
            charaName = n;
            field = f;
            position = new Vector2(0.5f, 0.5f);

            body.SetParent(field.transform);
            body.localPosition = field.GetPosition(position);
            body.localScale = Vector3.one;
        }

        public void SetPosition(Vector2 newPosition)
        {
            position = newPosition;
            body.localPosition = field.GetPosition(position);
        }

        public IEnumerator Move(Vector2 goal, float time)
        {
            yield break;
        }

        public IEnumerator Scale(float goal, float time)
        {
            yield break;
        }

        public IEnumerator FadeIn(float time)
        {
            yield break;
        }

        public IEnumerator FadeOut(float time)
        {
            yield break;
        }

        public IEnumerator NodUp()
        {
            yield break;
        }

        public IEnumerator NodDown()
        {
            yield break;
        }
    }
}