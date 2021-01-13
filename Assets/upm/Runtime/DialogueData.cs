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
        public bool hideName;

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
                    if (hideName)
                        realTalker = "???";
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
                            try { valueword = dialog.data.GetVariable(keyword).value; }
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
        public bool dontChangePhase;

        public IEnumerator Run(DialogueManager dialog)
        {
            yield return dialog.selector.ShowSelect(select, result =>
            {
                if (storeInVariable)
                    dialog.data.SetVariable(variableKey, result.ToString());
                if (!dontChangePhase)
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
                    //var sprite = DialogueManager.Source.GetSprite(fileName);
                    yield return DialogueManager.Source.GetSpriteAsync(fileName, (sprite) =>
                    {
                        dialog.characters[characterKey].image.sprite = sprite == null ? dialog.characters[characterKey].defaultSprite : sprite;
                    });
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
                case 5:  // show
                    dialog.characters[characterKey].Show();
                    break;
                case 6:  // hide
                    dialog.characters[characterKey].Hide();
                    break;
                case 10: // move - animation index starts with 10
                    if (shouldWait)
                        yield return dialog.characters[characterKey].Move(position, duration);
                    else
                        dialog.StartCoroutine(dialog.characters[characterKey].Move(position, duration));
                    break;
                case 11: // scale
                    if (shouldWait)
                        yield return dialog.characters[characterKey].Scale(scale, duration);
                    else
                        dialog.StartCoroutine(dialog.characters[characterKey].Scale(scale, duration));
                    break;
                case 12: // fadein
                    if (shouldWait)
                        yield return dialog.characters[characterKey].FadeIn(duration);
                    else
                        dialog.StartCoroutine(dialog.characters[characterKey].FadeIn(duration));
                    break;
                case 13: // fadeout
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
                case 16: // blackout
                    if (shouldWait)
                        yield return dialog.characters[characterKey].Blackout(duration);
                    else
                        dialog.StartCoroutine(dialog.characters[characterKey].Blackout(duration));
                    break;
                case 17:  // colorize
                    if (shouldWait)
                        yield return dialog.characters[characterKey].Colorize(duration);
                    else
                        dialog.StartCoroutine(dialog.characters[characterKey].Colorize(duration));
                    break;
            }
            yield break;
        }
    }

    public class DialogueDataScene : IDialogue
    {
        public int type;
        public string spriteKey;
        public Vector2 position;
        public float scale;
        public float duration;
        public bool shouldWait;
        public bool isForeground;

        public IEnumerator Run(DialogueManager dialog)
        {
            switch(type)
            {
                case 0:  // scenefadein
                    if (shouldWait)
                        yield return dialog.SceneFadeIn(duration);
                    else
                        dialog.StartCoroutine(dialog.SceneFadeIn(duration));
                    break;
                case 1: // scenefadeout
                    if (shouldWait)
                        yield return dialog.SceneFadeOut(duration);
                    else
                        dialog.StartCoroutine(dialog.SceneFadeOut(duration));
                    break;
                case 2: // setbg, setfg
                    if (isForeground)
                    {
                        yield return DialogueManager.Source.GetSpriteAsync(spriteKey, (sprite) =>
                        {
                            dialog.foreground.SetSprite(sprite);
                            dialog.foreground.SetPosition(position);
                            dialog.foreground.SetScale(scale);
                        });
                    }
                    else
                    {
                        yield return DialogueManager.Source.GetSpriteAsync(spriteKey, (sprite) =>
                        {
                            dialog.background.SetSprite(sprite);
                            dialog.background.SetPosition(position);
                            dialog.background.SetScale(scale);
                        });
                    }
                    break;
                case 3: // bgshow, fgshow
                    if (isForeground)
                        dialog.foreground.Show();
                    else
                        dialog.background.Show();
                    break;
                case 4: // bghide, fghide
                    if (isForeground)
                        dialog.foreground.Hide();
                    else
                        dialog.background.Hide();
                    break;
                case 5: // bgfadein, fgfadein
                    if (shouldWait)
                        yield return isForeground ? dialog.foreground.FadeIn(duration) : dialog.background.FadeIn(duration);
                    else
                        dialog.StartCoroutine(isForeground ? dialog.foreground.FadeIn(duration) : dialog.background.FadeIn(duration));
                    break;
                case 6: // bgfadeout, fgfadeout
                    if (shouldWait)
                        yield return isForeground ? dialog.foreground.FadeOut(duration) : dialog.background.FadeOut(duration);
                    else
                        dialog.StartCoroutine(isForeground ? dialog.foreground.FadeOut(duration) : dialog.background.FadeOut(duration));
                    break;
                case 7: // bgcrossfade, fgcrossfade
                    Sprite sprite_7 = null;
                    yield return DialogueManager.Source.GetSpriteAsync(spriteKey, sprite => { sprite_7 = sprite; });

                    if (shouldWait)
                        yield return isForeground
                            ? dialog.foreground.CrossFade(sprite_7, duration)
                            : dialog.background.CrossFade(sprite_7, duration);
                    else
                        dialog.StartCoroutine(isForeground
                            ? dialog.foreground.CrossFade(sprite_7, duration)
                            : dialog.background.CrossFade(sprite_7, duration));
                    break;
                case 8: // bgmove, fgmove
                    if (shouldWait)
                        yield return isForeground ? dialog.foreground.Move(position, duration) : dialog.background.Move(position, duration);
                    else
                        dialog.StartCoroutine(isForeground ? dialog.foreground.Move(position, duration) : dialog.background.Move(position, duration));
                    break;
                case 9: // bgscale, fgscale
                    if (shouldWait)
                        yield return isForeground ? dialog.foreground.Scale(scale, duration) : dialog.background.Scale(scale, duration);
                    else
                        dialog.StartCoroutine(isForeground ? dialog.foreground.Scale(scale, duration) : dialog.background.Scale(scale, duration));
                    break;
            }
            yield break;
        }
    }

    public class DialogueDataSystem : IDialogue
    {
        public int type;
        public int phase;
        public float duration;
        public string musicKey;
        public string variableKey;
        public NbkVariableType variableType;
        public string value;

        public IEnumerator Run(DialogueManager dialog)
        {
            switch(type)
            {
                case 0: // define variables
                    dialog.data.CreateVariable(variableKey, value);
                    yield break;
                case 1: // change variable value
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
                case 5: // waitfor
                    yield return new WaitForSeconds(duration);
                    break;
            }
        }
    }

    public class DialogueDataList : IDialogue
    {
        public List<IDialogue> list;

        public DialogueDataList()
        {
            list = new List<IDialogue>();
        }

        public IEnumerator Run(DialogueManager dialog)
        {
            foreach (var dialogue in list)
                yield return dialogue.Run(dialog);
        }
    }
}