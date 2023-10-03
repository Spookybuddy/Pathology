using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class CharacterText : MonoBehaviour
{
    private GameManager manager;
    public string folder;
    public string fileName;

    public TextMeshPro person;

    private int lineIndex;
    private string[] dialog;

    private bool inputable;
    private int inputOptions;
    private bool confirmable;
    private bool cancelable;
    private bool gathering;
    private bool eventFlag;
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
        //Player continue
        if (confirmable) {
            confirmable = false;
            lineIndex++;
            PrintLine();
            return;
        }
        
        //Check if player has the specified items
        if (gathering) {
            string itemID = "";
            for (int i = 0; i < dialog[lineIndex + 1].Length; i++) {
                if (char.IsDigit(dialog[lineIndex + 1][i])) itemID += dialog[lineIndex + 1][i];
                if (char.IsWhiteSpace(dialog[lineIndex + 1][i])) break;
            }
            if (manager.CheckInven(int.Parse(itemID))) {
                gathering = false;
                lineIndex++;
                PrintLine();
                return;
            }
        }
        
        //Player input buttons
        if (inputable && inputOptions >= EastNorthWest && EastNorthWest > 0) {
            inputable = false;
            //Parse the line value to jump to
            string toLine = "";
            for (int i = 0; i < dialog[lineIndex + EastNorthWest].Length; i++) {
                if (char.IsDigit(dialog[lineIndex + EastNorthWest][i])) toLine += dialog[lineIndex + EastNorthWest][i];
                if (char.IsWhiteSpace(dialog[lineIndex + EastNorthWest][i])) break;
            }
            lineIndex = int.Parse(toLine);
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
                    break;
                //Check for event flag triggered
                case '=':
                    //eventFlag = true;
                    break;
                //Conversation will continue otherwise
                default:
                    confirmable = true;
                    manager.MouseClick(true);
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