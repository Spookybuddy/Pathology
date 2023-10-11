using UnityEngine;
using System.IO;

public class EventTrigger : MonoBehaviour
{
    private GameManager manager;
    private string location = "/SaveData.txt";
    private string read;
    private bool active;
    public int fileIndex;
    public bool Enter;
    public bool Erase;

    void Start()
    {
        manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        location = Application.streamingAssetsPath + location;
        read = File.ReadAllLines(location)[5];
        active = read[fileIndex] == '0';
        if (Enter) Erase = false;
    }

    //Trigger walked into
    void OnTriggerEnter() { if (Enter && active) manager.WriteBool(5, fileIndex, '1'); }

    //Trigger destroyed, usually on an item
    void OnDestroy() { if (Erase && active) manager.WriteBool(5, fileIndex, '1'); }
}