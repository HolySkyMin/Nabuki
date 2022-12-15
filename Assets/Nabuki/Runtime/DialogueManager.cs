using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NaughtyAttributes;

namespace Nabuki
{
    /// <summary>
    /// Base class for all Nabuki-derived dialogue components.
    /// </summary>
    public abstract class DialogueManager : MonoBehaviour
    {
        /// <summary>
        /// Source of the dialogue component, capable of loading scripts, images, sounds and objects.
        /// </summary>
        public DialogueSource Source => source;

        /// <summary>
        /// Displayer of the dialogue component. Text content will be shown by this.
        /// </summary>
        public DialogueDisplayer Displayer => displayer;

        /// <summary>
        /// Logger of the dialogue component.
        /// </summary>
        public DialogueLogger Logger => logger;

        /// <summary>
        /// Indicates whether dialogue logging is enabled.
        /// </summary>
        public bool LogEnabled => enableLog;

        /// <summary>
        /// Indicates whether the dialogue has ended.
        /// </summary>
        public bool Ended { get; private set; }

        /// <summary>
        /// Current phase of the dialogue.
        /// </summary>
        public int Phase => _dialogue.CurrentPhase;

        /// <summary>
        /// Current keyword of player. This is used only when no characters are designated as player.
        /// </summary>
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

        /// <summary>
        /// Sets the dialogue parser.
        /// </summary>
        /// <param name="parser">Parser object which implements <see cref="IDialogueParser"/> interface.</param>
        public void SetParser(IDialogueParser parser)
        {
            _parser = parser;
        }

        /// <summary>
        /// Plays the dialogue with given script file path.
        /// </summary>
        /// <param name="filePath">Relative path of desired script file.</param>
        public void Play(string filePath)
        {
            StartCoroutine(PlayAsync(filePath));
        }

        /// <summary>
        /// Plays the dialogue with given script file path.
        /// </summary>
        /// <param name="filePath">Relative path of desired script file.</param>
        /// <returns></returns>
        public IEnumerator PlayAsync(string filePath)
        {
            // Initializes everything.
            Initialize();
            displayer.Initialize();
            logger.Initialize();
            Ended = false;

            string textData = string.Empty;
            yield return source.GetDialogueAsync(filePath, result => { textData = result; });

            var before = OnBeforePlay();
            while (before.MoveNext())
                yield return before.Current;

            _dialogue = _parser.Parse(textData);
            yield return ExecuteDialogue();

            var after = OnAfterPlay();
            while (after.MoveNext())
                yield return after.Current;

            Ended = true;
        }

        /// <summary>
        /// Initializes the component before playing dialogue.
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// Triggered right before the dialogue execution.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator OnBeforePlay()
        {
            yield break;
        }

        /// <summary>
        /// Triggered right after the dialogue execution.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator OnAfterPlay()
        {
            yield break;
        }
        
        /// <summary>
        /// Executes the dialogue. Dialogue should be parsed before calling this.
        /// </summary>
        /// <returns></returns>
        protected IEnumerator ExecuteDialogue()
        {
            foreach (var data in _dialogue)
            {
                if (data.Accept(this))
                    yield return data.Execute(this);
            }
        }

        /// <summary>
        /// Manually sets the phase of current dialogue.
        /// </summary>
        /// <param name="phase"></param>
        public void SetPhase(int phase)
        {
            _dialogue.CurrentPhase = phase;
        }

        /// <summary>
        /// Sets the keyword of player.
        /// </summary>
        /// <param name="keyword"></param>
        public void SetPlayerKeyword(string keyword)
        {
            playerKeyword = keyword;
        }

        /// <summary>
        /// Returns the player's name.
        /// </summary>
        /// <returns></returns>
        public abstract string GetPlayerName();
    }
}