using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nabuki;
using Nabuki.Inko;

public class IngameTest : MonoBehaviour
{
    public string dialogToPlay;
    public string dialogEntryPoint;
    public StandardDialogue manager;
    public StandardDialogueAudio stdAudio;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        manager.SetRuntime(new InkoRuntime(manager, dialogEntryPoint));
        manager.SetAudio(stdAudio);
        manager.SetVariableData(new NbkData() { playerName = "플레이어" });
        manager.Play(dialogToPlay);
        yield return new WaitUntil(() => manager.Ended);
        Debug.Log("Dialogue ended.");
    }
}