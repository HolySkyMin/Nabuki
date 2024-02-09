using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if NAUGHTY_ATTRIBUTE_EXISTS
using NaughtyAttributes;
#endif

namespace Nabuki
{
    public class DialogueField : MonoBehaviour
    {
        public bool UseSlot => useSlot;

        public List<Vector2> Slots => slots;

        [SerializeField] DialogueManager manager;
        [SerializeField] float width, height;
        [SerializeField] bool useSlot;
#if NAUGHTY_ATTRIBUTE_EXISTS
        [SerializeField, ShowIf("useSlot")] List<Vector2> slots;
#else
        [SerializeField] List<Vector2> slots;
#endif
        
        Dictionary<int, string> resident = new Dictionary<int, string>();

        public Vector3 GetPosition(Vector2 scalePos)
        {
            var zero = new Vector3(-width / 2, -height / 2);
            var one = new Vector3(width / 2, height / 2);
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
                (manager as IFeatureCharacterWithField).GetCharacter(resident[index]).Hide();
                resident[index] = character;
            }
            else
                resident.Add(index, character);
        }
    }
}