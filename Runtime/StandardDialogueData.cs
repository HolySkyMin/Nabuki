using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Nabuki
{
    public static class StandardDialogueData
    {
        public enum SystemCommand
        {
            DefineValue, SetValue, ChangePhase, PlayMusic, PlaySoundEffect, Wait, SetPlayer, CallAction
        }

        public enum CharacterCommand
        {
            Add, HideName, ShowName, SetSprite, SetPosition, SetSize, SetState,
            Show, Hide, Move, MoveX, MoveY, Scale, FadeIn, FadeOut, NodUp, NodDown, Blackout, Colorize
        }

        public enum TransitionCommand
        {
            SceneFadeIn, SceneFadeOut, ShowUI, HideUI
        }

        public enum BackgroundCommand
        {
            Set, Show, Hide, FadeIn, FadeOut, CrossFade, Move, Scale
        }
        
        public class System : IDialogueData
        {
            public SystemCommand command;
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
                switch (command)
                {
                    case SystemCommand.DefineValue when dialog is IFeatureVariable fVariable:
                        fVariable.VariableData.CreateVariable(variableKey, value);
                        yield break;
                    case SystemCommand.SetValue when dialog is IFeatureVariable fVariable:
                        fVariable.VariableData.SetVariable(variableKey, value);
                        yield break;
                    case SystemCommand.ChangePhase:
                        dialog.SetPhase(phase);
                        yield break;
                    case SystemCommand.PlayMusic when dialog is IFeatureAudio fAudio:
                        fAudio.Audio.PlayBGM(musicKey);
                        yield break;
                    case SystemCommand.PlaySoundEffect when dialog is IFeatureAudio fAudio:
                        fAudio.Audio.PlaySE(musicKey);
                        yield break;
                    case SystemCommand.Wait:
                        yield return new WaitForSeconds(duration);
                        break;
                    case SystemCommand.SetPlayer:
                        dialog.SetPlayerKeyword(value);
                        break;
                    case SystemCommand.CallAction when dialog is IFeatureExternalAction feature:
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

                        if (dialog is IFeatureCharacterWithField feature && feature.FindCharacterName(talker, out _))
                            feature.HighlightCharacter(talker);
                        break;
                    case "none":  // Monologue.
                        realTalker = "";
                        break;
                    default:
                        if (dialog is IFeatureCharacter feature2)  // If dialogue supports character,
                        {
                            // Try finding its name.
                            var registered = feature2.FindCharacterName(talker, out realTalker);
                            if (!registered)
                                realTalker = talker;

                            if (registered && dialog is IFeatureCharacterWithField featureEx)
                                featureEx.HighlightCharacter(talker);
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
            public CharacterCommand command;
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
                switch (command)
                {
                    case CharacterCommand.Add:
                        if (dialog is IFeatureCharacterWithField featurePlus)
                            featurePlus.AddCharacter(characterKey, characterName, slotIndex);
                        else
                            feature.AddCharacter(characterKey, characterName);
                        break;
                    case CharacterCommand.HideName:
                        feature.OverrideCharacterName(characterKey, characterName);
                        break;
                    case CharacterCommand.ShowName:
                        feature.ResetCharacterName(characterKey);
                        break;
                }

                yield break;
            }
        }

        public class CharacterAnimation : IDialogueData
        {
            public CharacterCommand command;
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
                switch (command)
                {
                    case CharacterCommand.SetSprite:
                        var fileName = string.Format("{0}_{1}", characterKey, spriteKey);
                        yield return dialog.Source.GetSpriteAsync(fileName, (sprite) =>
                        {
                            _feature.GetCharacter(characterKey).SetSprite(sprite, spriteKey);
                        });
                        break;
                    case CharacterCommand.SetPosition:
                        _feature.GetCharacter(characterKey).SetPosition(position);
                        break;
                    case CharacterCommand.SetSize:
                        _feature.GetCharacter(characterKey).SetScale(new Vector3(scale, scale, 1));
                        break;
                    case CharacterCommand.SetState:
                        switch (state)
                        {
                            case 0:  // inactive (-) - does nothing. because default state is active!
                                break;
                            case 1:  // inactive
                                _feature.GetCharacter(characterKey).SetColor(new Color(0.75f, 0.75f, 0.75f));
                                break;
                            case 2:  // blackout
                                _feature.GetCharacter(characterKey).SetColor(new Color(0, 0, 0));
                                break;
                        }
                        break;
                    case CharacterCommand.Show:
                        _feature.GetCharacter(characterKey).Show();
                        break;
                    case CharacterCommand.Hide:
                        _feature.GetCharacter(characterKey).Hide();
                        break;
                    case CharacterCommand.Move:
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).Move(position, duration);
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).Move(position, duration));
                        break;
                    case CharacterCommand.MoveX:
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).MoveX(position.x, duration);
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).MoveX(position.x, duration));
                        break;
                    case CharacterCommand.MoveY:
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).MoveY(position.y, duration);
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).MoveY(position.y, duration));
                        break;
                    case CharacterCommand.Scale:
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).Scale(scale, duration);
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).Scale(scale, duration));
                        break;
                    case CharacterCommand.FadeIn:
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).FadeIn(duration);
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).FadeIn(duration));
                        break;
                    case CharacterCommand.FadeOut:
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).FadeOut(duration);
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).FadeOut(duration));
                        break;
                    case CharacterCommand.NodUp:
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).NodUp();
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).NodUp());
                        break;
                    case CharacterCommand.NodDown:
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).NodDown();
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).NodDown());
                        break;
                    case CharacterCommand.Blackout:
                        if (shouldWait)
                            yield return _feature.GetCharacter(characterKey).Blackout(duration);
                        else
                            dialog.StartCoroutine(_feature.GetCharacter(characterKey).Blackout(duration));
                        break;
                    case CharacterCommand.Colorize:
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
            public TransitionCommand command;
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
                switch(command)
                {
                    case TransitionCommand.SceneFadeIn:
                        if (shouldWait)
                            yield return _feature.SceneFadeIn(duration);
                        else
                            dialog.StartCoroutine(_feature.SceneFadeIn(duration));
                        break;
                    case TransitionCommand.SceneFadeOut:
                        if (shouldWait)
                            yield return _feature.SceneFadeOut(duration);
                        else
                            dialog.StartCoroutine(_feature.SceneFadeOut(duration));
                        break;
                    case TransitionCommand.ShowUI:
                        yield return dialog.Displayer.SetVisible(true);
                        break;
                    case TransitionCommand.HideUI:
                        yield return dialog.Displayer.SetVisible(false);
                        break;
                }
            }
        }

        public class Background : IDialogueData
        {
            public BackgroundCommand command;
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
                switch (command)
                {
                    case BackgroundCommand.Set:
                        yield return dialog.Source.GetSpriteAsync(spriteKey, (sprite) =>
                        {
                            _feature.Background.SetSprite(sprite);
                            _feature.Background.SetPosition(position);
                            _feature.Background.SetScale(scale);
                        });
                        break;
                    case BackgroundCommand.Show:
                        _feature.Background.Show();
                        break;
                    case BackgroundCommand.Hide:
                        _feature.Background.Hide();
                        break;
                    case BackgroundCommand.FadeIn:
                        if (shouldWait)
                            yield return _feature.Background.FadeIn(duration);
                        else
                            dialog.StartCoroutine(_feature.Background.FadeIn(duration));
                        break;
                    case BackgroundCommand.FadeOut:
                        if (shouldWait)
                            yield return _feature.Background.FadeOut(duration);
                        else
                            dialog.StartCoroutine(_feature.Background.FadeOut(duration));
                        break;
                    case BackgroundCommand.CrossFade:
                        Sprite sprite_7 = null;
                        yield return dialog.Source.GetSpriteAsync(spriteKey, sprite => { sprite_7 = sprite; });

                        if (shouldWait)
                            yield return _feature.Background.CrossFade(sprite_7, duration);
                        else
                            dialog.StartCoroutine(_feature.Background.CrossFade(sprite_7, duration));
                        break;
                    case BackgroundCommand.Move: 
                        if (shouldWait)
                            yield return _feature.Background.Move(position, duration);
                        else
                            dialog.StartCoroutine(_feature.Background.Move(position, duration));
                        break;
                    case BackgroundCommand.Scale:
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
            public BackgroundCommand command;
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
                switch (command)
                {
                    case BackgroundCommand.Set:
                        yield return dialog.Source.GetSpriteAsync(spriteKey, (sprite) =>
                        {
                            _feature.Foreground.SetSprite(sprite);
                            _feature.Foreground.SetPosition(position);
                            _feature.Foreground.SetScale(scale);
                        });
                        break;
                    case BackgroundCommand.Show:
                        _feature.Foreground.Show();
                        break;
                    case BackgroundCommand.Hide:
                        _feature.Foreground.Hide();
                        break;
                    case BackgroundCommand.FadeIn:
                        if (shouldWait)
                            yield return _feature.Foreground.FadeIn(duration);
                        else
                            dialog.StartCoroutine(_feature.Foreground.FadeIn(duration));
                        break;
                    case BackgroundCommand.FadeOut:
                        if (shouldWait)
                            yield return _feature.Foreground.FadeOut(duration);
                        else
                            dialog.StartCoroutine(_feature.Foreground.FadeOut(duration));
                        break;
                    case BackgroundCommand.CrossFade:
                        Sprite sprite_7 = null;
                        yield return dialog.Source.GetSpriteAsync(spriteKey, sprite => { sprite_7 = sprite; });

                        if (shouldWait)
                            yield return _feature.Foreground.CrossFade(sprite_7, duration);
                        else
                            dialog.StartCoroutine(_feature.Foreground.CrossFade(sprite_7, duration));
                        break;
                    case BackgroundCommand.Move:
                        if (shouldWait)
                            yield return _feature.Foreground.Move(position, duration);
                        else
                            dialog.StartCoroutine(_feature.Foreground.Move(position, duration));
                        break;
                    case BackgroundCommand.Scale:
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