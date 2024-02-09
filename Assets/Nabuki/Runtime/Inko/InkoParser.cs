using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

namespace Nabuki.Inko
{
    public class InkoParser
    {
        private DialogueManager _manager;
        private List<string> _existingCharacters;
        
        public InkoParser(DialogueManager manager)
        {
            _manager = manager;
            _existingCharacters = new List<string>();
        }
        
        public List<IDialogueData> ParseNextLine(ref Story story)
        {
            var commandList = new List<IDialogueData>();
            var targetCharacter = string.Empty;
            
            // First we check whether story can be continued or not.
            if (story.canContinue)
            {
                // Get main text by continuing.
                var nextLine = story.Continue();
                
                // TODO: Extra process for main line
                
                // Next, parse tags for next line.
                foreach (var tag in story.currentTags)
                {
                    var parsedTag = tag.Split(':');
                    var tagKey = parsedTag[0].Trim().ToLower();
                    var tagValue = parsedTag.Length > 1 ? parsedTag[1].Trim() : string.Empty;

                    switch (tagKey) // Forced low capitalization.
                    {
                        case "speaker":
                        case "talker":
                            // # Speaker: John
                            targetCharacter = tagValue;

                            if (!_existingCharacters.Contains(targetCharacter))
                            {
                                // Create character
                                commandList.Add(new StandardDialogueData.Character
                                {
                                    command = StandardDialogueData.CharacterCommand.Add,
                                    characterKey = targetCharacter,
                                    characterName = targetCharacter
                                });
                                _existingCharacters.Add(targetCharacter);
                            }
                            break;
                        case "portrait":
                        case "sprite":
                            if (targetCharacter == "player")
                                break;
                            
                            // # Portrait: Angry
                            commandList.Add(new StandardDialogueData.CharacterAnimation
                            {
                                command = StandardDialogueData.CharacterCommand.SetSprite,
                                characterKey = targetCharacter,
                                spriteKey = tagValue
                            });
                            break;
                        case "position":
                        case "pos":
                            if (targetCharacter == "player")
                                break;
                            
                            // # Position: 0.3
                            commandList.Add(new StandardDialogueData.CharacterAnimation
                            {
                                command = StandardDialogueData.CharacterCommand.MoveX,
                                characterKey = targetCharacter,
                                position = new Vector2(float.Parse(tagValue), 0)
                            });
                            break;
                        default:
                            // Unknown tag
                            break;
                    }
                }
                
                commandList.Add(StandardDialogueData.CreateDialogue(targetCharacter, nextLine, string.Empty));
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
    }
}
