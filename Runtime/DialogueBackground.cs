using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public abstract class DialogueBackground : MonoBehaviour
    {
        private protected DialogueField Field => field;

        private protected Sprite DefaultSprite => defaultSprite;

        [SerializeField] DialogueField field;
        [SerializeField] Sprite defaultSprite;

        public abstract void Show();

        public abstract void Hide();

        public abstract void SetSprite(Sprite sprite);

        public abstract void SetPosition(Vector2 position);

        public abstract void SetScale(float scale);

        public abstract IEnumerator Move(Vector2 goal, float time);

        public abstract IEnumerator Scale(float goal, float time);

        public abstract IEnumerator FadeIn(float time);

        public abstract IEnumerator FadeOut(float time);

        public abstract IEnumerator CrossFade(Sprite nextSprite, float time);
    }
}