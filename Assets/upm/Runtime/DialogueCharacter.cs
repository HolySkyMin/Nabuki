using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public class DialogueCharacter : MonoBehaviour
    {
        public SpriteRenderer image;
        public Transform body;
        public Sprite defaultSprite;
        
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

            body.name = "Sprite: " + key;
            body.SetParent(field.transform);
            body.localPosition = field.GetPosition(position);
            body.localScale = Vector3.one;
        }

        public void Show()
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
            body.gameObject.SetActive(true);
        }

        public void Hide()
        {
            body.gameObject.SetActive(false);
        }

        public void SetPosition(Vector2 newPosition)
        {
            position = newPosition;
            body.localPosition = field.GetPosition(position);
        }

        public IEnumerator Move(Vector2 goal, float time)
        {
            var originPos = position;

            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                SetPosition(Vector2.Lerp(originPos, goal, progress));
                yield return null;
            }
            SetPosition(goal);
        }

        public IEnumerator Scale(float goal, float time)
        {
            var originScale = body.localScale.x;

            for(var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                var curScale = Mathf.Lerp(originScale, goal, progress);
                body.localScale = new Vector3(curScale, curScale, 1);
                yield return null;
            }
            body.localScale = new Vector3(goal, goal, 1);
        }

        public IEnumerator FadeIn(float time)
        {
            body.gameObject.SetActive(true);
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                image.color = new Color(image.color.r, image.color.g, image.color.b, progress);
                yield return null;
            }
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
        }

        public IEnumerator FadeOut(float time)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                image.color = new Color(image.color.r, image.color.g, image.color.b, 1 - progress);
                yield return null;
            }
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
            body.gameObject.SetActive(false);
        }

        public IEnumerator NodUp()
        {
            var originPos = position;
            yield return Move(originPos + new Vector2(0, 0.05f), 0.1f);
            yield return Move(originPos, 0.1f);
        }

        public IEnumerator NodDown()
        {
            var originPos = position;
            yield return Move(originPos - new Vector2(0, 0.05f), 0.1f);
            yield return Move(originPos, 0.1f);
        }

        public IEnumerator Blackout(float time)
        {
            var originColor = image.color;
            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                image.color = Color.Lerp(originColor, Color.black, progress);
                yield return null;
            }
            yield break;
        }

        public IEnumerator Colorize(float time, bool inactive = false)
        {
            var originColor = image.color;
            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                image.color = Color.Lerp(originColor, inactive ? Color.gray : Color.white, progress);
                yield return null;
            }
            yield break;
        }
    }
}