using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CharacterText : MonoBehaviour
{
    private GameManager manager;
    public string folder;
    public string fileName;
    private int lineIndex;
    private string[] dialog;

    void Awake()
    {
        manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
    }

    void Start()
    {
        if (folder != null) fileName = Application.streamingAssetsPath + "/" + folder + "/" + fileName + ".txt";
        else fileName = Application.streamingAssetsPath + "/" + fileName + ".txt";
        dialog = File.ReadAllLines(fileName);
    }

    public void PrintLine()
    {
        Debug.Log(dialog[lineIndex]);
        if (lineIndex + 1 < dialog.Length) {
            switch (dialog[lineIndex + 1][0]) {
                case '>':
                    Debug.Log("Player Input");
                    break;
                case '@':
                    Debug.Log("End Convo");
                    break;
                default:
                    Debug.Log("Uh Oh");
                    break;
            }
        }
    }
}