using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nabuki;

public class IngameTest : MonoBehaviour
{
    public string dialogToPlay;
    public StandardDialogue manager;
    public StandardDialogueAudio stdAudio;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        manager.SetParser(new StandardDialogueParser(manager));
        manager.SetAudio(stdAudio);
        manager.SetVariableData(new NbkData() { playerName = "플레이어" });
        manager.Play(dialogToPlay);
        yield return new WaitUntil(() => manager.Ended);
        Debug.Log("Dialogue ended.");
    }
}