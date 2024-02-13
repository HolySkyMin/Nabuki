using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nabuki
{
    public class StandardDialogueCharacterUGUI : DialogueCharacter
    {
        [SerializeField] Image image;
        [SerializeField] RectTransform body;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Sprite defaultSprite;

        protected override void InitializeTransform()
        {
            transform.SetParent(Field.transform);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;

            UpdateTransform(Field.GetPosition(Position));
        }

        protected override void UpdateTransform(Vector2 convertedPosition)
        {
            body.anchoredPosition = convertedPosition;
        }

        public override void Show()
        {
            if (Field.UseSlot)
            {
                Position = Field.Slots[SlotIndex];
                Field.FillSlot(SlotIndex, Key);
            }
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
            body.gameObject.SetActive(true);
        }

        public override void Hide()
        {
            body.gameObject.SetActive(false);
        }

        public override void SetSprite(Sprite sprite, string spriteKey)
        {
            image.sprite = sprite == null ? defaultSprite : sprite;
        }

        public override void SetScale(Vector3 scale)
        {
            body.localScale = scale;
        }

        public override void SetColor(Color color)
        {
            image.color = color;
        }

        public override IEnumerator Move(Vector2 goal, float time)
        {
            var originPos = Position;

            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                Position = Vector2.Lerp(originPos, goal, progress);
                yield return null;
            }
            Position = goal;
        }

        public override IEnumerator Scale(float goal, float time)
        {
            var originScale = body.localScale.x;

            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                var curScale = Mathf.Lerp(originScale, goal, progress);
                body.localScale = new Vector3(curScale, curScale, 1);
                yield return null;
            }
            body.localScale = new Vector3(goal, goal, 1);
        }

        public override IEnumerator FadeIn(float time)
        {
            body.gameObject.SetActive(true);
            canvasGroup.alpha = 0;
            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                canvasGroup.alpha = progress;
                yield return null;
            }
            canvasGroup.alpha = 1;
        }

        public override IEnumerator FadeOut(float time)
        {
            canvasGroup.alpha = 1;
            for (var clock = 0f; clock < time; clock += Time.deltaTime)
            {
                var progress = clock / time;
                canvasGroup.alpha = 1 - progress;
                yield return null;
            }
            canvasGroup.alpha = 0;
            body.gameObject.SetActive(false);
        }

        public override IEnumerator NodUp()
        {
            var originPos = Position;
            yield return Move(originPos + new Vector2(0, 0.05f), 0.1f);
            yield return Move(originPos, 0.1f);
        }

        public override IEnumerator NodDown()
        {
            var originPos = Position;
            yield return Move(originPos - new Vector2(0, 0.05f), 0.1f);
            yield return Move(originPos, 0.1f);
        }

        public override IEnumerator Blackout(float time)
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

        public override IEnumerator Colorize(float time, bool inactive = false)
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
