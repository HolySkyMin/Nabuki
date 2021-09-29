﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NaughtyAttributes;

namespace Nabuki
{
    public abstract class DialogueManager : MonoBehaviour
    {
        public DialogueSource Source => source;

        public DialogueDisplayer Displayer => displayer;

        public DialogueLogger Logger => logger;

        public bool LogEnabled => enableLog;

        public bool Ended { get; private set; }

        public int Phase => _dialogue.CurrentPhase;

        public string PlayerKeyword => playerKeyword;

        [Header("Universal Components")]
        [SerializeField] DialogueSource source;
        [SerializeField] DialogueDisplayer displayer;
        [SerializeField] bool enableLog;
        [ShowIf("enableLog")]
        [SerializeField] DialogueLogger logger;
        [Header("General Config")]
        [SerializeField] string playerKeyword = "player";

        DialogueDataCollection _dialogue;
        IDialogueParser _parser;

        protected void OnDestroy()
        {
            source.Dispose();
        }

        public void SetParser(IDialogueParser parser)
        {
            _parser = parser;
        }

        public void PlayDirectly(string script)
        {
            Initialize();

            Ended = false;
            _dialogue = _parser.Parse(script);

            StartCoroutine(Play_Routine());
        }

        public void Play(string filePath)
        {
            Initialize();

            Ended = false;

            StartCoroutine(Play_WithFileLoading(filePath));
        }

        IEnumerator Play_WithFileLoading(string filePath)
        {
            string textData = string.Empty;
            yield return source.GetDialogueAsync(filePath, result => { textData = result; });

            _dialogue = _parser.Parse(textData);

            yield return Play_Routine();
        }
        
        IEnumerator Play_Routine()
        {
            foreach (var data in _dialogue)
            {
                if (data.Accept(this))
                    yield return data.Execute(this);
            }

            Ended = true;
        }

        public void SetPhase(int phase)
        {
            _dialogue.CurrentPhase = phase;
        }

        public void SetPlayerKeyword(string keyword)
        {
            playerKeyword = keyword;
        }

        public abstract string GetPlayerName();

        protected abstract void Initialize();

        protected virtual void OnDialogueEnd() { }
    }
}