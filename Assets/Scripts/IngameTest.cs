using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nabuki;

public class IngameTest : MonoBehaviour
{
    public string dialogToPlay;
    public StandardDialogue manager;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        manager.data = new NbkData() { playerName = "플레이어" };
        manager.SetParser(new DialogueParser(manager));
        manager.Play(dialogToPlay);
        yield return new WaitUntil(() => manager.Ended);
        Debug.Log("Dialogue ended.");
    }
}