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
            string realTalker = talker;
            switch(talker)
            {
                case "player":
                    isPlayer = true;
                    if (dialog.supportsVariable)
                        realTalker = dialog.GetData().playerName;
                    break;
                case "none":
                    realTalker = "";
                    break;
                default:
                    if (dialog.supportsVariable)
                    {
                        var registered = dialog.FindCharacterName(talker, out realTalker);
                        if (!registered)
                            realTalker = talker;
                        if (hideName)
                            realTalker = "???";
                    }
                    break;
            }

            if (dialog.supportsVariable)
            {
                var keywords = Regex.Matches(text, @"{[\w\s]*}");
                for (int i = 0; i < keywords.Count; i++)
                {
                    var keyword = keywords[i].Value.TrimStart('{').TrimEnd('}');
                    string valueword;
                    switch (keyword)
                    {
                        case "player":
                            valueword = dialog.GetData().playerName; break;
                        default:
                            var charaRegistered = dialog.FindCharacterName(keyword, out valueword);
                            if (!charaRegistered)
                            {
                                try { valueword = dialog.GetData().GetVariable(keyword).value; }
                                catch { valueword = keyword; }
                            }
                            break;
                    }
                    text = text.Replace(keywords[i].Value, valueword);
                }
            }

            if (voiceKey != "")  // Parser sends voice key only when does manager support audio
                dialog.PlayVoice(voiceKey);
            if (dialog.enableLog)
                dialog.logger.Log(realTalker, text, voiceKey, isPlayer);
            yield return dialog.displayer.ShowText(realTalker, text, displayIndex, unstoppable);
        }
    }

    public class DialogueDataSelect : IDialogue
    {
        // Parser creates this object only when does manager support selection.

        public Dictionary<int, string> select;
        public bool storeInVariable;
        public string variableKey;
        public bool dontChangePhase;

        public IEnumerator Run(DialogueManager dialog)
        {
            yield return dialog.GetSelector().ShowSelect(select, result =>
            {
                if (storeInVariable)
                    dialog.GetData().SetVariable(variableKey, result.ToString());
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
        // Parser creates this object only when does manager support character.
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
                case 1: // setsprite, only when does manager support character field
                    var fileName = string.Format("{0}_{1}", characterKey, spriteKey);
                    //var sprite = DialogueManager.Source.GetSprite(fileName);
                    yield return dialog.source.GetSpriteAsync(fileName, (sprite) =>
                    {
                        dialog.GetCharacter(characterKey).image.sprite = sprite == null ? dialog.GetCharacter(characterKey).defaultSprite : sprite;
                    });
                    break;
                case 2: // setpos, only when does manager support character field
                    dialog.GetCharacter(characterKey).SetPosition(position);
                    break;
                case 3: // setsize, only when does manager support character field
                    dialog.GetCharacter(characterKey).body.localScale = new Vector3(scale, scale, 1);
                    break;
                case 4: // setstate, only when does manager support character field
                    switch (state)
                    {
                        case 0: // active (-) - does nothing. because default state is active!
                            break;
                        case 1: // inactive
                            dialog.GetCharacter(characterKey).image.color = new Color(0.5f, 0.5f, 0.5f, 1);
                            break;
                        case 2: // blackout
                            dialog.GetCharacter(characterKey).image.color = new Color(0, 0, 0, 1);
                            break;
                    }
                    break;
                case 5:  // show, only when does manager support character field
                    dialog.GetCharacter(characterKey).Show();
                    break;
                case 6:  // hide, only when does manager support character field
                    dialog.GetCharacter(characterKey).Hide();
                    break;
                case 10: // move - animation index starts with 10, only when does manager support character field
                    if (shouldWait)
                        yield return dialog.GetCharacter(characterKey).Move(position, duration);
                    else
                        dialog.StartCoroutine(dialog.GetCharacter(characterKey).Move(position, duration));
                    break;
                case 11: // scale, only when does manager support character field
                    if (shouldWait)
                        yield return dialog.GetCharacter(characterKey).Scale(scale, duration);
                    else
                        dialog.StartCoroutine(dialog.GetCharacter(characterKey).Scale(scale, duration));
                    break;
                case 12: // fadein, only when does manager support character field
                    if (shouldWait)
                        yield return dialog.GetCharacter(characterKey).FadeIn(duration);
                    else
                        dialog.StartCoroutine(dialog.GetCharacter(characterKey).FadeIn(duration));
                    break;
                case 13: // fadeout, only when does manager support character field
                    if (shouldWait)
                        yield return dialog.GetCharacter(characterKey).FadeOut(duration);
                    else
                        dialog.StartCoroutine(dialog.GetCharacter(characterKey).FadeOut(duration));
                    break;
                case 14: // nodup, only when does manager support character field
                    if (shouldWait)
                        yield return dialog.GetCharacter(characterKey).NodUp();
                    else
                        dialog.StartCoroutine(dialog.GetCharacter(characterKey).NodUp());
                    break;
                case 15: // noddown, only when does manager support character field
                    if (shouldWait)
                        yield return dialog.GetCharacter(characterKey).NodDown();
                    else
                        dialog.StartCoroutine(dialog.GetCharacter(characterKey).NodDown());
                    break;
                case 16: // blackout, only when does manager support character field
                    if (shouldWait)
                        yield return dialog.GetCharacter(characterKey).Blackout(duration);
                    else
                        dialog.StartCoroutine(dialog.GetCharacter(characterKey).Blackout(duration));
                    break;
                case 17:  // colorize, only when does manager support character field
                    if (shouldWait)
                        yield return dialog.GetCharacter(characterKey).Colorize(duration);
                    else
                        dialog.StartCoroutine(dialog.GetCharacter(characterKey).Colorize(duration));
                    break;
            }
            yield break;
        }
    }

    public class DialogueDataScene : IDialogue
    {
        // Parser creates this object only when does manager support cg walls.
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
                        yield return dialog.source.GetSpriteAsync(spriteKey, (sprite) =>
                        {
                            dialog.GetForeground().SetSprite(sprite);
                            dialog.GetForeground().SetPosition(position);
                            dialog.GetForeground().SetScale(scale);
                        });
                    }
                    else
                    {
                        yield return dialog.source.GetSpriteAsync(spriteKey, (sprite) =>
                        {
                            dialog.GetBackground().SetSprite(sprite);
                            dialog.GetBackground().SetPosition(position);
                            dialog.GetBackground().SetScale(scale);
                        });
                    }
                    break;
                case 3: // bgshow, fgshow
                    if (isForeground)
                        dialog.GetForeground().Show();
                    else
                        dialog.GetBackground().Show();
                    break;
                case 4: // bghide, fghide
                    if (isForeground)
                        dialog.GetForeground().Hide();
                    else
                        dialog.GetBackground().Hide();
                    break;
                case 5: // bgfadein, fgfadein
                    if (shouldWait)
                        yield return isForeground ? dialog.GetForeground().FadeIn(duration) : dialog.GetBackground().FadeIn(duration);
                    else
                        dialog.StartCoroutine(isForeground ? dialog.GetForeground().FadeIn(duration) : dialog.GetBackground().FadeIn(duration));
                    break;
                case 6: // bgfadeout, fgfadeout
                    if (shouldWait)
                        yield return isForeground ? dialog.GetForeground().FadeOut(duration) : dialog.GetBackground().FadeOut(duration);
                    else
                        dialog.StartCoroutine(isForeground ? dialog.GetForeground().FadeOut(duration) : dialog.GetBackground().FadeOut(duration));
                    break;
                case 7: // bgcrossfade, fgcrossfade
                    Sprite sprite_7 = null;
                    yield return dialog.source.GetSpriteAsync(spriteKey, sprite => { sprite_7 = sprite; });

                    if (shouldWait)
                        yield return isForeground
                            ? dialog.GetForeground().CrossFade(sprite_7, duration)
                            : dialog.GetBackground().CrossFade(sprite_7, duration);
                    else
                        dialog.StartCoroutine(isForeground
                            ? dialog.GetForeground().CrossFade(sprite_7, duration)
                            : dialog.GetBackground().CrossFade(sprite_7, duration));
                    break;
                case 8: // bgmove, fgmove
                    if (shouldWait)
                        yield return isForeground ? dialog.GetForeground().Move(position, duration) : dialog.GetBackground().Move(position, duration);
                    else
                        dialog.StartCoroutine(isForeground ? dialog.GetForeground().Move(position, duration) : dialog.GetBackground().Move(position, duration));
                    break;
                case 9: // bgscale, fgscale
                    if (shouldWait)
                        yield return isForeground ? dialog.GetForeground().Scale(scale, duration) : dialog.GetBackground().Scale(scale, duration);
                    else
                        dialog.StartCoroutine(isForeground ? dialog.GetForeground().Scale(scale, duration) : dialog.GetBackground().Scale(scale, duration));
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
                case 0: // define variables, only when does manager support variable
                    dialog.GetData().CreateVariable(variableKey, value);
                    yield break;
                case 1: // change variable value, only when does manager support variable
                    dialog.GetData().SetVariable(variableKey, value);
                    yield break;
                case 2: // set next phase
                    dialog.phase = phase;
                    dialog.dialogueIndex = -1;
                    yield break;
                case 3: // play music, only when does manager support audio
                    dialog.PlayBGM(musicKey);
                    yield break;
                case 4: // play sound effect, only when does manager support audio
                    dialog.PlaySE(musicKey);
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