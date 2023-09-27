using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    public Player player;
    public Camera innerCam;
    public CharacterText currentConvo;
    public CharacterText[] characterIDs;
    public CharacterText[] interiorChars;
    public Vector3[] cameraLocations;
    private string filename;
    private string[] savedData;
    public static Vector3 position;
    public static int location;

    void Start()
    {
        filename = Application.streamingAssetsPath + "/SaveData.txt";
        savedData = File.ReadAllLines(filename);
        if (player != null) {
            player.transform.position = position + Vector3.back;
            ReadData(characterIDs, 0);
        } else {
            innerCam.transform.position = cameraLocations[location];
            //ReadData(interiorChars, 1);
        }
    }

    void OnApplicationQuit()
    {
        ClearData();
    }
    
    //Convert all character indicies into chars
    public void WriteData(CharacterText[] array, int line)
    {
        string newLine = "";
        for (int i = 0; i < array.Length; i++) {
            int x = array[i].WriteData();
            char A = (char)(x / 95 + 32);
            char B = (char)(x % 95 + 32);
            newLine += (A + "" + B);
        }
        savedData[line] = newLine;
        File.WriteAllLines(filename, savedData);
    }

    //Read the data from the saved file and record it into character indicies
    public void ReadData(CharacterText[] array, int line)
    {
        for (int i = 0; i < array.Length; i++) {
            int x = ((int)savedData[line][2 * i] - 32) * 95 + ((int)savedData[line][2 * i + 1] - 32);
            array[i].ReadData(x);
        }
    }

    //Resets all character indexes to 00
    private void ClearData()
    {
        string clearline = "";
        for (int i = 0; i < characterIDs.Length; i++) clearline += "  ";
        savedData[0] = clearline;
        /*
        clearline = "";
        for (int i = 0; i < interiorChars.Length; i++) clearline += "  ";
        savedData[1] = clearline;
        */
        File.WriteAllLines(filename, savedData);
    }

    //Check player's inventory for item ID X
    public bool CheckInven(int ID)
    {
        return false;
    }

    //Player inputs are translated into whatever the current conversation is
    public void Advance(int EastNorthWest) { currentConvo.PlayerContinue(EastNorthWest); }
    public void Decline() { currentConvo.PlayerCancel(); }
    public void PlayerLeaves() { player.CloseDialog(); }

    public void Locate(int loc) { location = loc; }

    //Record position for both scenes & load desired scene
    public void Scene(string scene)
    {
        if (player != null) {
            position = player.transform.position;
            WriteData(characterIDs, 0);
        } else {
            //WriteData(interiorChars, 1);
        }
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