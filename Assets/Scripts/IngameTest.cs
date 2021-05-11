using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nabuki;

public class IngameTest : MonoBehaviour
{
    public TextAsset dialog;
    public StandardDialogue manager;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        //DialogueManager.Source = new DialogueSource("Images/", "Sounds/", "Prefabs/", DialogueSourceType.Addressable);
        //DialogueManager.Now.data = new NbkData() { playerName = "플레이어" };
        //DialogueManager.Now.Play(dialog.text);
        //yield return new WaitUntil(() => DialogueManager.Now.Ended);
        //Debug.Log("Dialogue ended. Result phase: " + DialogueManager.Now.Phase);

        manager.data = new NbkData() { playerName = "플레이어" };
        manager.Play(dialog.text);
        yield return new WaitUntil(() => manager.Ended);
    }
}