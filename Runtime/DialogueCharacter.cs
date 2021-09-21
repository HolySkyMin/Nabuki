using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public abstract class DialogueCharacter : MonoBehaviour
    {
        public string Key => _key;

        public string Name => _name;

        private protected int SlotIndex => _slotIndex;

        private protected DialogueField Field => _field;

        private protected Vector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                UpdateTransform(_field.GetPosition(_position));
            }
        }

        int _slotIndex;
        string _key, _name;
        Vector2 _position;
        DialogueField _field;

        public void Initialize(string key, string name, DialogueField field, int index)
        {
            _key = key;
            _name = name;
            _field = field;
            _slotIndex = index;

            Position = field.UseSlot ? field.Slots[_slotIndex] : new Vector2(0.5f, 0.5f);

            InitializeTransform();
        }

        public void SetPosition(Vector2 position)
        {
            Position = position;
        }

        private protected abstract void InitializeTransform();

        private protected abstract void UpdateTransform(Vector2 convertedPosition);

        public abstract void Show();

        public abstract void Hide();

        public abstract void SetSprite(Sprite sprite);

        public abstract void SetScale(Vector3 scale);

        public abstract void SetColor(Color color);

        public abstract IEnumerator Move(Vector2 goal, float time);

        public abstract IEnumerator Scale(float goal, float time);

        public abstract IEnumerator FadeIn(float time);

        public abstract IEnumerator FadeOut(float time);

        public abstract IEnumerator NodUp();

        public abstract IEnumerator NodDown();

        public abstract IEnumerator Blackout(float time);

        public abstract IEnumerator Colorize(float time, bool inactive = false);
    }
}