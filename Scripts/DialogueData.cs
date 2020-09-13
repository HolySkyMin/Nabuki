using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Nabuki
{
    public interface IDialogue
    {
        IEnumerator Run(DialogueManager dialog);
    }

    public class DialogueData : IDialogue
    {
        public bool isPlayer;
        public string talker;
        public string spriteKey;
        public string text;
        public string voiceKey;
        public int displayIndex;
        public int cps;
        public bool unstoppable;

        public IEnumerator Run(DialogueManager dialog)
        {
            string realTalker;
            switch(talker)
            {
                case "player":
                    isPlayer = true;
                    realTalker = dialog.data.playerName;
                    break;
                case "none":
                    realTalker = "";
                    break;
                default:
                    var registered = dialog.FindCharacterName(talker, out realTalker);
                    if (!registered)
                        realTalker = talker;
                    break;
            }

            var keywords = Regex.Matches(text, @"{[\w\s]*}");
            for(int i = 0; i < keywords.Count; i++)
            {
                var keyword = keywords[i].Value.TrimStart('{').TrimEnd('}');
                string valueword;
                switch(keyword)
                {
                    case "player":
                        valueword = dialog.data.playerName; break;
                    default:
                        var charaRegistered = dialog.FindCharacterName(keyword, out valueword);
                        if(!charaRegistered)
                        {
                            try { valueword = dialog.data.GetVariable(keyword).GetValue().ToString(); }
                            catch { valueword = keyword; }
                        }
                        break;
                }
                text = text.Replace(keywords[i].Value, valueword);
            }

            if (voiceKey != "")
                dialog.audio.PlayVoice(voiceKey);
            if (dialog.enableLog)
                dialog.logger.Log(realTalker, text, voiceKey, isPlayer);
            yield return dialog.displayer[displayIndex].ShowText(realTalker, text, unstoppable);
        }
    }

    public class DialogueDataSelect : IDialogue
    {
        public Dictionary<int, string> select;
        public bool storeInVariable;
        public string variableKey;

        public IEnumerator Run(DialogueManager dialog)
        {
            yield return dialog.selector.ShowSelect(select, result =>
            {
                if (storeInVariable)
                    dialog.data.SetVariable(variableKey, result);
                else
                {
                    dialog.phase = result;
                    dialog.dialogueIndex = -1;
                }
            });
        }
    }

    public class DialogueDataCondition : IDialogue
    {
        public List<List<IDialogue>> conditions;
        public List<NbkConditionSet> states;

        int available = -1;

        public IEnumerator Run(DialogueManager dialog)
        {
            for(int i = 0; i < states.Count; i++)
            {
                states[i].Link();
                if (states[i].isElse || states[i].Get() == true)
                {
                    available = i;
                    break;
                }
            }

            if (available == -1)
                yield break;

            for (int i = 0; i < conditions[available].Count; i++)
                yield return conditions[available][i].Run(dialog);
        }
    }

    public class DialogueDataCharacter : IDialogue
    {
        public int type;
        public string characterKey;
        public string spriteKey;
        public Vector2 position;
        public float scale;
        public float duration;
        public int state;
        public bool shouldWait;

        public IEnumerator Run(DialogueManager dialog)
        {
            switch(type)
            {
                case 0: // character
                    dialog.AddCharacter(characterKey, spriteKey, state);
                    break;
                case 1: // setsprite
                    var fileName = string.Format("{0}_{1}", characterKey, spriteKey);
                    dialog.characters[characterKey].image.sprite = DialogueManager.Source.GetSprite(fileName);
                    break;
                case 2: // setpos
                    dialog.characters[characterKey].SetPosition(position);
                    break;
                case 3: // setsize
                    dialog.characters[characterKey].body.localScale = new Vector3(scale, scale, 1);
                    break;
                case 4: // setstate
                    switch(state)
                    {
                        case 0: // active (-) - does nothing. because default state is active!
                            break;
                        case 1: // inactive
                            dialog.characters[characterKey].image.color = new Color(0.5f, 0.5f, 0.5f, 1);
                            break;
                        case 2: // blackout
                            dialog.characters[characterKey].image.color = new Color(0, 0, 0, 1);
                            break;
                    }
                    break;
                case 10: // charmove - animation index starts with 10
                    if (shouldWait)
                        yield return dialog.characters[characterKey].Move(position, duration);
                    else
                        dialog.StartCoroutine(dialog.characters[characterKey].Move(position, duration));
                    break;
                case 11: // charscale
                    if (shouldWait)
                        yield return dialog.characters[characterKey].Scale(scale, duration);
                    else
                        dialog.StartCoroutine(dialog.characters[characterKey].Scale(scale, duration));
                    break;
                case 12: // charfadein
                    if (shouldWait)
                        yield return dialog.characters[characterKey].FadeIn(duration);
                    else
                        dialog.StartCoroutine(dialog.characters[characterKey].FadeIn(duration));
                    break;
                case 13: // charfadeout
                    if (shouldWait)
                        yield return dialog.characters[characterKey].FadeOut(duration);
                    else
                        dialog.StartCoroutine(dialog.characters[characterKey].FadeOut(duration));
                    break;
                case 14: // nodup
                    if (shouldWait)
                        yield return dialog.characters[characterKey].NodUp();
                    else
                        dialog.StartCoroutine(dialog.characters[characterKey].NodUp());
                    break;
                case 15: // noddown
                    if (shouldWait)
                        yield return dialog.characters[characterKey].NodDown();
                    else
                        dialog.StartCoroutine(dialog.characters[characterKey].NodDown());
                    break;
            }
            yield break;
        }
    }

    public class DialogueDataSystem : IDialogue
    {
        public int type;
        public int phase;
        public string musicKey;
        public string variableKey;
        public NbkVariableType variableType;
        public dynamic value;

        public IEnumerator Run(DialogueManager dialog)
        {
            switch(type)
            {
                case 0: // define variables
                    dialog.data.CreateVariable(variableKey, variableType, value);
                    yield break;
                case 1: // change variable value
                    var vt = dialog.data.GetVariable(variableKey).GetNbkType();
                    switch (vt)
                    {
                        case NbkVariableType.Int:
                            value = int.Parse(value);
                            break;
                        case NbkVariableType.Bool:
                            value = bool.Parse(value);
                            break;
                        case NbkVariableType.Float:
                            value = float.Parse(value);
                            break;
                    }

                    dialog.data.SetVariable(variableKey, value);
                    yield break;
                case 2: // set next phase
                    dialog.phase = phase;
                    dialog.dialogueIndex = -1;
                    yield break;
                case 3: // play music
                    dialog.audio.PlayBGM(musicKey);
                    yield break;
                case 4: // play sound effect
                    dialog.audio.PlaySE(musicKey);
                    yield break;
            }
        }
    }
}