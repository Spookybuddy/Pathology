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
    private bool firstEncounter;
    private bool inputable;
    private int inputOptions;
    private bool confirmable;
    private bool cancelable;

    void Awake()
    {
        manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();

        //Ensure that box collider & rigidbody are adjusted properly for detecting player
    }

    void Start()
    {
        person.text = fileName;
        if (!folder.Equals("")) fileName = Application.streamingAssetsPath + "/" + folder + "/" + fileName + ".txt";
        else fileName = Application.streamingAssetsPath + "/" + fileName + ".txt";
        dialog = File.ReadAllLines(fileName);
        firstEncounter = true;
    }

    //Player hit EAST
    public void PlayerContinue(int EastNorthWest)
    {
        if (cancelable) {
            PlayerCancel();
            return;
        }

        if (confirmable) {
            confirmable = false;
            lineIndex++;
            PrintLine();
            return;
        }

        if (inputable && inputOptions >= EastNorthWest) {
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

        //First time talking, so none of the booleans have values
        if (firstEncounter) {
            firstEncounter = false;
            PrintLine();
            return;
        }

        //If all else fails, just print the current line
        PrintLine();
    }

    //Player hit SOUTH
    public void PlayerCancel()
    {
        if (cancelable) {
            cancelable = false;
            manager.PlayerLeaves();
        }
    }

    public void PrintLine()
    {
        Debug.Log(dialog[lineIndex]);
        if (lineIndex + 1 < dialog.Length) {
            switch (dialog[lineIndex + 1][0]) {
                //Player Responses
                case '>':
                    inputable = true;
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
                        if (dialog[lineIndex + 4][0] == '>') inputOptions = 4;
                        else if (dialog[lineIndex + 4][0] == '@') cancelable = true;
                        else return;
                    }
                    break;
                //End Conversation Button available
                case '@':
                    cancelable = true;
                    break;
                //Conversation will continue otherwise
                default:
                    confirmable = true;
                    break;
            }
        }
    }
}