using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public int Id;
    public string Name;
    public char Category;
}

public class ItemScript : MonoBehaviour
{
    public Item item;
}