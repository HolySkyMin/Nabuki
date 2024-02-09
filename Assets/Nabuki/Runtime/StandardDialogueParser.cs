using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Nabuki
{
    public class StandardDialogueParser
    {
        DialogueManager manager;
        Dictionary<string, Func<NbkTokenizer, IDialogueData>> customSyntax;

        public StandardDialogueParser(DialogueManager target)
        {
            manager = target;
            customSyntax = new Dictionary<string, Func<NbkTokenizer, IDialogueData>>();
        }

        public StandardDialogueParser(DialogueManager target, Dictionary<string, Func<NbkTokenizer, IDialogueData>> syntaxPack)
        {
            manager = target;
            customSyntax = syntaxPack;
        }

        public void AddCustomSyntax(string command, Func<NbkTokenizer, IDialogueData> function)
        {
            customSyntax.Add(command, function);
        }

        public DialogueDataCollection Parse(string script)
        {
            var data = new Dictionary<int, List<IDialogueData>>
            {
                { 0, new List<IDialogueData>() }
            };
            var line = 0;
            var phase = 0;

            var reader = new StringReader(script);
            while (reader.Peek() != -1)
            {
                line++;

                var command = reader.ReadLine();
                try { ParseLine(ref data, ref phase, command); }
                catch (Exception e) { throw new NbkDialogueParseException(line, e); }
            }

            return new DialogueDataCollection(data);
        }

        public List<IDialogueData> Interpret()
        {
            return null;
        }

        void ParseLine(ref Dictionary<int, List<IDialogueData>> data, ref int phase, string line)
        {
            var tokenizer = new NbkTokenizer(line);
            var firstToken = tokenizer.GetToken();

            // Comment
            if (firstToken.content == "#")
                return;

            // Dialogue
            if(firstToken.content != "")
            {
                var param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                var newDialog = StandardDialogueData.CreateDialogue(firstToken.content, param[1].content, string.Empty);
                if (param[0].content != "")
                    ParseLine(ref data, ref phase, $"\tsetsprite\t{firstToken.content}\t{param[0].content}");

                var tags = tokenizer.GetTag();
                foreach(var tag in tags)
                {
                    if (tag.function == "voice")
                        newDialog.voiceKey = tag.parameter.items[0];
                    else if (tag.function == "cps")
                    {
                        var succeed = int.TryParse(tag.parameter.items[0], out newDialog.cps);
                        if (!succeed) throw new NbkWrongSyntaxException("Failed to parse number (int) parameter.");
                    }
                    else if (tag.function == "unskippable")
                        newDialog.unstoppable = true;
                }

                data[phase].Add(newDialog);
            }
            else
            {
                // Command
                IDialogueData newCommand = null;
                var command = tokenizer.GetToken();
                if (command == null) return;

                List<NbkToken> param;
                List<NbkFuncToken> tags;
                switch (command.content)
                {
                    case "phase":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        int.TryParse(param[0].content, out phase);
                        if (!data.ContainsKey(phase))
                            data.Add(phase, new List<IDialogueData>());
                        return;

                    // ============ BASE SYSTEM

                    case "define" when manager is IFeatureVariable feature:
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        feature.VariableData.CreateVariable(param[0].content, param[1].content);
                        return;
                    case "set" when manager is IFeatureVariable feature:
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.System
                        {
                            command = StandardDialogueData.SystemCommand.SetValue,
                            variableKey = param[0].content, value = param[1].content
                        };
                        break;
                    case "nextphase":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.System
                        {
                            command = StandardDialogueData.SystemCommand.ChangePhase, phase = int.Parse(param[0].content)
                        };
                        break;
                    case "playmusic":
                    case "play-music":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.System
                        {
                            command = StandardDialogueData.SystemCommand.PlayMusic, musicKey = param[0].content
                        };
                        break;
                    case "playse":
                    case "play-se":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.System
                        {
                            command = StandardDialogueData.SystemCommand.PlaySoundEffect, musicKey = param[0].content
                        };
                        break;
                    case "waitfor":
                    case "wait":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.System
                        {
                            command = StandardDialogueData.SystemCommand.Wait, duration = float.Parse(param[0].content)
                        };
                        break;
                    case "playeris":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.System
                        {
                            command = StandardDialogueData.SystemCommand.SetPlayer, value = param[0].content
                        };
                        break;
                    case "call":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.System
                        {
                            command = StandardDialogueData.SystemCommand.CallAction, variableKey = param[0].content
                        };
                        break;

                    // ============ SELECTION

                    case "select":
                        newCommand = StandardDialogueData.CreateSelection();

                        param = tokenizer.GetParameter(NbkTokenType.TupleString, NbkTokenType.Tuple);
                        var count = ((NbkTupleToken)param[0]).items.Length;
                        for (int i = 0; i < count; i++)
                        {
                            var succeed = int.TryParse(((NbkTupleToken)param[1]).items[i], out int dest);
                            if (!succeed) throw new NbkWrongSyntaxException("Failed to parse number (int) parameter.");
                            ((StandardDialogueData.Selection)newCommand).select.Add(dest, ((NbkTupleToken)param[0]).items[i]);
                        }

                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                        {
                            if (tag.function == "saveto")
                            {
                                ((StandardDialogueData.Selection)newCommand).storeInVariable = true;
                                ((StandardDialogueData.Selection)newCommand).variableKey = tag.parameter.items[0];
                            }
                            else if (tag.function == "saveonly")
                                ((StandardDialogueData.Selection)newCommand).dontChangePhase = true;
                        }
                        break;

                    // ============ CHARACTER & CHARACTER ANIMATION

                    case "character":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Character
                        {
                            command = StandardDialogueData.CharacterCommand.Add, characterKey = param[0].content, characterName = param[1].content
                        };
                        break;
                    case "hidename":
                    case "hide-name":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Character { command = StandardDialogueData.CharacterCommand.HideName, characterKey = param[0].content, characterName = param[1].content };
                        break;
                    case "showname":
                    case "show-name":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Character { command = StandardDialogueData.CharacterCommand.ShowName, characterKey = param[0].content };
                        break;
                    case "setsprite":
                    case "set-char-sprite":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation
                        { command = StandardDialogueData.CharacterCommand.SetSprite, characterKey = param[0].content, spriteKey = param[1].content };
                        break;
                    case "setpos":
                    case "set-char-pos":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Tuple);
                        newCommand = new StandardDialogueData.CharacterAnimation
                        {
                            command = StandardDialogueData.CharacterCommand.SetPosition,
                            characterKey = param[0].content,
                            position = new Vector2(float.Parse(((NbkTupleToken)param[1]).items[0]), float.Parse(((NbkTupleToken)param[1]).items[1]))
                        };
                        break;
                    case "setsize":
                    case "set-char-size":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation { command = StandardDialogueData.CharacterCommand.SetSize, characterKey = param[0].content, scale = float.Parse(param[1].content) };
                        break;
                    case "setstate":
                    case "set-char-state":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation { command = StandardDialogueData.CharacterCommand.SetState, characterKey = param[0].content };
                        switch (param[1].content)
                        {
                            case "inactive":
                                ((StandardDialogueData.CharacterAnimation)newCommand).state = 1; break;
                            case "blackout":
                                ((StandardDialogueData.CharacterAnimation)newCommand).state = 2; break;
                        }
                        break;
                    case "setchar":
                    case "set-char":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value, NbkTokenType.Tuple, NbkTokenType.Value);
                        ParseLine(ref data, ref phase, $"\tsetsprite\t{param[0].content}\t{param[1].content}");
                        ParseLine(ref data, ref phase, $"\tsetpos\t{param[0].content}\t{param[2].content}");
                        ParseLine(ref data, ref phase, $"\tsetsize\t{param[0].content}\t{param[3].content}");
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            ParseLine(ref data, ref phase, $"\tsetstate\t{param[0].content}\t{tag.function}");
                        return;
                    case "show":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation { command = StandardDialogueData.CharacterCommand.Show, characterKey = param[0].content };
                        break;
                    case "hide":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation { command = StandardDialogueData.CharacterCommand.Hide, characterKey = param[0].content };
                        break;

                    case "move":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Tuple, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation
                        {
                            command = StandardDialogueData.CharacterCommand.Move,
                            characterKey = param[0].content,
                            position = new Vector2(float.Parse(((NbkTupleToken)param[1]).items[0]), float.Parse(((NbkTupleToken)param[1]).items[1])),
                            duration = float.Parse(param[2].content)
                        };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.CharacterAnimation)newCommand).shouldWait = true;
                        break;
                    case "movex":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation
                        {
                            command = StandardDialogueData.CharacterCommand.MoveX,
                            characterKey = param[0].content,
                            position = new Vector2(float.Parse(param[1].content), 0),
                            duration = float.Parse(param[2].content)
                        };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.CharacterAnimation)newCommand).shouldWait = true;
                        break;
                    case "movey":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation
                        {
                            command = StandardDialogueData.CharacterCommand.MoveY,
                            characterKey = param[0].content,
                            position = new Vector2(0, float.Parse(param[1].content)),
                            duration = float.Parse(param[2].content)
                        };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.CharacterAnimation)newCommand).shouldWait = true;
                        break;
                    case "scale":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation
                        {
                            command = StandardDialogueData.CharacterCommand.Scale,
                            characterKey = param[0].content,
                            scale = float.Parse(param[1].content),
                            duration = float.Parse(param[2].content)
                        };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.CharacterAnimation)newCommand).shouldWait = true;
                        break;
                    case "fadein":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation { command = StandardDialogueData.CharacterCommand.FadeIn, characterKey = param[0].content, duration = float.Parse(param[1].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.CharacterAnimation)newCommand).shouldWait = true;
                        break;
                    case "fadeout":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation { command = StandardDialogueData.CharacterCommand.FadeOut, characterKey = param[0].content, duration = float.Parse(param[1].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.CharacterAnimation)newCommand).shouldWait = true;
                        break;
                    case "nodup":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation { command = StandardDialogueData.CharacterCommand.NodUp, characterKey = param[0].content };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.CharacterAnimation)newCommand).shouldWait = true;
                        break;
                    case "noddown":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation { command = StandardDialogueData.CharacterCommand.NodDown, characterKey = param[0].content };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.CharacterAnimation)newCommand).shouldWait = true;
                        break;
                    case "blackout":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation { command = StandardDialogueData.CharacterCommand.Blackout, characterKey = param[0].content, duration = float.Parse(param[1].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.CharacterAnimation)newCommand).shouldWait = true;
                        break;
                    case "colorize":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation { command = StandardDialogueData.CharacterCommand.Colorize, characterKey = param[0].content, duration = float.Parse(param[1].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.CharacterAnimation)newCommand).shouldWait = true;
                        break;
                    case "colorize-fadein":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        ParseLine(ref data, ref phase, $"\tsetstate\t{param[0].content}\tblackout");

                        var totalDuration = float.Parse(param[1].content);
                        data[phase].Add(
                            new StandardDialogueData.CharacterAnimation { command = StandardDialogueData.CharacterCommand.FadeIn, characterKey = param[0].content, duration = totalDuration / 2, shouldWait = true });
                        newCommand =
                            new StandardDialogueData.CharacterAnimation { command = StandardDialogueData.CharacterCommand.Colorize, characterKey = param[0].content, duration = totalDuration / 2, shouldWait = true };
                        break;
                    case "blackout-fadeout":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);

                        var totalDuration2 = float.Parse(param[1].content);
                        data[phase].Add(
                            new StandardDialogueData.CharacterAnimation { command = StandardDialogueData.CharacterCommand.Blackout, characterKey = param[0].content, duration = totalDuration2 / 2, shouldWait = true });
                        newCommand =
                            new StandardDialogueData.CharacterAnimation { command = StandardDialogueData.CharacterCommand.FadeOut, characterKey = param[0].content, duration = totalDuration2 / 2, shouldWait = true };
                        break;

                    // ============ BACKGROUND, FOREGROUND & SCENE ANIMATION

                    case "scenefadein":
                    case "fadein-scene":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Transition { command = StandardDialogueData.TransitionCommand.SceneFadeIn, duration = float.Parse(param[0].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Transition)newCommand).shouldWait = true;
                        break;
                    case "scenefadeout":
                    case "fadeout-scene":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Transition { command = StandardDialogueData.TransitionCommand.SceneFadeOut, duration = float.Parse(param[0].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Transition)newCommand).shouldWait = true;
                        break;
                    case "show-ui":
                        newCommand = new StandardDialogueData.Transition { command = StandardDialogueData.TransitionCommand.ShowUI };
                        break;
                    case "hide-ui":
                        newCommand = new StandardDialogueData.Transition { command = StandardDialogueData.TransitionCommand.HideUI };
                        break;
                    case "setbg":
                    case "set-bg":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Tuple, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Background
                        {
                            command = StandardDialogueData.BackgroundCommand.Set,
                            spriteKey = param[0].content,
                            position = new Vector2(float.Parse(((NbkTupleToken)param[1]).items[0]), float.Parse(((NbkTupleToken)param[1]).items[1])),
                            scale = float.Parse(param[2].content)
                        }; break;
                    case "setfg":
                    case "set-fg":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Tuple, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Foreground
                        {
                            command = StandardDialogueData.BackgroundCommand.Set,
                            spriteKey = param[0].content,
                            position = new Vector2(float.Parse(((NbkTupleToken)param[1]).items[0]), float.Parse(((NbkTupleToken)param[1]).items[1])),
                            scale = float.Parse(param[2].content),
                        };
                        break;
                    case "bgshow":
                    case "show-bg":
                        newCommand = new StandardDialogueData.Background { command = StandardDialogueData.BackgroundCommand.Show };
                        break;
                    case "fgshow":
                    case "show-fg":
                        newCommand = new StandardDialogueData.Foreground { command = StandardDialogueData.BackgroundCommand.Show };
                        break;
                    case "bghide":
                    case "hide-bg":
                        newCommand = new StandardDialogueData.Background { command = StandardDialogueData.BackgroundCommand.Hide };
                        break;
                    case "fghide":
                    case "hide-fg":
                        newCommand = new StandardDialogueData.Foreground { command = StandardDialogueData.BackgroundCommand.Hide };
                        break;
                    case "bgfadein":
                    case "fadein-bg":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Background { command = StandardDialogueData.BackgroundCommand.FadeIn, duration = float.Parse(param[0].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Background)newCommand).shouldWait = true;
                        break;
                    case "fgfadein":
                    case "fadein-fg":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Foreground { command = StandardDialogueData.BackgroundCommand.FadeIn, duration = float.Parse(param[0].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Foreground)newCommand).shouldWait = true;
                        break;
                    case "bgfadeout":
                    case "fadeout-bg":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Background { command = StandardDialogueData.BackgroundCommand.FadeOut, duration = float.Parse(param[0].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Background)newCommand).shouldWait = true;
                        break;
                    case "fgfadeout":
                    case "fadeout-fg":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Foreground { command = StandardDialogueData.BackgroundCommand.FadeOut, duration = float.Parse(param[0].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Foreground)newCommand).shouldWait = true;
                        break;
                    case "bgcrossfade":
                    case "crossfade-bg":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Background
                        {
                            command = StandardDialogueData.BackgroundCommand.CrossFade,
                            spriteKey = param[0].content,
                            duration = float.Parse(param[1].content),
                        };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Background)newCommand).shouldWait = true;
                        break;
                    case "fgcrossfade":
                    case "crossfade-fg":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Foreground
                        {
                            command = StandardDialogueData.BackgroundCommand.CrossFade,
                            spriteKey = param[0].content,
                            duration = float.Parse(param[1].content),
                        };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Foreground)newCommand).shouldWait = true;
                        break;
                    case "bgmove":
                    case "move-bg":
                        param = tokenizer.GetParameter(NbkTokenType.Tuple, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Background
                        {
                            command = StandardDialogueData.BackgroundCommand.Move,
                            position = new Vector2(float.Parse(((NbkTupleToken)param[0]).items[0]), float.Parse(((NbkTupleToken)param[0]).items[1])),
                            duration = float.Parse(param[1].content),
                        };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Background)newCommand).shouldWait = true;
                        break;
                    case "fgmove":
                    case "move-fg":
                        param = tokenizer.GetParameter(NbkTokenType.Tuple, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Foreground
                        {
                            command = StandardDialogueData.BackgroundCommand.Move,
                            position = new Vector2(float.Parse(((NbkTupleToken)param[0]).items[0]), float.Parse(((NbkTupleToken)param[0]).items[1])),
                            duration = float.Parse(param[1].content),
                        };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Foreground)newCommand).shouldWait = true;
                        break;
                    case "bgscale":
                    case "scale-bg":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Background
                        {
                            command = StandardDialogueData.BackgroundCommand.Scale,
                            scale = float.Parse(param[0].content),
                            duration = float.Parse(param[1].content),
                        };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Background)newCommand).shouldWait = true;
                        break;
                    case "fgscale":
                    case "scale-fg":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Foreground
                        {
                            command = StandardDialogueData.BackgroundCommand.Scale,
                            scale = float.Parse(param[0].content),
                            duration = float.Parse(param[1].content),
                        };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Foreground)newCommand).shouldWait = true;
                        break;

                    // ============ ETC

                    case "": return;
                    default:
                        if (customSyntax.ContainsKey(command.content))
                        {
                            newCommand = customSyntax[command.content](tokenizer);
                            break;
                        }
                        else return;
                }
                data[phase].Add(newCommand);
            }
        }

        NbkConditionSet ParseConditions(bool iselse, params string[] command)
        {
            var condSet = new NbkConditionSet()
            {
                conditions = new List<NbkCondition>(),
                jointLogics = new List<NbkConditionJointLogic>(),
                isElse = iselse
            };
            for (int i = 2; ;)
            {
                if (i + 3 > command.Length)
                    break;

                var left = command[i];
                var right = command[i + 2];
                NbkCompareType comp;
                switch(command[i + 1])
                {
                    case "=":
                    case "==":
                        comp = NbkCompareType.Same; break;
                    case "!=":
                        comp = NbkCompareType.NotSame; break;
                    case ">":
                        comp = NbkCompareType.LeftBig; break;
                    case "<":
                        comp = NbkCompareType.RightBig; break;
                    case ">=":
                        comp = NbkCompareType.LeftBigSame; break;
                    case "<=":
                        comp = NbkCompareType.RightBigSame; break;
                    default:
                        throw new NbkWrongSyntaxException("Invalid compare operator.");
                }
                condSet.conditions.Add(new NbkCondition(manager as IFeatureVariable, left, right, comp));
                i += 3;

                if (i >= command.Length)
                    break;
                switch(command[i])
                {
                    case "and":
                        condSet.jointLogics.Add(NbkConditionJointLogic.AND); break;
                    case "or":
                        condSet.jointLogics.Add(NbkConditionJointLogic.OR); break;
                    default:
                        throw new NbkWrongSyntaxException("Invalid condition joint logic.");
                }
                i++;
            }
            return condSet;
        }
    }
}