using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public class DialogueField : MonoBehaviour
    {
        public DialogueManager manager;
        public float width, height;
        public bool usesSlot;
        public List<Vector2> slots;

        Dictionary<int, string> resident = new Dictionary<int, string>();

        public Vector3 GetPosition(Vector2 scalePos)
        {
            var zero = transform.localPosition - new Vector3(width, height);
            var one = transform.localPosition + new Vector3(width, height);
            return new Vector3(
                Mathf.LerpUnclamped(zero.x, one.x, scalePos.x),
                Mathf.LerpUnclamped(zero.y, one.y, scalePos.y));
        }

        public Vector3 GetPosition(int index)
        {
            return GetPosition(slots[index]);
        }

        public void FillSlot(int index, string character)
        {
            if (resident.ContainsKey(index))
            {
                manager.GetCharacter(resident[index]).Hide();
                resident[index] = character;
            }
            else
                resident.Add(index, character);
        }
    }
}