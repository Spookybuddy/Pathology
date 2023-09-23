using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player;
    public CharacterText currentConvo;

    //Player inputs are translated into whatever the current conversation is
    public void Advance(int EastNorthWest)
    {
        currentConvo.PlayerContinue(EastNorthWest);
    }

    public void Decline()
    {
        currentConvo.PlayerCancel();
    }

    public void PlayerLeaves()
    {
        player.CloseDialog();
    }

    public void PlayerResponses(int amount)
    {

    }
}