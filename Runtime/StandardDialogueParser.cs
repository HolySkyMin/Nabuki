using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Nabuki
{
    public class StandardDialogueParser : IDialogueParser
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

                    case "define" when manager is IFeatureVariable feature:
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        feature.VariableData.CreateVariable(param[0].content, param[1].content);
                        return;
                    case "set" when manager is IFeatureVariable feature:
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.BaseSystem() { type = 1, variableKey = param[0].content, value = param[1].content };
                        break;
                    case "nextphase":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.BaseSystem() { type = 2, phase = int.Parse(param[0].content) };
                        break;
                    case "playmusic":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.BaseSystem() { type = 3, musicKey = param[0].content };
                        break;
                    case "playse":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.BaseSystem() { type = 4, musicKey = param[0].content };
                        break;
                    case "waitfor":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.BaseSystem() { type = 5, duration = float.Parse(param[0].content) };
                        break;
                    case "playeris":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.BaseSystem() { type = 6, value = param[0].content };
                        break;
                    case "call":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.BaseSystem() { type = 10, variableKey = param[0].content };
                        break;

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

                    case "character":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Character() { type = 0, characterKey = param[0].content, characterName = param[1].content };
                        break;
                    case "hidename":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Character() { type = 1, characterKey = param[0].content, characterName = param[1].content };
                        break;
                    case "showname":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Character() { type = 2, characterKey = param[0].content };
                        break;
                    case "setsprite":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation()
                        { type = 0, characterKey = param[0].content, spriteKey = param[1].content };
                        break;
                    case "setpos":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Tuple);
                        newCommand = new StandardDialogueData.CharacterAnimation()
                        {
                            type = 1,
                            characterKey = param[0].content,
                            position = new Vector2(float.Parse(((NbkTupleToken)param[1]).items[0]), float.Parse(((NbkTupleToken)param[1]).items[1]))
                        };
                        break;
                    case "setsize":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation() { type = 2, characterKey = param[0].content, scale = float.Parse(param[1].content) };
                        break;
                    case "setstate":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation() { type = 3, characterKey = param[0].content };
                        switch (param[1].content)
                        {
                            case "inactive":
                                ((StandardDialogueData.CharacterAnimation)newCommand).state = 1; break;
                            case "blackout":
                                ((StandardDialogueData.CharacterAnimation)newCommand).state = 2; break;
                        }
                        break;
                    case "setchar":
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
                        newCommand = new StandardDialogueData.CharacterAnimation() { type = 4, characterKey = param[0].content };
                        break;
                    case "hide":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation() { type = 5, characterKey = param[0].content };
                        break;

                    case "move":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Tuple, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation()
                        {
                            type = 10,
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
                        newCommand = new StandardDialogueData.CharacterAnimation()
                        {
                            type = 11,
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
                        newCommand = new StandardDialogueData.CharacterAnimation()
                        {
                            type = 12,
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
                        newCommand = new StandardDialogueData.CharacterAnimation()
                        {
                            type = 13,
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
                        newCommand = new StandardDialogueData.CharacterAnimation() { type = 14, characterKey = param[0].content, duration = float.Parse(param[1].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.CharacterAnimation)newCommand).shouldWait = true;
                        break;
                    case "fadeout":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation() { type = 15, characterKey = param[0].content, duration = float.Parse(param[1].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.CharacterAnimation)newCommand).shouldWait = true;
                        break;
                    case "nodup":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation() { type = 16, characterKey = param[0].content };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.CharacterAnimation)newCommand).shouldWait = true;
                        break;
                    case "noddown":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation() { type = 17, characterKey = param[0].content };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.CharacterAnimation)newCommand).shouldWait = true;
                        break;
                    case "colorize":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.CharacterAnimation() { type = 18, characterKey = param[0].content, duration = float.Parse(param[1].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.CharacterAnimation)newCommand).shouldWait = true;
                        break;

                    case "scenefadein":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Transition() { type = 0, duration = float.Parse(param[0].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Transition)newCommand).shouldWait = true;
                        break;
                    case "scenefadeout":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Transition() { type = 1, duration = float.Parse(param[0].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Transition)newCommand).shouldWait = true;
                        break;
                    case "setbg":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Tuple, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Background()
                        {
                            type = 0,
                            spriteKey = param[0].content,
                            position = new Vector2(float.Parse(((NbkTupleToken)param[1]).items[0]), float.Parse(((NbkTupleToken)param[1]).items[1])),
                            scale = float.Parse(param[2].content)
                        }; break;
                    case "setfg":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Tuple, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Foreground()
                        {
                            type = 0,
                            spriteKey = param[0].content,
                            position = new Vector2(float.Parse(((NbkTupleToken)param[1]).items[0]), float.Parse(((NbkTupleToken)param[1]).items[1])),
                            scale = float.Parse(param[2].content),
                        };
                        break;
                    case "bgshow":
                        newCommand = new StandardDialogueData.Background() { type = 1 };
                        break;
                    case "fgshow":
                        newCommand = new StandardDialogueData.Foreground() { type = 1 };
                        break;
                    case "bghide":
                        newCommand = new StandardDialogueData.Background() { type = 2 };
                        break;
                    case "fghide":
                        newCommand = new StandardDialogueData.Foreground() { type = 2 };
                        break;
                    case "bgfadein":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Background() { type = 10, duration = float.Parse(param[0].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Background)newCommand).shouldWait = true;
                        break;
                    case "fgfadein":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Foreground() { type = 10, duration = float.Parse(param[0].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Foreground)newCommand).shouldWait = true;
                        break;
                    case "bgfadeout":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Background() { type = 11, duration = float.Parse(param[0].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Background)newCommand).shouldWait = true;
                        break;
                    case "fgfadeout":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Foreground() { type = 11, duration = float.Parse(param[0].content) };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Foreground)newCommand).shouldWait = true;
                        break;
                    case "bgcrossfade":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Background()
                        {
                            type = 12,
                            spriteKey = param[0].content,
                            duration = float.Parse(param[1].content),
                        };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Background)newCommand).shouldWait = true;
                        break;
                    case "fgcrossfade":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Foreground()
                        {
                            type = 12,
                            spriteKey = param[0].content,
                            duration = float.Parse(param[1].content),
                        };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Foreground)newCommand).shouldWait = true;
                        break;
                    case "bgmove":
                        param = tokenizer.GetParameter(NbkTokenType.Tuple, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Background()
                        {
                            type = 13,
                            position = new Vector2(float.Parse(((NbkTupleToken)param[0]).items[0]), float.Parse(((NbkTupleToken)param[0]).items[1])),
                            duration = float.Parse(param[1].content),
                        };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Background)newCommand).shouldWait = true;
                        break;
                    case "fgmove":
                        param = tokenizer.GetParameter(NbkTokenType.Tuple, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Foreground()
                        {
                            type = 13,
                            position = new Vector2(float.Parse(((NbkTupleToken)param[0]).items[0]), float.Parse(((NbkTupleToken)param[0]).items[1])),
                            duration = float.Parse(param[1].content),
                        };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Foreground)newCommand).shouldWait = true;
                        break;
                    case "bgscale":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Background()
                        {
                            type = 14,
                            scale = float.Parse(param[0].content),
                            duration = float.Parse(param[1].content),
                        };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Background)newCommand).shouldWait = true;
                        break;
                    case "fgscale":
                        param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                        newCommand = new StandardDialogueData.Foreground()
                        {
                            type = 14,
                            scale = float.Parse(param[0].content),
                            duration = float.Parse(param[1].content),
                        };
                        tags = tokenizer.GetTag();
                        foreach (var tag in tags)
                            if (tag.function == "wait")
                                ((StandardDialogueData.Foreground)newCommand).shouldWait = true;
                        break;

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