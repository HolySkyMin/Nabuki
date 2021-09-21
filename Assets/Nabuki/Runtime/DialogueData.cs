using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Nabuki
{
    public class DialogueData : IDialogueData
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

        public IEnumerator Execute(DialogueManager dialog)
        {
            string realTalker = talker;
            switch(talker)
            {
                case "player":
                    isPlayer = true;
                    if (dialog is IFeatureVariable fVariableLocal)
                        realTalker = fVariableLocal.VariableData.playerName;
                    break;
                case "none":
                    realTalker = "";
                    break;
                default:
                    if (dialog is IFeatureCharacter fCharacter)
                    {
                        var registered = fCharacter.FindCharacterName(talker, out realTalker);
                        if (!registered)
                            realTalker = talker;
                        if (hideName)
                            realTalker = "???";
                    }
                    break;
            }

            if (dialog is IFeatureVariable fVariable)
            {
                var keywords = Regex.Matches(text, @"{[\w\s]*}");
                for (int i = 0; i < keywords.Count; i++)
                {
                    var keyword = keywords[i].Value.TrimStart('{').TrimEnd('}');
                    string valueword;
                    switch (keyword)
                    {
                        case "player":
                            valueword = fVariable.VariableData.playerName; break;
                        default:
                            if (dialog is IFeatureCharacter fCharacter && fCharacter.FindCharacterName(keyword, out valueword))
                                break;
                            else
                            {
                                try { valueword = fVariable.VariableData.GetVariable(keyword).value; }
                                catch { valueword = keyword; }
                            }
                            break;
                    }
                    text = text.Replace(keywords[i].Value, valueword);
                }
            }

            if (dialog is IFeatureAudio fAudio && voiceKey != "")  // Parser sends voice key only when does manager support audio
                fAudio.Audio.PlayVoice(voiceKey);

            if (dialog.enableLog)
                dialog.logger.Log(realTalker, text, voiceKey, isPlayer);
            yield return dialog.displayer.ShowText(realTalker, text, displayIndex, unstoppable);
        }
    }

    public class DialogueDataSelect : IDialogueData
    {
        // Parser creates this object only when does manager support selection.

        public Dictionary<int, string> select;
        public bool storeInVariable;
        public string variableKey;
        public bool dontChangePhase;

        public IEnumerator Execute(DialogueManager dialog)
        {
            if(dialog is IFeatureSelection fSelection)
            {
                yield return fSelection.Selector.ShowSelect(select, result =>
                {
                    if (dialog is IFeatureVariable fVariable && storeInVariable)
                        fVariable.VariableData.SetVariable(variableKey, result.ToString());
                    if (!dontChangePhase)
                        dialog.SetPhase(result);
                });
            }
        }
    }

    public class DialogueDataCondition : IDialogueData
    {
        public List<List<IDialogueData>> conditions;
        public List<NbkConditionSet> states;

        int available = -1;

        public IEnumerator Execute(DialogueManager dialog)
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
                yield return conditions[available][i].Execute(dialog);
        }
    }

    public class DialogueDataCharacter : IDialogueData
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

        public IEnumerator Execute(DialogueManager dialog)
        {
            if (dialog is IFeatureCharacterWithField fCharacter)
            {
                switch (type)
                {
                    case 0: // character
                        fCharacter.AddCharacter(characterKey, spriteKey, state);
                        break;
                    case 1: // setsprite, only when does manager support character field
                        var fileName = string.Format("{0}_{1}", characterKey, spriteKey);
                        yield return dialog.source.GetSpriteAsync(fileName, (sprite) =>
                        {
                            fCharacter.GetCharacter(characterKey).image.sprite = sprite == null ? fCharacter.GetCharacter(characterKey).defaultSprite : sprite;
                        });
                        break;
                    case 2: // setpos, only when does manager support character field
                        fCharacter.GetCharacter(characterKey).SetPosition(position);
                        break;
                    case 3: // setsize, only when does manager support character field
                        fCharacter.GetCharacter(characterKey).body.localScale = new Vector3(scale, scale, 1);
                        break;
                    case 4: // setstate, only when does manager support character field
                        switch (state)
                        {
                            case 0: // active (-) - does nothing. because default state is active!
                                break;
                            case 1: // inactive
                                fCharacter.GetCharacter(characterKey).image.color = new Color(0.5f, 0.5f, 0.5f, 1);
                                break;
                            case 2: // blackout
                                fCharacter.GetCharacter(characterKey).image.color = new Color(0, 0, 0, 1);
                                break;
                        }
                        break;
                    case 5:  // show, only when does manager support character field
                        fCharacter.GetCharacter(characterKey).Show();
                        break;
                    case 6:  // hide, only when does manager support character field
                        fCharacter.GetCharacter(characterKey).Hide();
                        break;
                    case 10: // move - animation index starts with 10, only when does manager support character field
                        if (shouldWait)
                            yield return fCharacter.GetCharacter(characterKey).Move(position, duration);
                        else
                            dialog.StartCoroutine(fCharacter.GetCharacter(characterKey).Move(position, duration));
                        break;
                    case 11: // scale, only when does manager support character field
                        if (shouldWait)
                            yield return fCharacter.GetCharacter(characterKey).Scale(scale, duration);
                        else
                            dialog.StartCoroutine(fCharacter.GetCharacter(characterKey).Scale(scale, duration));
                        break;
                    case 12: // fadein, only when does manager support character field
                        if (shouldWait)
                            yield return fCharacter.GetCharacter(characterKey).FadeIn(duration);
                        else
                            dialog.StartCoroutine(fCharacter.GetCharacter(characterKey).FadeIn(duration));
                        break;
                    case 13: // fadeout, only when does manager support character field
                        if (shouldWait)
                            yield return fCharacter.GetCharacter(characterKey).FadeOut(duration);
                        else
                            dialog.StartCoroutine(fCharacter.GetCharacter(characterKey).FadeOut(duration));
                        break;
                    case 14: // nodup, only when does manager support character field
                        if (shouldWait)
                            yield return fCharacter.GetCharacter(characterKey).NodUp();
                        else
                            dialog.StartCoroutine(fCharacter.GetCharacter(characterKey).NodUp());
                        break;
                    case 15: // noddown, only when does manager support character field
                        if (shouldWait)
                            yield return fCharacter.GetCharacter(characterKey).NodDown();
                        else
                            dialog.StartCoroutine(fCharacter.GetCharacter(characterKey).NodDown());
                        break;
                    case 16: // blackout, only when does manager support character field
                        if (shouldWait)
                            yield return fCharacter.GetCharacter(characterKey).Blackout(duration);
                        else
                            dialog.StartCoroutine(fCharacter.GetCharacter(characterKey).Blackout(duration));
                        break;
                    case 17:  // colorize, only when does manager support character field
                        if (shouldWait)
                            yield return fCharacter.GetCharacter(characterKey).Colorize(duration);
                        else
                            dialog.StartCoroutine(fCharacter.GetCharacter(characterKey).Colorize(duration));
                        break;
                }
            }
            
            yield break;
        }
    }

    public class DialogueDataScene : IDialogueData
    {
        // Parser creates this object only when does manager support cg walls.
        public int type;
        public string spriteKey;
        public Vector2 position;
        public float scale;
        public float duration;
        public bool shouldWait;
        public bool isForeground;

        public IEnumerator Execute(DialogueManager dialog)
        {
            switch(type)
            {
                case 0 when dialog is IFeatureTransition fTransition:  // scenefadein
                    if (shouldWait)
                        yield return fTransition.SceneFadeIn(duration);
                    else
                        dialog.StartCoroutine(fTransition.SceneFadeIn(duration));
                    break;
                case 1 when dialog is IFeatureTransition fTransition: // scenefadeout
                    if (shouldWait)
                        yield return fTransition.SceneFadeOut(duration);
                    else
                        dialog.StartCoroutine(fTransition.SceneFadeOut(duration));
                    break;
                case 2 when !isForeground && dialog is IFeatureBackground fBackground: // setbg
                    yield return dialog.source.GetSpriteAsync(spriteKey, (sprite) =>
                    {
                        fBackground.Background.SetSprite(sprite);
                        fBackground.Background.SetPosition(position);
                        fBackground.Background.SetScale(scale);
                    });
                    break;
                case 2 when isForeground && dialog is IFeatureForeground fForeground: // setfg
                    yield return dialog.source.GetSpriteAsync(spriteKey, (sprite) =>
                    {
                        fForeground.Foreground.SetSprite(sprite);
                        fForeground.Foreground.SetPosition(position);
                        fForeground.Foreground.SetScale(scale);
                    });
                    break;
                case 3 when !isForeground && dialog is IFeatureBackground feature: // bgshow
                    feature.Background.Show();
                    break;
                case 3 when isForeground && dialog is IFeatureForeground feature: // fgshow
                    feature.Foreground.Show();
                    break;
                case 4 when !isForeground && dialog is IFeatureBackground feature: // bghide
                    feature.Background.Hide();
                    break;
                case 4 when isForeground && dialog is IFeatureForeground feature: // fghide
                    feature.Foreground.Hide();
                    break;
                case 5 when !isForeground && dialog is IFeatureBackground feature: // bgfadein
                    if (shouldWait)
                        yield return feature.Background.FadeIn(duration);
                    else
                        dialog.StartCoroutine(feature.Background.FadeIn(duration));
                    break;
                case 5 when isForeground && dialog is IFeatureForeground feature: // fgfadein
                    if (shouldWait)
                        yield return feature.Foreground.FadeIn(duration);
                    else
                        dialog.StartCoroutine(feature.Foreground.FadeIn(duration));
                    break;
                case 6 when !isForeground && dialog is IFeatureBackground feature: // bgfadeout
                    if (shouldWait)
                        yield return feature.Background.FadeOut(duration);
                    else
                        dialog.StartCoroutine(feature.Background.FadeOut(duration));
                    break;
                case 6 when isForeground && dialog is IFeatureForeground feature: // fgfadeout
                    if (shouldWait)
                        yield return feature.Foreground.FadeOut(duration);
                    else
                        dialog.StartCoroutine(feature.Foreground.FadeOut(duration));
                    break;
                case 7 when !isForeground && dialog is IFeatureBackground feature: // bgcrossfade
                    Sprite sprite_7_b = null;
                    yield return dialog.source.GetSpriteAsync(spriteKey, sprite => { sprite_7_b = sprite; });

                    if (shouldWait)
                        yield return feature.Background.CrossFade(sprite_7_b, duration);
                    else
                        dialog.StartCoroutine(feature.Background.CrossFade(sprite_7_b, duration));
                    break;
                case 7 when isForeground && dialog is IFeatureForeground feature: // fgcrossfade
                    Sprite sprite_7_f = null;
                    yield return dialog.source.GetSpriteAsync(spriteKey, sprite => { sprite_7_f = sprite; });

                    if (shouldWait)
                        yield return feature.Foreground.CrossFade(sprite_7_f, duration);
                    else
                        dialog.StartCoroutine(feature.Foreground.CrossFade(sprite_7_f, duration));
                    break;
                case 8 when !isForeground && dialog is IFeatureBackground feature: // bgmove
                    if (shouldWait)
                        yield return feature.Background.Move(position, duration);
                    else
                        dialog.StartCoroutine(feature.Background.Move(position, duration));
                    break;
                case 8 when isForeground && dialog is IFeatureForeground feature: // fgmove
                    if (shouldWait)
                        yield return feature.Foreground.Move(position, duration);
                    else
                        dialog.StartCoroutine(feature.Foreground.Move(position, duration));
                    break;
                case 9 when !isForeground && dialog is IFeatureBackground feature: // bgscale
                    if (shouldWait)
                        yield return feature.Background.Scale(scale, duration);
                    else
                        dialog.StartCoroutine(feature.Background.Scale(scale, duration));
                    break;
                case 9 when isForeground && dialog is IFeatureForeground feature: // fgscale
                    if (shouldWait)
                        yield return feature.Foreground.Scale(scale, duration);
                    else
                        dialog.StartCoroutine(feature.Foreground.Scale(scale, duration));
                    break;
            }
            yield break;
        }
    }

    public class DialogueDataSystem : IDialogueData
    {
        public int type;
        public int phase;
        public float duration;
        public string musicKey;
        public string variableKey;
        public NbkVariableType variableType;
        public string value;

        public IEnumerator Execute(DialogueManager dialog)
        {
            switch(type)
            {
                case 0 when dialog is IFeatureVariable fVariable: // define variables, only when does manager support variable
                    fVariable.VariableData.CreateVariable(variableKey, value);
                    yield break;
                case 1 when dialog is IFeatureVariable fVariable: // change variable value, only when does manager support variable
                    fVariable.VariableData.SetVariable(variableKey, value);
                    yield break;
                case 2: // set next phase
                    dialog.SetPhase(phase);
                    yield break;
                case 3 when dialog is IFeatureAudio fAudio: // play music, only when does manager support audio
                    fAudio.Audio.PlayBGM(musicKey);
                    yield break;
                case 4 when dialog is IFeatureAudio fAudio: // play sound effect, only when does manager support audio
                    fAudio.Audio.PlaySE(musicKey);
                    yield break;
                case 5: // waitfor
                    yield return new WaitForSeconds(duration);
                    break;
                case 10 when dialog is IFeatureExternalAction feature: // call
                    yield return feature.CallAction(variableKey);
                    break;
            }
        }
    }

    public class DialogueDataList : IDialogueData
    {
        public List<IDialogueData> list;

        public DialogueDataList()
        {
            list = new List<IDialogueData>();
        }

        public IEnumerator Execute(DialogueManager dialog)
        {
            foreach (var dialogue in list)
                yield return dialogue.Execute(dialog);
        }
    }
}