using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Nabuki
{
    public class Barker : MonoBehaviour
    {
        public TextMeshPro text;

        public void Bark(string context, float activeTime)
        {
            text.text = context;

            StartCoroutine(BarkCoroutine(activeTime));
        }

        public IEnumerator WaitForBark(string context, float activeTime)
        {
            text.text = context;
            yield return BarkCoroutine(activeTime);
        }

        IEnumerator BarkCoroutine(float time)
        {
            yield return Appear();
            yield return new WaitForSeconds(time);
            yield return Disappear();
        }

        public virtual IEnumerator Appear()
        {
            gameObject.SetActive(true);
            yield break;
        }

        public virtual IEnumerator Disappear()
        {
            gameObject.SetActive(false);
            yield break;
        }
    }
}