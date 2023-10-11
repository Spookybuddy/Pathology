using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.CompilerServices;

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
    private Item item;

    //To be modified by the level design
    public int fileIndex;
    public int id;
    public int amount;

    //Read in from save file to determine if item has been picked up, as well as create a new item from the catalog
    void Start()
    {
        manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        location = Application.streamingAssetsPath + location;
        read = File.ReadAllLines(location)[4];
        spawn = read[fileIndex] == '0';
        if (!spawn) Destroy(gameObject);

        //Get the item from the catalog using the manager
        item = manager.ParseCatalog(id, amount);
    }

    public Item Pickup() { return item; }

    //Call function when picked up
    void OnDestroy()
    {
        if (spawn) manager.WriteBool(4, fileIndex, '1');
    }
}