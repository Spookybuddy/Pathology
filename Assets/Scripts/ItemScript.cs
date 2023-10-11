using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class Item
{
    public int Id;
    public string Name;
    public char Category;
    public int Quantity;
}

public class ItemScript : MonoBehaviour
{
    private GameManager manager;
    private string location = "/SaveData.txt";
    private string read;
    private bool spawn;
    public int fileIndex;
    public Item item;

    //Read in from save file to determine if item has been picked up
    void Start()
    {
        manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        location = Application.streamingAssetsPath + location;
        read = File.ReadAllLines(location)[4];
        spawn = read[fileIndex] == '0';
        if (!spawn) Destroy(gameObject);
    }

    //Call function when picked up
    void OnDestroy()
    {
        if (spawn) manager.WriteBool(4, fileIndex, '1');
    }
}