using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Nabuki
{
    public class DialogueParser
    {
        bool ifExist;
        DialogueDataCondition ifData;
        int ifIndex;

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

                var command = reader.ReadLine().Split('\t');

                if (command.Length < 1 || (command.Length == 1 && command[0] == ""))
                    continue;

                try
                {
                    if (command[0] == "#")
                        continue;

                    ParseLine(ref data, ref phase, command);
                }
                catch (Exception e) { throw new NbkDialogueParseException(line, e); }
            }

            return data;
        }

        void ParseLine(ref Dictionary<int, List<IDialogue>> data, ref int phase, params string[] command)
        {
            // Check and parse dialogue text data first.
            if(command[0] != "")
            {
                var newDialog = new DialogueData() { talker = command[0], text = command[2], voiceKey = "" };
                if (command[1] != "")
                    ParseLine(ref data, ref phase, "", "setsprite", command[0], command[1]);
                for(int i = 3; i < command.Length; i++)
                {
                    var tag = command[i].Split(':');
                    switch(tag[0])
                    {
                        case "voice":
                            newDialog.voiceKey = tag[1];
                            break;
                        case "cps":
                            newDialog.cps = int.Parse(tag[1]);
                            break;
                        case "unskippable":
                            newDialog.unstoppable = true;
                            break;
                    }
                }
                if (ifExist)
                    ifData.conditions[ifIndex].Add(newDialog);
                else
                    data[phase].Add(newDialog);
                Debug.Log("Added dialog. Current phase: " + phase);
                return;
            }

            IDialogue newData = null;
            // Now command[0] is no text, so it is a dialogue command data.
            // Command depends on command[1].
            switch(command[1])
            {
                case "phase":
                    phase = int.Parse(command[2]);
                    if (!data.ContainsKey(phase))
                        data.Add(phase, new List<IDialogue>());
                    Debug.Log("Changed phase to " + phase);
                    return;

                #region System Data
                case "define":
                    newData = new DialogueDataSystem() { type = 0, variableKey = command[2] };
                    switch(command[3])
                    {
                        case "int":
                            ((DialogueDataSystem)newData).variableType = NbkVariableType.Int;
                            ((DialogueDataSystem)newData).value = 0;
                            break;
                        case "float":
                            ((DialogueDataSystem)newData).variableType = NbkVariableType.Float;
                            ((DialogueDataSystem)newData).value = 0f;
                            break;
                        case "bool":
                            ((DialogueDataSystem)newData).variableType = NbkVariableType.Bool;
                            ((DialogueDataSystem)newData).value = false;
                            break;
                        case "string":
                            ((DialogueDataSystem)newData).variableType = NbkVariableType.String;
                            ((DialogueDataSystem)newData).value = "";
                            break;
                    }
                    if(command.Length > 4) // If initializer exists
                    {
                        if (((DialogueDataSystem)newData).variableType == NbkVariableType.Int)
                            ((DialogueDataSystem)newData).value = int.Parse(command[4]);
                        else if (((DialogueDataSystem)newData).variableType == NbkVariableType.Float)
                            ((DialogueDataSystem)newData).value = float.Parse(command[4]);
                        else if (((DialogueDataSystem)newData).variableType == NbkVariableType.Bool)
                            ((DialogueDataSystem)newData).value = bool.Parse(command[4]);
                        else if (((DialogueDataSystem)newData).variableType == NbkVariableType.String)
                            ((DialogueDataSystem)newData).value = command[4];
                    }
                    break;
                case "set":
                    newData = new DialogueDataSystem() { type = 1, variableKey = command[2], value = command[3] };
                    break;
                case "nextphase":
                    newData = new DialogueDataSystem() { type = 2, phase = int.Parse(command[2]) };
                    break;
                case "playmusic":
                    newData = new DialogueDataSystem() { type = 3, musicKey = command[2] };
                    break;
                case "playse":
                    newData = new DialogueDataSystem() { type = 4, musicKey = command[2] };
                    break;
                #endregion

                case "select":
                    newData = new DialogueDataSelect() { select = new Dictionary<int, string>() };
                    var texts = command[2].Split('|');
                    var nums = new List<int>();
                    if (command[3] == "")
                        for (int i = 0; i < texts.Length; i++)
                            nums.Add(i);
                    else
                    {
                        var numtexts = command[3].Split(',');
                        if (texts.Length > numtexts.Length)
                            throw new NbkWrongSyntaxException("Select result must be fully exist.");
                        for (int i = 0; i < texts.Length; i++)
                            nums.Add(int.Parse(numtexts[i]));
                    }
                    for (int i = 0; i < texts.Length; i++)
                        ((DialogueDataSelect)newData).select.Add(nums[i], texts[i]);

                    if(command.Length > 4 && command[4] != "")
                    {
                        ((DialogueDataSelect)newData).storeInVariable = true;
                        ((DialogueDataSelect)newData).variableKey = command[4];
                    }
                    break;

                #region Condition Data
                case "if":
                    ifData = new DialogueDataCondition() 
                    { 
                        conditions = new List<List<IDialogue>>(),
                        states = new List<NbkConditionSet>()
                    };
                    ifExist = true;
                    ifIndex = 0;

                    ifData.states.Add(ParseConditions(false, command));
                    ifData.conditions.Add(new List<IDialogue>());
                    return;
                case "elseif":
                    ifIndex++;
                    ifData.conditions.Add(new List<IDialogue>());
                    ifData.states.Add(ParseConditions(false, command));
                    return;
                case "else":
                    ifIndex++;
                    ifData.conditions.Add(new List<IDialogue>());
                    ifData.states.Add(ParseConditions(true));
                    return;
                case "endif":
                    data[phase].Add(ifData);
                    ifExist = false;
                    return;
                #endregion

                case "character":
                    newData = new DialogueDataCharacter() { type = 0, characterKey = command[2], spriteKey = command[3], state = 0 };
                    if (command.Length > 4 && command[4] != "")
                        ((DialogueDataCharacter)newData).state = int.Parse(command[4]);
                    break;
            }

            if (ifExist)
                ifData.conditions[ifIndex].Add(newData);
            else
                data[phase].Add(newData);
            Debug.Log("Added data. Current phase: " + phase);
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
                condSet.conditions.Add(new NbkCondition(left, right, comp));
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