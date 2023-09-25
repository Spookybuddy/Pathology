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
    public TextMeshProUGUI display;
    public GameObject[] buttons;
    public TextMeshProUGUI[] responses;

    private int lineIndex;
    private string[] dialog;

    private bool firstEncounter;
    private bool inputable;
    private int inputOptions;
    private bool confirmable;
    private bool cancelable;
    private bool printing;

    void Awake()
    {
        manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
    }

    void Start()
    {
        //Read in the file & folder
        person.text = fileName;
        if (!folder.Equals("")) fileName = Application.streamingAssetsPath + "/" + folder + "/" + fileName + ".txt";
        else fileName = Application.streamingAssetsPath + "/" + fileName + ".txt";
        dialog = File.ReadAllLines(fileName);
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

        //Skip typing and display full text
        if (printing) {
            printing = false;
            PrintAll();
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

    //Fully print out the line
    private void PrintAll()
    {
        display.text = dialog[lineIndex];
        ReadNext();
        ButtonDisplay();
    }

    //Clear line and began printing
    public void PrintLine()
    {
        ButtonDisplay();
        display.text = "";
        printing = true;
        StartCoroutine(Typing(0));
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

    //Show the buttons with responses
    private void ButtonDisplay()
    {
        for (int i = 0; i < buttons.Length; i++) buttons[i].SetActive(false);

        if (inputable) {
            //Display proper number of buttons for the responses
            for (int i = 0; i < inputOptions; i++) {
                //Remove the indent & line id
                for (int a = 1; a < dialog[lineIndex + i + 1].Length; a++) {
                    if (char.IsWhiteSpace(dialog[lineIndex + i + 1][a])) {
                        responses[i].text = dialog[lineIndex + i + 1].Remove(0, a);
                        break;
                    }
                }
                buttons[i].SetActive(true);
            }
        }

        //Display the leave button if the option is available
        if (cancelable) buttons[3].SetActive(true);
    }

    //Slowly print each letter of the line
    private IEnumerator Typing(int i)
    {
        yield return new WaitForSeconds(0.05f);
        if (printing) display.text += dialog[lineIndex][i];
        if (i + 1 < dialog[lineIndex].Length && printing) StartCoroutine(Typing(i + 1));
        else {
            ReadNext();
            ButtonDisplay();
        }
    }
}