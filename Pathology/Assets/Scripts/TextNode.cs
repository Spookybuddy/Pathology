using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TextNode
{
    public string text;
    public string[] responses;
    public TextNode[] branches;

    public TextNode Next(int index)
    {
        return branches[index];
    }
}