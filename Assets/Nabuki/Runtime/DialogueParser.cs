﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Nabuki
{
    public class DialogueParser
    {
        //bool ifExist;
        //DialogueDataCondition ifData;
        //int ifIndex;
        DialogueManager manager;
        Dictionary<string, Func<NbkTokenizer, IDialogue>> customSyntax;
        

        public DialogueParser(DialogueManager target)
        {
            manager = target;
            customSyntax = new Dictionary<string, Func<NbkTokenizer, IDialogue>>();
        }

        public DialogueParser(DialogueManager target, Dictionary<string, Func<NbkTokenizer, IDialogue>> syntaxPack)
        {
            manager = target;
            customSyntax = syntaxPack;
        }

        public void AddCustomSyntax(string command, Func<NbkTokenizer, IDialogue> function)
        {
            customSyntax.Add(command, function);
        }

        public Dictionary<int, List<IDialogue>> Parse(string script)
        {
            var data = new Dictionary<int, List<IDialogue>>
            {
                { 0, new List<IDialogue>() }
            };
            var line = 0;
            var phase = 0;

            var reader = new StringReader(script);
            while(reader.Peek() != -1)
            {
                line++;

                var command = reader.ReadLine();
                try { ParseLine(ref data, ref phase, command); }
                catch (Exception e) { throw new NbkDialogueParseException(line, e); }
            }
            return data;
        }

        public List<IDialogue> Interpret()
        {
            return null;
        }

        void ParseLine(ref Dictionary<int, List<IDialogue>> data, ref int phase, string line)
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
                var newDialog = new DialogueData() { talker = firstToken.content, text = param[1].content, voiceKey = "" };
                if (param[0].content != "" && manager.supportsCharacter && manager.supportsCharacterField)
                    ParseLine(ref data, ref phase, $"\tsetsprite\t{firstToken.content}\t{param[0].content}");

                var tags = tokenizer.GetTag();
                foreach(var tag in tags)
                {
                    if (tag.function == "voice" && manager.supportsAudio)
                        newDialog.voiceKey = tag.parameter.items[0];
                    else if (tag.function == "cps")
                    {
                        var succeed = int.TryParse(tag.parameter.items[0], out newDialog.cps);
                        if (!succeed) throw new NbkWrongSyntaxException("Failed to parse number (int) parameter.");
                    }
                    else if (tag.function == "unskippable")
                        newDialog.unstoppable = true;
                    else if (tag.function == "hidename")
                        newDialog.hideName = true;
                }

                data[phase].Add(newDialog);
            }
            else
            {
                // Command
                IDialogue newCommand = null;
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
                            data.Add(phase, new List<IDialogue>());
                        return;

                    case "define":
                        if (manager.supportsVariable)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                            manager.GetData().CreateVariable(param[0].content, param[1].content);
                        }
                        return;
                    case "set":
                        if (manager.supportsVariable)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                            newCommand = new DialogueDataSystem() { type = 1, variableKey = param[0].content, value = param[1].content };
                            break;
                        }
                        else return;
                    case "nextphase":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new DialogueDataSystem() { type = 2, phase = int.Parse(param[0].content) };
                        break;
                    case "playmusic":
                        if (manager.supportsAudio)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value);
                            newCommand = new DialogueDataSystem() { type = 3, musicKey = param[0].content };
                            break;
                        }
                        else return;
                    case "playse":
                        if (manager.supportsAudio)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value);
                            newCommand = new DialogueDataSystem() { type = 4, musicKey = param[0].content };
                            break;
                        }
                        else return;
                    case "waitfor":
                        param = tokenizer.GetParameter(NbkTokenType.Value);
                        newCommand = new DialogueDataSystem() { type = 5, duration = float.Parse(param[0].content) };
                        break;
                    case "call":
                        if (manager.supportsExternalFunctions)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value);
                            newCommand = new DialogueDataSystem() { type = 10, variableKey = param[0].content };
                            break;
                        }
                        else return;

                    case "select":
                        if (manager.supportsSelection)
                        {
                            newCommand = new DialogueDataSelect() { select = new Dictionary<int, string>() };

                            param = tokenizer.GetParameter(NbkTokenType.TupleString, NbkTokenType.Tuple);
                            var count = ((NbkTupleToken)param[0]).items.Length;
                            for (int i = 0; i < count; i++)
                            {
                                var succeed = int.TryParse(((NbkTupleToken)param[1]).items[i], out int dest);
                                if (!succeed) throw new NbkWrongSyntaxException("Failed to parse number (int) parameter.");
                                ((DialogueDataSelect)newCommand).select.Add(dest, ((NbkTupleToken)param[0]).items[i]);
                            }

                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                            {
                                if (tag.function == "saveto" && manager.supportsVariable)
                                {
                                    ((DialogueDataSelect)newCommand).storeInVariable = true;
                                    ((DialogueDataSelect)newCommand).variableKey = tag.parameter.items[0];
                                }
                                else if (tag.function == "saveonly")
                                    ((DialogueDataSelect)newCommand).dontChangePhase = true;
                            }
                            break;
                        }
                        else return;

                    case "character":
                        if (manager.supportsCharacter)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                            newCommand = new DialogueDataCharacter() { type = 0, characterKey = param[0].content, spriteKey = param[1].content };

                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                            {
                                if (tag.function == "field")
                                {
                                    var succeed = int.TryParse(tag.parameter.items[0], out ((DialogueDataCharacter)newCommand).state);
                                    if (!succeed) throw new NbkWrongSyntaxException("Failed to parse number (int) parameter.");
                                }
                            }
                            break;
                        }
                        else return;
                    case "setsprite":
                        if (manager.supportsCharacter && manager.supportsCharacterField)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                            newCommand = new DialogueDataCharacter() { type = 1, characterKey = param[0].content, spriteKey = param[1].content };
                            break;
                        }
                        else return;
                    case "setpos":
                        if (manager.supportsCharacter && manager.supportsCharacterField)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Tuple);
                            newCommand = new DialogueDataCharacter()
                            {
                                type = 2,
                                characterKey = param[0].content,
                                position = new Vector2(float.Parse(((NbkTupleToken)param[1]).items[0]), float.Parse(((NbkTupleToken)param[1]).items[1]))
                            };
                            break;
                        }
                        else return;
                    case "setsize":
                        if (manager.supportsCharacter && manager.supportsCharacterField)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                            newCommand = new DialogueDataCharacter() { type = 3, characterKey = param[0].content, scale = float.Parse(param[1].content) };
                            break;
                        }
                        else return;
                    case "setstate":
                        if (manager.supportsCharacter && manager.supportsCharacterField)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                            newCommand = new DialogueDataCharacter() { type = 4, characterKey = param[0].content };
                            switch (param[1].content)
                            {
                                case "inactive":
                                    ((DialogueDataCharacter)newCommand).state = 1; break;
                                case "blackout":
                                    ((DialogueDataCharacter)newCommand).state = 2; break;
                            }
                            break;
                        }
                        else return;
                    case "setchar":
                        if (manager.supportsCharacter && manager.supportsCharacterField)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value, NbkTokenType.Tuple, NbkTokenType.Value);
                            ParseLine(ref data, ref phase, $"\tsetsprite\t{param[0].content}\t{param[1].content}");
                            ParseLine(ref data, ref phase, $"\tsetpos\t{param[0].content}\t{param[2].content}");
                            ParseLine(ref data, ref phase, $"\tsetsize\t{param[0].content}\t{param[3].content}");
                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                                ParseLine(ref data, ref phase, $"\tsetstate\t{param[0].content}\t{tag.function}");
                            return;
                        }
                        else return;
                    case "show":
                        if (manager.supportsCharacter && manager.supportsCharacterField)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value);
                            newCommand = new DialogueDataCharacter() { type = 5, characterKey = param[0].content };
                            break;
                        }
                        else return;
                    case "hide":
                        if (manager.supportsCharacter && manager.supportsCharacterField)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value);
                            newCommand = new DialogueDataCharacter() { type = 6, characterKey = param[0].content };
                            break;
                        }
                        else return;

                    case "move":
                        if (manager.supportsCharacter && manager.supportsCharacterField)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Tuple, NbkTokenType.Value);
                            newCommand = new DialogueDataCharacter()
                            {
                                type = 10,
                                characterKey = param[0].content,
                                position = new Vector2(float.Parse(((NbkTupleToken)param[1]).items[0]), float.Parse(((NbkTupleToken)param[1]).items[1])),
                                duration = float.Parse(param[2].content)
                            };
                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                                if (tag.function == "wait")
                                    ((DialogueDataCharacter)newCommand).shouldWait = true;
                            break;
                        }
                        else return;
                    case "scale":
                        if (manager.supportsCharacter && manager.supportsCharacterField)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value, NbkTokenType.Value);
                            newCommand = new DialogueDataCharacter()
                            {
                                type = 11,
                                characterKey = param[0].content,
                                scale = float.Parse(param[1].content),
                                duration = float.Parse(param[2].content)
                            };
                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                                if (tag.function == "wait")
                                    ((DialogueDataCharacter)newCommand).shouldWait = true;
                            break;
                        }
                        else return;
                    case "fadein":
                        if (manager.supportsCharacter && manager.supportsCharacterField)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                            newCommand = new DialogueDataCharacter() { type = 12, characterKey = param[0].content, duration = float.Parse(param[1].content) };
                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                                if (tag.function == "wait")
                                    ((DialogueDataCharacter)newCommand).shouldWait = true;
                            break;
                        }
                        else return;
                    case "fadeout":
                        if (manager.supportsCharacter && manager.supportsCharacterField)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                            newCommand = new DialogueDataCharacter() { type = 13, characterKey = param[0].content, duration = float.Parse(param[1].content) };
                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                                if (tag.function == "wait")
                                    ((DialogueDataCharacter)newCommand).shouldWait = true;
                            break;
                        }
                        else return;
                    case "nodup":
                        if (manager.supportsCharacter && manager.supportsCharacterField)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value);
                            newCommand = new DialogueDataCharacter() { type = 14, characterKey = param[0].content };
                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                                if (tag.function == "wait")
                                    ((DialogueDataCharacter)newCommand).shouldWait = true;
                            break;
                        }
                        else return;
                    case "noddown":
                        if (manager.supportsCharacter && manager.supportsCharacterField)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value);
                            newCommand = new DialogueDataCharacter() { type = 15, characterKey = param[0].content };
                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                                if (tag.function == "wait")
                                    ((DialogueDataCharacter)newCommand).shouldWait = true;
                            break;
                        }
                        else return;
                    case "colorize":
                        if (manager.supportsCharacter && manager.supportsCharacterField)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                            newCommand = new DialogueDataCharacter() { type = 17, characterKey = param[0].content, duration = float.Parse(param[1].content) };
                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                                if (tag.function == "wait")
                                    ((DialogueDataCharacter)newCommand).shouldWait = true;
                            break;
                        }
                        else return;

                    case "scenefadein":
                        if (manager.supportsCGWall)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value);
                            newCommand = new DialogueDataScene() { type = 0, duration = float.Parse(param[0].content) };
                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                                if (tag.function == "wait")
                                    ((DialogueDataScene)newCommand).shouldWait = true;
                            break;
                        }
                        else return;
                    case "scenefadeout":
                        if (manager.supportsCGWall)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value);
                            newCommand = new DialogueDataScene() { type = 1, duration = float.Parse(param[0].content) };
                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                                if (tag.function == "wait")
                                    ((DialogueDataScene)newCommand).shouldWait = true;
                            break;
                        }
                        else return;
                    case "setbg":
                    case "setfg":
                        if (manager.supportsCGWall)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Tuple, NbkTokenType.Value);
                            newCommand = new DialogueDataScene()
                            {
                                type = 2,
                                spriteKey = param[0].content,
                                position = new Vector2(float.Parse(((NbkTupleToken)param[1]).items[0]), float.Parse(((NbkTupleToken)param[1]).items[1])),
                                scale = float.Parse(param[2].content),
                                isForeground = command.content == "setfg"
                            };
                            break;
                        }
                        else return;
                    case "bgshow":
                    case "fgshow":
                        if (manager.supportsCGWall)
                        {
                            newCommand = new DialogueDataScene() { type = 3, isForeground = command.content == "fgshow" };
                            break;
                        }
                        else return;
                    case "bghide":
                    case "fghide":
                        if (manager.supportsCGWall)
                        {
                            newCommand = new DialogueDataScene() { type = 4, isForeground = command.content == "fghide" };
                            break;
                        }
                        else return;
                    case "bgfadein":
                    case "fgfadein":
                        if (manager.supportsCGWall)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value);
                            newCommand = new DialogueDataScene() { type = 5, duration = float.Parse(param[0].content), isForeground = command.content == "fgfadein" };
                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                                if (tag.function == "wait")
                                    ((DialogueDataScene)newCommand).shouldWait = true;
                            break;
                        }
                        else return;
                    case "bgfadeout":
                    case "fgfadeout":
                        if (manager.supportsCGWall)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value);
                            newCommand = new DialogueDataScene() { type = 6, duration = float.Parse(param[0].content), isForeground = command.content == "fgfadeout" };
                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                                if (tag.function == "wait")
                                    ((DialogueDataScene)newCommand).shouldWait = true;
                            break;
                        }
                        else return;
                    case "bgcrossfade":
                    case "fgcrossfade":
                        if (manager.supportsCGWall)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                            newCommand = new DialogueDataScene()
                            {
                                type = 7,
                                spriteKey = param[0].content,
                                duration = float.Parse(param[1].content),
                                isForeground = command.content == "fgcrossfade"
                            };
                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                                if (tag.function == "wait")
                                    ((DialogueDataScene)newCommand).shouldWait = true;
                            break;
                        }
                        else return;
                    case "bgmove":
                    case "fgmove":
                        if (manager.supportsCGWall)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Tuple, NbkTokenType.Value);
                            newCommand = new DialogueDataScene()
                            {
                                type = 8,
                                position = new Vector2(float.Parse(((NbkTupleToken)param[0]).items[0]), float.Parse(((NbkTupleToken)param[0]).items[1])),
                                duration = float.Parse(param[1].content),
                                isForeground = command.content == "fgmove"
                            };
                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                                if (tag.function == "wait")
                                    ((DialogueDataScene)newCommand).shouldWait = true;
                            break;
                        }
                        else return;
                    case "bgscale":
                    case "fgscale":
                        if (manager.supportsCGWall)
                        {
                            param = tokenizer.GetParameter(NbkTokenType.Value, NbkTokenType.Value);
                            newCommand = new DialogueDataScene()
                            {
                                type = 9,
                                scale = float.Parse(param[0].content),
                                duration = float.Parse(param[1].content),
                                isForeground = command.content == "fgscale"
                            };
                            tags = tokenizer.GetTag();
                            foreach (var tag in tags)
                                if (tag.function == "wait")
                                    ((DialogueDataScene)newCommand).shouldWait = true;
                            break;
                        }
                        else return;

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
                condSet.conditions.Add(new NbkCondition(manager, left, right, comp));
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