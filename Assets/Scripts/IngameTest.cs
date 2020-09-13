using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nabuki;

public class IngameTest : MonoBehaviour
{
    public TextAsset dialog;

    // Start is called before the first frame update
    void Start()
    {
        DialogueManager.Source = new DialogueSource("Images/", "Sounds/", "Prefabs/");
        DialogueManager.Now.data.playerName = "플레이어";
        DialogueManager.Now.Play(dialog.text);
    }
}
