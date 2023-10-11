using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class EventTrigger : MonoBehaviour
{
    private GameManager manager;
    private string location = "/SaveData.txt";
    private string read;
    public int fileIndex;
    public bool Enter;
    public bool Erase;

    void Start()
    {
        manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        location = Application.streamingAssetsPath + location;
        read = File.ReadAllLines(location)[5];
        if (Enter) Erase = false;
    }

    //Trigger walked into
    void OnTriggerEnter() { if (Enter) manager.WriteBool(5, fileIndex, '1'); }

    //Trigger destroyed, usually on an item
    void OnDestroy() { if (Erase) manager.WriteBool(5, fileIndex, '1'); }
}