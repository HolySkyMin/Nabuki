using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public class DialogueBackground : MonoBehaviour
    {
        public DialogueField field;
        public Sprite defaultSprite;
        public SpriteRenderer[] spritePool;

        Queue<SpriteRenderer> spriteQueue;
        SpriteRenderer current;

        void Awake()
        {
            spriteQueue = new Queue<SpriteRenderer>();
            foreach (var sprite in spritePool)
            {
                sprite.gameObject.SetActive(false);
                spriteQueue.Enqueue(sprite);
            }

            current = spriteQueue.Dequeue();
        }

        public void Show()
        {
            current.color = new Color(current.color.r, current.color.g, current.color.b, 1);
            current.gameObject.SetActive(true);
        }

        public void Hide()
        {
            current.gameObject.SetActive(false);
            spriteQueue.Enqueue(current);
            current = spriteQueue.Dequeue();
        }

        public void SetSprite(Sprite sprite)
        {
            current.sprite = sprite == null ? defaultSprite : sprite;
        }

        public void SetPosition(Vector2 position)
        {
            current.transform.localPosition = field.GetPosition(position);
        }

        public void SetScale(float scale)
        {
            current.transform.localScale = new Vector3(scale, scale, 1);
        }

        public IEnumerator Move(Vector2 goal, float time)
        {
            var originPos = current.transform.localPosition;

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
            var originScale = current.transform.localScale.x;

            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                var curScale = Mathf.Lerp(originScale, goal, progress);
                current.transform.localScale = new Vector3(curScale, curScale, 1);
                yield return null;
            }
            current.transform.localScale = new Vector3(goal, goal, 1);
        }

        public IEnumerator FadeIn(float time)
        {
            current.color = new Color(current.color.r, current.color.g, current.color.b, 0);
            current.gameObject.SetActive(true);

            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                current.color = new Color(current.color.r, current.color.g, current.color.b, progress);
                yield return null;
            }
            current.color = new Color(current.color.r, current.color.g, current.color.b, 1);
        }

        public IEnumerator FadeOut(float time)
        {
            current.color = new Color(current.color.r, current.color.g, current.color.b, 1);

            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                current.color = new Color(current.color.r, current.color.g, current.color.b, 1 - progress);
                yield return null;
            }
            current.color = new Color(current.color.r, current.color.g, current.color.b, 0);
            Hide();
        }

        public IEnumerator CrossFade(Sprite nextSprite, float time)
        {
            var next = spriteQueue.Dequeue();
            next.sprite = nextSprite;
            next.gameObject.SetActive(true);

            current.color = new Color(current.color.r, current.color.g, current.color.b, 1);
            next.color = new Color(next.color.r, next.color.g, next.color.b, 0);
            for(var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                current.color = new Color(current.color.r, current.color.g, current.color.b, 1 - progress);
                next.color = new Color(next.color.r, next.color.g, next.color.b, progress);
                yield return null;
            }
            current.color = new Color(current.color.r, current.color.g, current.color.b, 0);
            next.color = new Color(next.color.r, next.color.g, next.color.b, 1);

            current.gameObject.SetActive(false);
            spriteQueue.Enqueue(current);
            current = next;
        }
    }
}