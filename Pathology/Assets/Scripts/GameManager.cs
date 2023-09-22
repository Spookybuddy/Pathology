using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CharacterText currentConvo;

    public void Advance()
    {
        currentConvo.PrintLine();
    }
}