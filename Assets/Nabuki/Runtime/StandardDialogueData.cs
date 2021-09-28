using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Nabuki
{
    public static class StandardDialogueData
    {
        public class BaseSystem : IDialogueData
        {
            public int type;
            public int phase;
            public float duration;
            public string musicKey;
            public string variableKey;
            public string value;

            public bool Accept(DialogueManager dialog)
            {
                return true;
            }

            public IEnumerator Execute(DialogueManager dialog)
            {
                switch (type)
                {
                    case 0 when dialog is IFeatureVariable fVariable: // define: create a variable
                        fVariable.VariableData.CreateVariable(variableKey, value);
                        yield break;
                    case 1 when dialog is IFeatureVariable fVariable: // set: change variable's value
                        fVariable.VariableData.SetVariable(variableKey, value);
                        yield break;
                    case 2: // nextphase: set next phase
                        dialog.SetPhase(phase);
                        yield break;
                    case 3 when dialog is IFeatureAudio fAudio: // playmusic: play music
                        fAudio.Audio.PlayBGM(musicKey);
                        yield break;
                    case 4 when dialog is IFeatureAudio fAudio: // playse: play sound effect
                        fAudio.Audio.PlaySE(musicKey);
                        yield break;
                    case 5: // waitfor
                        yield return new WaitForSeconds(duration);
                        break;
                    case 6: // setplayer
                        dialog.SetPlayerKeyword(value);
                        break;
                    case 10 when dialog is IFeatureExternalAction feature: // call
                        yield return feature.CallAction(variableKey);
                        break;
                }
            }
        }

        public class Dialogue : IDialogueData
        {
            public bool isPlayer;
            public string talker;
            public string text;
            public string voiceKey;
            public int cps;
            public bool unstoppable;

            public bool Accept(DialogueManager dialog)
            {
                return true;
            }

            public IEnumerator Execute(DialogueManager dialog)
            {
                // Find character name.
                string realTalker = talker;
                switch (talker)
                {
                    case string s when s == dialog.PlayerKeyword:  // If talker is player, mark this dialogue as player's dialogue and get its name.
                        isPlayer = true;
                        realTalker = dialog.GetPlayerName();
                        break;
                    case "none":  // Monologue.
                        realTalker = "";
                        break;
                    default:
                        if (dialog is IFeatureCharacter feature)  // If dialogue supports character,
                        {
                            // Try finding its name.
                            var registered = feature.FindCharacterName(talker, out realTalker);
                            if (!registered)
                                realTalker = talker;
                        }
                        break;
                }

                // Find and replace keywords.
                var keywords = Regex.Matches(text, @"{[\w\s]*}");
                for (int i = 0; i < keywords.Count; i++)
                {
                    var keyword = keywords[i].Value.TrimStart('{').TrimEnd('}');
                    string valueword = keyword;
                    switch (keyword)
                    {
                        case string s when s == dialog.PlayerKeyword:  // {playerKeyword} should be replaced by its name.
                            valueword = dialog.GetPlayerName(); break;
                        default:
                            // If dialogue supports character and keyword is character, show its name.
                            if (dialog is IFeatureCharacter fc && fc.FindCharacterName(keyword, out valueword))
                                break;

                            // If dialogue supports variable, try find and show it.
                            if (dialog is IFeatureVariable fv)
                            {
                                try { valueword = fv.VariableData.GetVariable(keyword).value; }
                                catch { }
                            }
                            break;
                    }
                    text = text.Replace(keywords[i].Value, valueword);
                }

                if (dialog is IFeatureAudio fAudio && voiceKey != "")
                    fAudio.Audio.PlayVoice(voiceKey);

                if (dialog.LogEnabled)
                    dialog.Logger.Log(realTalker, text, voiceKey, isPlayer);
                yield return dialog.Displayer.ShowText(realTalker, text, cps, unstoppable);
            }
        }

        public class Selection : IDialogueData
        {
            public Dictionary<int, string> select;
            public bool storeInVariable;
            public string variableKey;
            public bool dontChangePhase;

            IFeatureSelection feature;

            public bool Accept(DialogueManager dialog)
            {
                feature = dialog as IFeatureSelection;
                return feature != null;
            }

            public IEnumerator Execute(DialogueManager dialog)
            {
                yield return feature.Selector.ShowSelect(select, result =>
                {
                    if (dialog is IFeatureVariable fVariable && storeInVariable)
                        fVariable.VariableData.SetVariable(variableKey, result.ToString());
                    if (!dontChangePhase)
                        dialog.SetPhase(result);
                });
            }
        }

        public class Character : IDialogueData
        {
            public int type;
            public string characterKey;
            public string characterName;
            public int slotIndex;

            IFeatureCharacter feature;

            public bool Accept(DialogueManager dialog)
            {
                feature = dialog as IFeatureCharacter;
                return feature != null;
            }

            public IEnumerator Execute(DialogueManager dialog)
            {
                switch (type)
                {
                    case 0: // character
                        if (dialog is IFeatureCharacterWithField featurePlus)
                            featurePlus.AddCharacter(characterKey, characterName, slotIndex);
                        else
                            feature.AddCharacter(characterKey, characterName);
                        break;
                    case 1: // hidename
                        feature.OverrideCharacterName(characterKey, characterName);
                        break;
                    case 2: // showname
                        feature.ResetCharacterName(characterKey);
                        break;
                }

                yield break;
            }
        }

        public class CharacterAnimation : IDialogueData
        {
            public int type;
            public string characterKey;
            public string spriteKey;
            public Vector2 position;
            public float scale;
            public float duration;
            public int state;
            public bool shouldWait;

            IFeatureCharacterWithField _feature;

            public bool Accept(DialogueManager dialog)
            {
                _feature = dialog as IFeatureCharacterWithField;
                return _feature != null;
            }

            public IEnumerator Execute(DialogueManager dialog)
            {
                switch (type)
                {
                    case 0: // setsprite
                        var fileName = string.Format("{0}_{1}", characterKey, spriteKey);
                        yield return dialog.Source.GetSpriteAsync(fileName, (sprite) =>
                        {
                            _feature.GetCharacter(characterKey).SetSprite(sprite);
                        });
                        break;
                    case 1:  // setpos
                        _feature.GetCharacter(characterKey).SetPosition(position);
                        break;
                    case 2:  // setsize
                        _feature.GetCharacter(characterKey).SetScale(new Vector3(scale, scale, 1));
                        break;
                    case 3:  // setstate
                        switch (state)
                        {
                            case 0:  // active (-) - does nothing. because default state is active!
                                break;
                            case 1:  // inactive
                                _feature.GetCharacter(characterKey).SetColor(new Color(0.5f, 0.5f, 0.5f, 1));
                                break;
                            case 2:  // blackout
                                _feature.GetCharacter(characterKey).SetColor(new Color(0, 0, 0, 1));
                                break;
                        }
                        break;
                    case 4:  // show
                        _feature.GetCharacter(characterKey).Show();
                        break;
                    case 5:  // hide
                        _feature.GetCharacter(characterKey).Hide();
                        break;
                    case 10:  // move - animation index starts with 10
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).Move(position, duration);
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).Move(position, duration));
                        break;
                    case 11:  // scale
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).Scale(scale, duration);
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).Scale(scale, duration));
                        break;
                    case 12:  // fadein
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).FadeIn(duration);
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).FadeIn(duration));
                        break;
                    case 13:  // fadeout
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).FadeOut(duration);
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).FadeOut(duration));
                        break;
                    case 14:  // nodup
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).NodUp();
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).NodUp());
                        break;
                    case 15:  // noddown
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).NodDown();
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).NodDown());
                        break;
                    case 16:  // blackout
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).Blackout(duration);
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).Blackout(duration));
                        break;
                    case 17:  // colorize
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).Colorize(duration);
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).Colorize(duration));
                        break;
                }
            }
        }

        public class Transition : IDialogueData
        {
            public int type;
            public float duration;
            public bool shouldWait;

            IFeatureTransition _feature;

            public bool Accept(DialogueManager dialog)
            {
                _feature = dialog as IFeatureTransition;
                return _feature != null;
            }

            public IEnumerator Execute(DialogueManager dialog)
            {
                switch(type)
                {
                    case 0:  // scenefadein
                        if (shouldWait)
                            yield return _feature.SceneFadeIn(duration);
                        else
                            dialog.StartCoroutine(_feature.SceneFadeIn(duration));
                        break;
                    case 1:  // scenefadeout
                        if (shouldWait)
                            yield return _feature.SceneFadeOut(duration);
                        else
                            dialog.StartCoroutine(_feature.SceneFadeOut(duration));
                        break;
                }
            }
        }

        public class Background : IDialogueData
        {
            public int type;
            public string spriteKey;
            public Vector2 position;
            public float scale;
            public float duration;
            public bool shouldWait;

            IFeatureBackground _feature;

            public bool Accept(DialogueManager dialog)
            {
                _feature = dialog as IFeatureBackground;
                return _feature != null;
            }

            public IEnumerator Execute(DialogueManager dialog)
            {
                switch (type)
                {
                    case 0: // setbg
                        yield return dialog.Source.GetSpriteAsync(spriteKey, (sprite) =>
                        {
                            _feature.Background.SetSprite(sprite);
                            _feature.Background.SetPosition(position);
                            _feature.Background.SetScale(scale);
                        });
                        break;
                    case 1: // bgshow
                        _feature.Background.Show();
                        break;
                    case 2: // bghide
                        _feature.Background.Hide();
                        break;
                    case 10: // bgfadein, tweening starts from type 10
                        if (shouldWait)
                            yield return _feature.Background.FadeIn(duration);
                        else
                            dialog.StartCoroutine(_feature.Background.FadeIn(duration));
                        break;
                    case 11: // bgfadeout
                        if (shouldWait)
                            yield return _feature.Background.FadeOut(duration);
                        else
                            dialog.StartCoroutine(_feature.Background.FadeOut(duration));
                        break;
                    case 12: // bgcrossfade
                        Sprite sprite_7 = null;
                        yield return dialog.Source.GetSpriteAsync(spriteKey, sprite => { sprite_7 = sprite; });

                        if (shouldWait)
                            yield return _feature.Background.CrossFade(sprite_7, duration);
                        else
                            dialog.StartCoroutine(_feature.Background.CrossFade(sprite_7, duration));
                        break;
                    case 13: // bgmove
                        if (shouldWait)
                            yield return _feature.Background.Move(position, duration);
                        else
                            dialog.StartCoroutine(_feature.Background.Move(position, duration));
                        break;
                    case 14: // bgscale
                        if (shouldWait)
                            yield return _feature.Background.Scale(scale, duration);
                        else
                            dialog.StartCoroutine(_feature.Background.Scale(scale, duration));
                        break;
                }
            }
        }

        public class Foreground : IDialogueData
        {
            public int type;
            public string spriteKey;
            public Vector2 position;
            public float scale;
            public float duration;
            public bool shouldWait;

            IFeatureForeground _feature;

            public bool Accept(DialogueManager dialog)
            {
                _feature = dialog as IFeatureForeground;
                return _feature != null;
            }

            public IEnumerator Execute(DialogueManager dialog)
            {
                switch (type)
                {
                    case 0: // setfg
                        yield return dialog.Source.GetSpriteAsync(spriteKey, (sprite) =>
                        {
                            _feature.Foreground.SetSprite(sprite);
                            _feature.Foreground.SetPosition(position);
                            _feature.Foreground.SetScale(scale);
                        });
                        break;
                    case 1: // fgshow
                        _feature.Foreground.Show();
                        break;
                    case 2: // fghide
                        _feature.Foreground.Hide();
                        break;
                    case 10: // fgfadein, tweening starts from type 10
                        if (shouldWait)
                            yield return _feature.Foreground.FadeIn(duration);
                        else
                            dialog.StartCoroutine(_feature.Foreground.FadeIn(duration));
                        break;
                    case 11: // fgfadeout
                        if (shouldWait)
                            yield return _feature.Foreground.FadeOut(duration);
                        else
                            dialog.StartCoroutine(_feature.Foreground.FadeOut(duration));
                        break;
                    case 12: // fgcrossfade
                        Sprite sprite_7 = null;
                        yield return dialog.Source.GetSpriteAsync(spriteKey, sprite => { sprite_7 = sprite; });

                        if (shouldWait)
                            yield return _feature.Foreground.CrossFade(sprite_7, duration);
                        else
                            dialog.StartCoroutine(_feature.Foreground.CrossFade(sprite_7, duration));
                        break;
                    case 13: // fgmove
                        if (shouldWait)
                            yield return _feature.Foreground.Move(position, duration);
                        else
                            dialog.StartCoroutine(_feature.Foreground.Move(position, duration));
                        break;
                    case 14: // fgscale
                        if (shouldWait)
                            yield return _feature.Foreground.Scale(scale, duration);
                        else
                            dialog.StartCoroutine(_feature.Foreground.Scale(scale, duration));
                        break;
                }
            }
        }

        public static Dialogue CreateDialogue(string talker, string text, string voiceKey)
            => new Dialogue() { talker = talker, text = text, voiceKey = voiceKey };

        public static Selection CreateSelection()
            => new Selection() { select = new Dictionary<int, string>() };
    }

    //public class StandardDialogueDataCondition : IDialogueData
    //{
    //    public List<List<IDialogueData>> conditions;
    //    public List<NbkConditionSet> states;

    //    int available = -1;

    //    public IEnumerator Execute(DialogueManager dialog)
    //    {
    //        for(int i = 0; i < states.Count; i++)
    //        {
    //            states[i].Link();
    //            if (states[i].isElse || states[i].Get() == true)
    //            {
    //                available = i;
    //                break;
    //            }
    //        }

    //        if (available == -1)
    //            yield break;

    //        for (int i = 0; i < conditions[available].Count; i++)
    //            yield return conditions[available][i].Execute(dialog);
    //    }
    //}

    //public class StandardDialogueDataList : IDialogueData
    //{
    //    public List<IDialogueData> list;

    //    public StandardDialogueDataList()
    //    {
    //        list = new List<IDialogueData>();
    //    }

    //    public IEnumerator Execute(DialogueManager dialog)
    //    {
    //        foreach (var dialogue in list)
    //            yield return dialogue.Execute(dialog);
    //    }
    //}
}