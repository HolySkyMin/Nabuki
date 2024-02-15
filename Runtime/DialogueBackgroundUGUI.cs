using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nabuki
{
    public class DialogueBackgroundUGUI : DialogueBackground
    {
        [SerializeField] Image[] imagePool;

        Queue<Image> _imageQueue;
        Image _current;

        void Awake()
        {
            _imageQueue = new Queue<Image>();
            foreach (var sprite in imagePool)
            {
                sprite.gameObject.SetActive(false);
                _imageQueue.Enqueue(sprite);
            }

            _current = _imageQueue.Dequeue();
        }

        public override void Show()
        {
            _current.color = new Color(_current.color.r, _current.color.g, _current.color.b, 1);
            _current.gameObject.SetActive(true);
        }

        public override void Hide()
        {
            _current.gameObject.SetActive(false);
            _imageQueue.Enqueue(_current);
            _current = _imageQueue.Dequeue();
        }

        public override void SetSprite(Sprite sprite)
        {
            _current.sprite = sprite == null ? DefaultSprite : sprite;
        }

        public override void SetPosition(Vector2 position)
        {
            _current.transform.localPosition = Field.GetPosition(position);
        }

        public override void SetScale(float scale)
        {
            _current.transform.localScale = new Vector3(scale, scale, 1);
        }

        public override IEnumerator Move(Vector2 goal, float time)
        {
            var originPos = _current.transform.localPosition;

            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                SetPosition(Vector2.Lerp(originPos, goal, progress));
                yield return null;
            }
            SetPosition(goal);
        }

        public override IEnumerator Scale(float goal, float time)
        {
            var originScale = _current.transform.localScale.x;

            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                var curScale = Mathf.Lerp(originScale, goal, progress);
                _current.transform.localScale = new Vector3(curScale, curScale, 1);
                yield return null;
            }
            _current.transform.localScale = new Vector3(goal, goal, 1);
        }

        public override IEnumerator FadeIn(float time)
        {
            _current.color = new Color(_current.color.r, _current.color.g, _current.color.b, 0);
            _current.gameObject.SetActive(true);

            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                _current.color = new Color(_current.color.r, _current.color.g, _current.color.b, progress);
                yield return null;
            }
            _current.color = new Color(_current.color.r, _current.color.g, _current.color.b, 1);
        }

        public override IEnumerator FadeOut(float time)
        {
            _current.color = new Color(_current.color.r, _current.color.g, _current.color.b, 1);

            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                _current.color = new Color(_current.color.r, _current.color.g, _current.color.b, 1 - progress);
                yield return null;
            }
            _current.color = new Color(_current.color.r, _current.color.g, _current.color.b, 0);
            Hide();
        }

        public override IEnumerator CrossFade(Sprite nextSprite, float time)
        {
            var next = _imageQueue.Dequeue();
            next.sprite = nextSprite;
            next.gameObject.SetActive(true);

            _current.color = new Color(_current.color.r, _current.color.g, _current.color.b, 1);
            next.color = new Color(next.color.r, next.color.g, next.color.b, 0);
            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                _current.color = new Color(_current.color.r, _current.color.g, _current.color.b, 1 - progress);
                next.color = new Color(next.color.r, next.color.g, next.color.b, progress);
                yield return null;
            }
            _current.color = new Color(_current.color.r, _current.color.g, _current.color.b, 0);
            next.color = new Color(next.color.r, next.color.g, next.color.b, 1);

            _current.gameObject.SetActive(false);
            _imageQueue.Enqueue(_current);
            _current = next;
        }
    }
}
