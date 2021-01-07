using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nabuki
{
    public class DialogueField : MonoBehaviour
    {
        public float width, height;

        public Vector3 GetPosition(Vector2 scalePos)
        {
            var zero = transform.localPosition - new Vector3(width, height);
            var one = transform.localPosition + new Vector3(width, height);
            return new Vector3(
                Mathf.LerpUnclamped(zero.x, one.x, scalePos.x),
                Mathf.LerpUnclamped(zero.y, one.y, scalePos.y));
        }
    }
}