using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

namespace Nabuki.Inko
{
    public class InkoParser
    {
        private float _animateTime;

        public List<IDialogueData> ParseGlobalTag(Story story)
        {
            var commandList = new List<IDialogueData>();
            
            foreach (var tag in story.globalTags)
            {
                var parsedTag = new InkoToken(tag, true);

                switch (parsedTag.Key)
                {
                    case "animate-time": case "animate_time": case "animate time":
                        _animateTime = float.Parse(parsedTag.Values[0]);
                        break;
                    case "character":
                        foreach (var value in parsedTag.Values)
                        {
                            var param = value.Split('/');
                            commandList.Add(new StandardDialogueData.Character
                            {
                                command = StandardDialogueData.CharacterCommand.Add,
                                characterKey = param[0],
                                characterName = param.Length > 1 ? param[1] : param[0]
                            });
                        }
                        break;
                    
                }
            }

            return commandList;
        }
        
        public List<IDialogueData> ParseNextLine(ref Story story)
        {
            var commandList = new List<IDialogueData>();
            
            // First we check whether story can be continued or not.
            if (story.canContinue)
            {
                // Get main text by continuing.
                var line = new InkoToken(story.Continue(), false);
                string talker = string.Empty, talkerSprite = string.Empty;
                if (!string.IsNullOrEmpty(line.Key))
                {
                    var keySplit = line.Key.Split('/');
                    talker = keySplit[0].Trim();
                    if (keySplit.Length > 1)
                        talkerSprite = keySplit[1].Trim();
                }
                var dialogueData = StandardDialogueData.CreateDialogue(
                    line.Values.Count > 0 ? talker : string.Empty,
                    line.Values.Count > 0 ? line.Values[0] : talker,
                    string.Empty);
                
                // Next, parse tags for next line.
                foreach (var tag in story.currentTags)
                {
                    var parsedTag = new InkoToken(tag, true);

                    switch (parsedTag.Key) // Forced low capitalization.
                    {
                        // ============ SYSTEM
                        case "animate-time": case "animate_time": case "animate time":
                            _animateTime = float.Parse(parsedTag.Values[0]);
                            break;
                        // ============ CHARACTER and CHARACTER ANIMATION
                        case "character":
                            foreach (var value in parsedTag.Values)
                            {
                                var param = value.Split('/');
                                commandList.Add(new StandardDialogueData.Character
                                {
                                    command = StandardDialogueData.CharacterCommand.Add,
                                    characterKey = param[0],
                                    characterName = param.Length > 1 ? param[1] : param[0]
                                });
                            }
                            break;
                        case "set-character": case "set_character": case "set character":
                            foreach (var value in parsedTag.Values)
                            {
                                var param = value.Split('/');
                                commandList.Add(SetCharacterSprite(param[0], param[1]));
                                commandList.Add(new StandardDialogueData.CharacterAnimation
                                {
                                    command = StandardDialogueData.CharacterCommand.SetPosition,
                                    characterKey = param[0],
                                    position = new Vector2(float.Parse(param[2]), 0),
                                    scale = 1
                                });
                            }
                            break;
                        case "show":
                            foreach (var value in parsedTag.Values)
                            {
                                var param = value.Split('/');
                                if (param.Length > 1)
                                    commandList.Add(SetCharacterSprite(param[0], param[1]));
                                commandList.Add(new StandardDialogueData.CharacterAnimation
                                {
                                    command = StandardDialogueData.CharacterCommand.Show,
                                    characterKey = param[0]
                                });
                            }
                            break;
                        case "hide":
                            foreach (var value in parsedTag.Values)
                            {
                                commandList.Add(new StandardDialogueData.CharacterAnimation
                                {
                                    command = StandardDialogueData.CharacterCommand.Hide,
                                    characterKey = value
                                });
                            }
                            break;
                        case "move": case "movex":
                            for (int i = 0; i < parsedTag.Values.Count; i++)
                            {
                                var param = parsedTag.Values[i].Split('/');
                                commandList.Add(new StandardDialogueData.CharacterAnimation
                                {
                                    command = StandardDialogueData.CharacterCommand.MoveX,
                                    characterKey = param[0], position = new Vector2(float.Parse(param[1]), 0),
                                    duration = _animateTime, shouldWait = i == parsedTag.Values.Count - 1
                                });
                            }
                            break;
                        case "fadein":
                            for (int i = 0; i < parsedTag.Values.Count; i++)
                            {
                                var param = parsedTag.Values[i].Split('/');
                                if (param.Length > 1)
                                    commandList.Add(SetCharacterSprite(param[0], param[1]));
                                commandList.Add(new StandardDialogueData.CharacterAnimation
                                {
                                    command = StandardDialogueData.CharacterCommand.FadeIn,
                                    characterKey = param[0], duration = _animateTime,
                                    shouldWait = i == parsedTag.Values.Count - 1
                                });
                            }
                            break;
                        case "fadeout":
                            for (int i = 0; i < parsedTag.Values.Count; i++)
                            {
                                commandList.Add(new StandardDialogueData.CharacterAnimation
                                {
                                    command = StandardDialogueData.CharacterCommand.FadeOut,
                                    characterKey = parsedTag.Values[i], duration = _animateTime,
                                    shouldWait = i == parsedTag.Values.Count - 1
                                });
                            }
                            break;
                        case "sprite":
                            foreach (var value in parsedTag.Values)
                            {
                                var param = value.Split('/');
                                commandList.Add(SetCharacterSprite(param[0], param[1]));
                            }
                            break;
                        // ============ SCENE and UI TRANSITION
                        case "scenefadein": case "fadein-scene": case "fadein_scene": case "fadein scene":
                            commandList.Add(new StandardDialogueData.Transition
                            {
                                command = StandardDialogueData.TransitionCommand.SceneFadeIn,
                                duration = _animateTime, shouldWait = true
                            });
                            break;
                        case "scenefadeout": case "fadeout-scene": case "fadeout_scene": case "fadeout scene":
                            commandList.Add(new StandardDialogueData.Transition
                            {
                                command = StandardDialogueData.TransitionCommand.SceneFadeOut,
                                duration = _animateTime, shouldWait = true
                            });
                            break;
                        case "show-ui": case "show_ui": case "show ui":
                            commandList.Add(new StandardDialogueData.Transition
                            {
                                command = StandardDialogueData.TransitionCommand.ShowUI
                            });
                            break;
                        case "hide-ui": case "hide_ui": case "hide ui":
                            commandList.Add(new StandardDialogueData.Transition
                            {
                                command = StandardDialogueData.TransitionCommand.HideUI
                            });
                            break;
                        // ============ BACKGROUND and FOREGROUND
                        case "background":
                            commandList.Add(new StandardDialogueData.Background
                            {
                                command = StandardDialogueData.BackgroundCommand.Set,
                                spriteKey = parsedTag.Values[0],
                                position = new Vector2(0.5f, 0.5f),
                                scale = 1
                            });
                            break;
                        case "show-background": case "show_background": case "show background":
                            commandList.Add(new StandardDialogueData.Background
                            {
                                command = StandardDialogueData.BackgroundCommand.Show
                            });
                            break;
                        case "hide-background": case "hide_background": case "hide background":
                            commandList.Add(new StandardDialogueData.Background
                            {
                                command = StandardDialogueData.BackgroundCommand.Hide
                            });
                            break;
                        case "fadein-background": case "fadein_background": case "fadein background":
                            commandList.Add(new StandardDialogueData.Background
                            {
                                command = StandardDialogueData.BackgroundCommand.FadeIn,
                                duration = _animateTime, shouldWait = true
                            });
                            break;
                        case "fadeout-background": case "fadeout_background": case "fadeout background":
                            commandList.Add(new StandardDialogueData.Background
                            {
                                command = StandardDialogueData.BackgroundCommand.FadeOut,
                                duration = _animateTime, shouldWait = true
                            });
                            break;
                        case "foreground":
                            commandList.Add(new StandardDialogueData.Foreground
                            {
                                command = StandardDialogueData.BackgroundCommand.Set,
                                spriteKey = parsedTag.Values[0],
                                position = new Vector2(0.5f, 0.5f),
                                scale = 1
                            });
                            break;
                        case "show-foreground": case "show_foreground": case "show foreground":
                            commandList.Add(new StandardDialogueData.Foreground
                            {
                                command = StandardDialogueData.BackgroundCommand.Show
                            });
                            break;
                        case "hide-foreground": case "hide_foreground": case "hide foreground":
                            commandList.Add(new StandardDialogueData.Foreground
                            {
                                command = StandardDialogueData.BackgroundCommand.Hide
                            });
                            break;
                        case "fadein-foreground": case "fadein_foreground": case "fadein foreground":
                            commandList.Add(new StandardDialogueData.Foreground
                            {
                                command = StandardDialogueData.BackgroundCommand.FadeIn,
                                duration = _animateTime, shouldWait = true
                            });
                            break;
                        case "fadeout-foreground": case "fadeout_foreground": case "fadeout foreground":
                            commandList.Add(new StandardDialogueData.Foreground
                            {
                                command = StandardDialogueData.BackgroundCommand.FadeOut,
                                duration = _animateTime, shouldWait = true
                            });
                            break;
                    }
                }
                
                // Process talker's sprite change after all other tags.
                if (!string.IsNullOrEmpty(talkerSprite))
                    commandList.Add(SetCharacterSprite(talker, talkerSprite));
                commandList.Add(dialogueData);
            }
            else if (story.currentChoices.Count > 0)
            {
                // Show selection if exists.
                var selection = StandardDialogueData.CreateSelection();
                foreach (var choice in story.currentChoices)
                    selection.select.Add(choice.index, choice.text);
                commandList.Add(selection);
            }

            return commandList;
        }
        
        #region Helper functions

        private StandardDialogueData.CharacterAnimation SetCharacterSprite(string character, string sprite) => new()
        {
            command = StandardDialogueData.CharacterCommand.SetSprite,
            characterKey = character, spriteKey = sprite
        };

        #endregion
    }
}
