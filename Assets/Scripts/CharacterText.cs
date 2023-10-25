using System.Collections;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEditor.Experimental.GraphView;

public class CharacterText : MonoBehaviour
{
    private GameManager manager;
    public string folder;
    public string fileName;
    public Texture2D[] portraits;
    public TextMeshPro person;

    public int lineIndex;
    private string[] dialog;

    private bool inputable;
    private int inputOptions;
    private bool confirmable;
    private bool cancelable;
    private bool gathering;
    private bool eventFlag;
    private bool changePlay;
    private bool changeNPC;
    private bool printing;

    void Awake() { manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>(); }

    void Start()
    {
        //Read in the file & folder
        person.text = fileName;
        if (!folder.Equals("")) fileName = Application.streamingAssetsPath + "/" + folder + "/" + fileName + ".txt";
        else fileName = Application.streamingAssetsPath + "/" + fileName + ".txt";
        dialog = File.ReadAllLines(fileName);
    }

    //Player input button
    public void PlayerContinue(int EastNorthWest)
    {
        //Change portraits at start of new line
        if (changePlay) { manager.PortraitPlayer(GetInt(dialog[lineIndex])); changePlay = false; }
        if (changeNPC) { manager.PortraitNPC(GetInt(dialog[lineIndex])); changeNPC = false; }

        //Player continue
        if (confirmable) {
            confirmable = false;
            lineIndex++;
            PrintLine();
            return;
        }

        //Check if player has the specified items
        if (gathering) {
            if (manager.CheckInven(GetInt(dialog[lineIndex + 1]))) {
                gathering = false;
                lineIndex += 2;
                PrintLine();
                return;
            } else {
                cancelable = true;
                return;
            }
        }

        //Player leave
        if (cancelable && !inputable) {
            PlayerCancel();
            return;
        }

        //Player input buttons
        if (inputable && inputOptions >= EastNorthWest && EastNorthWest > 0) {
            inputable = false;
            lineIndex = GetInt(dialog[lineIndex + EastNorthWest]);
            PrintLine();
            return;
        }

        //Player leave
        if (cancelable) {
            PlayerCancel();
            return;
        }

        //Skip typing and display full text
        if (printing) {
            printing = false;
            manager.MouseClick(false);
            PrintAll();
            return;
        }

        //Mouse block
        if (EastNorthWest < 0) {
            return;
        }

        //If all else fails, just print the current line
        PrintLine();
    }

    //Available function to cancel conversation
    public void PlayerCancel()
    {
        cancelable = false;
        inputable = false;
        printing = false;
        gathering = false;
        confirmable = false;
        manager.MouseClick(true);
        manager.PlayerLeaves();
    }

    //Can continue conversation
    private void Continue()
    {
        confirmable = true;
        manager.MouseClick(true);
    }

    //Check who is talking and update portraits
    private int CheckTalk()
    {
        if (char.Equals(dialog[lineIndex][0], '!')) {
            manager.PortraitFront(true);
            return 1;
        } else {
            manager.PortraitFront(false);
            return 0;
        }
    }

    //Check for events
    private bool CheckEvent()
    {
        return manager.ReadBool(5, GetInt(dialog[lineIndex + 1]));
    }

    //Fully print out the line
    private void PrintAll()
    {
        if (CheckTalk() == 1) manager.setDisplay(dialog[lineIndex].Substring(1));
        else manager.setDisplay(dialog[lineIndex]);
        ReadNext();
        manager.ButtonDisplay(inputOptions, inputable, cancelable);
    }

    //Clear line and began printing
    public void PrintLine()
    {
        cancelable = false;
        //Check even & gather state before printing line
        if (lineIndex + 1 < dialog.Length) {
            switch(dialog[lineIndex + 1][0]) {
                case '%':
                    if (CheckEvent()) lineIndex += 2;
                    break;
                case '$':
                    break;
            }
        }
        manager.ButtonDisplay(inputOptions, false, cancelable);
        manager.setDisplay("");
        manager.MouseClick(true);
        printing = true;
        StartCoroutine(Typing(CheckTalk()));
    }

    //Check what the next line is
    private void ReadNext()
    {
        printing = false;
        if (lineIndex + 1 < dialog.Length) {
            switch (dialog[lineIndex + 1][0]) {
                //Player Responses
                case '>':
                    inputable = true;
                    manager.MouseClick(false);
                    inputOptions = 1;
                    if (lineIndex + 2 < dialog.Length) {
                        if (dialog[lineIndex + 2][0] == '>') inputOptions = 2;
                        else if (dialog[lineIndex + 2][0] == '@') cancelable = true;
                        else return;
                    }
                    if (lineIndex + 3 < dialog.Length) {
                        if (dialog[lineIndex + 3][0] == '>') inputOptions = 3;
                        else if (dialog[lineIndex + 3][0] == '@') cancelable = true;
                        else return;
                    }
                    if (lineIndex + 4 < dialog.Length) {
                        if (dialog[lineIndex + 4][0] == '@') cancelable = true;
                        else return;
                    }
                    break;
                //End Conversation Button available
                case '@':
                    cancelable = true;
                    manager.MouseClick(false);
                    break;
                //Ask player for a certain item
                case '$':
                    gathering = true;
                    Continue();
                    break;
                //Give player an item
                case '&':
                    manager.AddInven(GetInt(dialog[lineIndex + 1]), 1);
                    lineIndex++;
                    Continue();
                    break;
                //Check for event flag triggered
                case '%':
                    if (CheckEvent()) {
                        lineIndex++;
                        Continue();
                    } else {
                        cancelable = true;
                        manager.MouseClick(false);
                    }
                    break;
                //Set event flag
                case '=':
                    manager.WriteBool(5, GetInt(dialog[lineIndex + 1]), '1');
                    lineIndex++;
                    Continue();
                    break;
                //Player portrait state
                case '^':
                    changePlay = true;
                    lineIndex++;
                    Continue();
                    break;
                //NPC portrait state - Move to change once next line starts printing
                case '~':
                    changeNPC = true;
                    lineIndex++;
                    Continue();
                    break;
                //Conversation will continue otherwise
                default:
                    Continue();
                    break;
            }
        }
    }

    //Return the dialog line after any whitespaces
    public string DialogLine(int x)
    {
        for (int i = 0; i < dialog[lineIndex + x].Length; i++) {
            if (char.IsWhiteSpace(dialog[lineIndex + x][i])) {
                return dialog[lineIndex + x].Substring(i);
            }
        }
        return ":(";
    }

    //Transfer data to save
    public int WriteData() { return lineIndex; }
    public void ReadData(int index) { lineIndex = index; }
    public string NameData() { return person.text; }

    //Returns the number from the commands
    private int GetInt(string line)
    {
        string number = "";
        for (int i = 1; i < line.Length; i++) {
            if (char.IsDigit(line[i])) number += line[i];
            if (char.IsWhiteSpace(line[i])) break;
        }
        return int.Parse(number);
    }

    //Slowly print each letter of the line
    private IEnumerator Typing(int i)
    {
        yield return new WaitForSeconds(0.05f);
        if (printing) manager.addDisplay(dialog[lineIndex][i]);
        if (i + 1 < dialog[lineIndex].Length && printing) StartCoroutine(Typing(i + 1));
        else {
            ReadNext();
            manager.ButtonDisplay(inputOptions, inputable, cancelable);
        }
    }
}