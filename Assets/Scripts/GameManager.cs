using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    public Player player;
    public CharacterText currentConvo;
    public CharacterText[] characterIDs;
    private string filename;
    private string[] savedData;
    public static Vector3 position;
    public static Vector3 location;

    void Start()
    {
        if (player != null) player.transform.position = position + Vector3.back;
        filename = Application.streamingAssetsPath + "/SaveData.txt";
        savedData = File.ReadAllLines(filename);
        ReadData();
    }

    void OnApplicationQuit()
    {
        ClearData();
    }
    
    //Convert all character indicies into chars
    public void WriteData()
    {
        string newLine = "";
        for (int i = 0; i < characterIDs.Length; i++) {
            int x = characterIDs[i].WriteData();
            char A = (char)(x / 95 + 32);
            char B = (char)(x % 95 + 32);
            newLine += (A + "" + B);
        }
        savedData[0] = newLine;
        File.WriteAllLines(filename, savedData);
    }

    //Read the data from the saved file and record it into character indicies
    public void ReadData()
    {
        for (int i = 0; i < characterIDs.Length; i++) {
            int x = ((int)savedData[0][2 * i] - 32) * 95 + ((int)savedData[0][2 * i + 1] - 32);
            characterIDs[i].ReadData(x);
        }
    }

    //Resets all character indexes to 00
    private void ClearData()
    {
        string clearline = "";
        for (int i = 0; i < characterIDs.Length; i++) clearline += "  ";
        savedData[0] = clearline;
        File.WriteAllLines(filename, savedData);
    }

    //Player inputs are translated into whatever the current conversation is
    public void Advance(int EastNorthWest) { currentConvo.PlayerContinue(EastNorthWest); }
    public void Decline() { currentConvo.PlayerCancel(); }
    public void PlayerLeaves() { player.CloseDialog(); }

    //Record position for both scenes & load desired scene
    public void Scene(string scene)
    {
        if (player != null) position = player.transform.position;
        if (characterIDs.Length > 0) WriteData();
        StartCoroutine(Load(scene));
    }

    private IEnumerator Load(string scene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);
        while (!asyncLoad.isDone) {
            yield return null;
        }
    }
}