using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class GameManager : MonoBehaviour
{
    public Player player;
    public Camera innerCam;
    public CharacterText currentConvo;
    public CharacterText[] characterIDs;
    public CharacterText[] interiorChars;
    public Vector3[] cameraLocations;

    public Item nearbyItem;
    public List<Item> Inventory = new List<Item>();
    public TextMeshProUGUI inventoryListing;
    private int topItem;
    private int sortType;

    public TextMeshProUGUI textbox;
    public RawImage playerPortrait;
    public RawImage otherPortrait;
    private int Ndex;
    public GameObject[] buttonIndicators;
    public Button[] buttons;
    private readonly Vector4 shade = new Vector4(0.3f, 0.3f, 0.3f, 1);

    private string filename;
    private string[] savedData;
    private static Vector3 position;
    private static int location;

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

    //Add the nearby item to inventory
    public void AddInven()
    {
        Inventory.Add(nearbyItem);
        InventorySort(false);
    }

    //Check player's inventory for item ID X
    public bool CheckInven(int ID)
    {
        return Inventory.Exists(x => x.Id == ID);
    }

    //Set the portraits, and grey out who is not talking
    public void PortraitFront(bool player)
    {
        if (player) {
            playerPortrait.color = Color.white;
            otherPortrait.color = shade;
        } else {
            playerPortrait.color = shade;
            otherPortrait.color = Color.white;
        }
    }

    //Mouse scroll wheel / controller scroll
    public void Scroll(int dir) 
    {
        if (Inventory.Count > 0) {
            topItem = (topItem + dir + Inventory.Count) % Inventory.Count;
            InventoryText();
        }
    }

    //Update the text so that only 10 items are shown at a time
    private void InventoryText()
    {
        inventoryListing.text = "";
        for (int i = topItem; i < Mathf.Min(Inventory.Count, topItem + 10); i++) {
            inventoryListing.text += Inventory[i].Name + "\n";
        }
    }

    //Sort the inventory by the desired method: Name, Number, Category
    public void InventorySort(bool increment)
    {
        if (increment) sortType = (sortType + 1) % 6;
        switch (sortType) {
            case 0:
                Inventory.Sort((x, y) => x.Id.CompareTo(y.Id));
                break;
            case 1:
                Inventory.Reverse();
                break;
            case 2:
                Inventory.Sort((x, y) => x.Name.CompareTo(y.Name));
                break;
            case 3:
                Inventory.Reverse();
                break;
            case 4:
                Inventory.Sort((x, y) => x.Category.CompareTo(y.Category));
                break;
            default:
                Inventory.Reverse();
                break;
        }
        InventoryText();
    }

    //Change the display buttons depending on what controller is currently in use
    public void ControllerButtons(string controls)
    {
        //Xbox, Pro, Dual shock controllers
        //Debug.Log(controls);
        switch (controls[0]) {
            case 'P':
                //A X Y B
                break;
            case 'X':
                //B Y X A
                break;
            case 'D':
                //O ^ # X
                Debug.Log("Playstation");
                break;
            default:
                break;
        }
    }

    //Display text
    public void setDisplay(string txt) { textbox.text = txt; }
    public void addDisplay(char chr) { textbox.text += chr; }

    //Show/Hide buttons
    public void ButtonDisplay(int amount, bool input, bool cancel)
    {
        for (int i = 0; i < 4; i++) buttons[i].gameObject.SetActive(false);
        if (input) {
            for (int i = 0; i < amount; i++) {
                buttons[i].gameObject.SetActive(true);
                buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = currentConvo.DialogLine(i + 1);
            }
        }
        buttons[3].gameObject.SetActive(cancel);
        buttons[3].transform.localPosition = new Vector3(-450, -60 * amount - 160, 0);
    }

    //Player inputs are translated into whatever the current conversation is
    public void Advance(int EastNorthWest) { currentConvo.PlayerContinue(EastNorthWest); }
    public void Decline() { currentConvo.PlayerCancel(); }
    public void PlayerLeaves() { player.CloseDialog(); }
    public void ClickButton(int EastNorthWest) { currentConvo.PlayerContinue(EastNorthWest); }
    public void MouseClick(bool status) { player.ClickState(status); }
    public void ReIndex() { topItem = 0; }
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