using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using TMPro;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public Player player;
    public Interior inner;
    public Camera innerCam;
    public CharacterText currentConvo;
    public CharacterText[] characterIDs;
    public CharacterText[] interiorChars;
    public Vector3[] cameraLocations;

    //Inventory data
    public Item nearbyItem;
    public List<Item> Inventory = new List<Item>();
    public TextMeshProUGUI inventoryListing;
    public TextMeshProUGUI sortFormat;
    public GameObject pointer;
    public Slider slider;
    public RawImage[] icons;
    public Texture2D[] categories;
    private int deltaSlide;
    private int indexedItem;
    private int sortType;

    //Dialog data
    public TextMeshProUGUI textbox;
    public RawImage playerPortrait;
    public Texture2D[] playerEmotes;
    public RawImage otherPortrait;
    public GameObject[] buttonIndicators;
    public Button[] buttons;
    public GameObject[] nameplates;
    public TextMeshProUGUI npcName;
    private readonly Vector4 shade = new Vector4(0.3f, 0.3f, 0.3f, 1);

    //Saved data
    private string filename;
    private string itemCollection;
    private string[] savedData;
    private string[] catalog;
    private static Vector3 position;
    private static int location;
    private static int EXChars;
    private static int INChars;
    private static float volume;
    private static bool CMEnabled;
    private static int txtSpd;
    private bool loading;

    //Gets all the saved data at start of scene
    void Awake()
    {
        filename = Application.streamingAssetsPath + "/SaveData.txt";
        itemCollection = Application.streamingAssetsPath + "/Catalog.txt";
        savedData = File.ReadAllLines(filename);
        catalog = File.ReadAllLines(itemCollection);
        EXChars = savedData[0].Length / 2;
        INChars = savedData[1].Length / 2;
        volume = int.Parse(savedData[6].Substring(0, 3)) / 100f;
        CMEnabled = savedData[6].Equals('1');
        txtSpd = int.Parse(savedData[6].Substring(4));
    }
    void Start()
    {
        ReadPosition();
        ReadInven();
        if (player != null) {
            player.ClickMovement(CMEnabled);
            player.transform.position = position + Vector3.back;
            ReadData(characterIDs, 0);
        } else {
            innerCam.transform.position = cameraLocations[location];
            ReadData(interiorChars, 1);
            if (location == 0) {
                currentConvo = null;
                inner.GameMode(true);
            } else {
                currentConvo = interiorChars[(location - 1)];
                inner.GameMode(false);
                StartCoroutine(ConvoDelay());
            }
        }
        loading = false;
    }

    void OnApplicationQuit()
    {
        loading = true;
        WriteInven();
        WritePosition();
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

    //Reads the desired line, replaces the index with the char, then rewrites the line
    public void WriteBool(int line, int index, char state)
    {
        if (loading) return;
        string current = savedData[line];
        char[] replace = current.ToCharArray();
        replace[index] = state;
        savedData[line] = new string(replace);
        File.WriteAllLines(filename, savedData);
    }

    //Write all the fun little inventory data to the save file using char & triple digits for ID & amount
    public void WriteInven()
    {
        string line = "";
        for (int i = 0; i < Inventory.Count; i++) {
            string obj = "";
            obj += (char)(Inventory[i].Id % 95 + 32);
            obj += (Inventory[i].Quantity).ToString("000");
            line += obj;
        }
        savedData[3] = line;
        File.WriteAllLines(filename, savedData);
    }

    //Save the player world positions
    private void WritePosition()
    {
        if (player != null) position = player.transform.position;
        string digits = "";
        float rounded = Mathf.RoundToInt(position.x * 100) / 100f;
        digits += rounded + " ";
        rounded = Mathf.RoundToInt(position.z * 100) / 100f;
        digits += rounded + " ";
        savedData[2] = digits;
        File.WriteAllLines(filename, savedData);
    }

    //Read the data from the saved file and record it into character indicies
    public void ReadData(CharacterText[] array, int line)
    {
        for (int i = 0; i < array.Length; i++) {
            int x = ((int)savedData[line][2 * i] - 32) * 95 + ((int)savedData[line][2 * i + 1] - 32);
            x = Mathf.Max(x, 0);
            array[i].ReadData(x);
        }
    }

    //Returns the boolean value at line # & index #
    public bool ReadBool(int line, int index)
    {
        return ((savedData[line][index] == '0') ? false : true);
    }

    //Reads through the inventory save data and the item catalog to create new items with proper stats
    private void ReadInven()
    {
        Inventory.Clear();
        for (int i = 0; i < savedData[3].Length / 4; i++) {
            Inventory.Add(ParseCatalog((int)(savedData[3][4 * i] - 32), int.Parse(savedData[3].Substring(4 * i + 1, 3))));
        }
        InventorySort(false);
    }

    //Parse the item data from the item catalog
    public Item ParseCatalog(int ID, int amt)
    {
        Item add = new Item();
        add.Id = ID;
        add.Quantity = amt;
        string name = "";
        for (int j = 0; j < catalog[add.Id].Length; j++) {
            if (char.IsWhiteSpace(catalog[add.Id][j])) {
                add.Category = catalog[add.Id][j + 1];
                break;
            }
            else name += catalog[add.Id][j];
        }
        add.Name = name;
        return add;
    }

    //Returns the positions
    private void ReadPosition()
    {
        string number = "";
        float x = -9999;
        float z = 0;
        for (int i = 0; i < savedData[2].Length; i++) {
            if (char.IsDigit(savedData[2][i]) || char.IsPunctuation(savedData[2][i])) number += savedData[2][i];
            if (char.IsWhiteSpace(savedData[2][i])) {
                if (x != -9999) {
                    z = float.Parse(number);
                    break;
                }
                x = float.Parse(number);
                number = "";
            }
        }
        position = new Vector3(x, 0, z);
    }

    //Resets all character indexes to 00
    private void ClearData()
    {
        string clearline = "";
        for (int i = 0; i < EXChars; i++) clearline += "  ";
        savedData[0] = clearline + ";";
        clearline = "";
        for (int i = 0; i < INChars; i++) clearline += "  ";
        savedData[1] = clearline + ";";
        clearline = "";
        for (int i = 0; i < savedData[4].Length; i++) clearline += "0";
        savedData[4] = clearline;
        clearline = "";
        for (int i = 0; i < savedData[5].Length; i++) clearline += "0";
        savedData[5] = clearline;
        File.WriteAllLines(filename, savedData);
    }

    //Pass settings data
    public float Sound() { return volume; }
    public float TextSpeed() { return txtSpd / -31f + 0.1f; }

    //Add the nearby item to inventory
    public void AddInven()
    {
        //Check if polayer already has that item ID in their inventory. If they do, add to its quantity. If not add to list
        if (Inventory.Exists(x => x.Id == nearbyItem.Id)) Inventory.Find(x => x.Id == nearbyItem.Id).Quantity = Mathf.Clamp(Inventory.Find(x => x.Id == nearbyItem.Id).Quantity + nearbyItem.Quantity, 1, 999);
        else Inventory.Add(nearbyItem);
        InventorySort(false);
    }

    //Add a new item of id & amount
    public void AddInven(int item, int amt)
    {
        if (Inventory.Exists(x => x.Id == item)) Inventory.Find(x => x.Id == item).Quantity += amt;
        else Inventory.Add(ParseCatalog(item, amt));
        InventorySort(false);
    }

    //Tries to remove the amount of item, and returns if it was successful or not
    public bool RemoveInven(int item, int amt)
    {
        if (Inventory.Exists(x => x.Id == item)) {
            Item remove = Inventory.Find(x => x.Id == item);
            if (remove.Quantity > amt) remove.Quantity -= amt;
            else if (remove.Quantity == amt) Inventory.Remove(remove);
            else return false;
        } else return false;
        InventorySort(false);
        return true;
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
            nameplates[0].SetActive(false);
            nameplates[1].SetActive(true);
        } else {
            playerPortrait.color = shade;
            otherPortrait.color = Color.white;
            nameplates[0].SetActive(true);
            nameplates[1].SetActive(false);
            npcName.text = currentConvo.NameData();
        }
    }

    //Change the portaits for player
    public void PortraitPlayer(int state)
    {
        playerPortrait.texture = playerEmotes[state];
    }

    //Change the portraits for npc
    public void PortraitNPC(int state)
    {
        otherPortrait.texture = currentConvo.portraits[state];
    }

    //Mouse scroll wheel / controller scroll
    public void Scroll(int dir) 
    {
        if (Inventory.Count > 0) {
            indexedItem = (indexedItem + dir + Inventory.Count) % Inventory.Count;
            int limit = Mathf.Min(Mathf.Max(Inventory.Count - 10, 0), indexedItem);
            if (indexedItem > limit) pointer.transform.localPosition = new Vector3(pointer.transform.localPosition.x, 175 - (indexedItem - limit) * 45, 0);
            else pointer.transform.localPosition = new Vector3(pointer.transform.localPosition.x, 175, 0);
            InventoryText();
        }
    }

    //Inventory slider
    public void Scrollbar()
    {
        Scroll((int)slider.value - deltaSlide);
        deltaSlide = (int)slider.value;
    }

    //Update the text so that only 10 items are shown at a time
    private void InventoryText()
    {
        for (int i = 0; i < icons.Length; i++) icons[i].gameObject.SetActive(false);
        inventoryListing.text = "";
        for (int i = Mathf.Min(Mathf.Max(Inventory.Count - 10, 0), indexedItem); i < Mathf.Min(Inventory.Count, indexedItem + 10); i++) {
            inventoryListing.text += "x" + Inventory[i].Quantity + " " + Inventory[i].Name + "\n";
            icons[i].texture = categories[(int)(Inventory[i].Category - 32)];
            icons[i].gameObject.SetActive(true);
        }
        slider.maxValue = Mathf.Max(Inventory.Count - 1, 0);
    }

    //Sort the inventory by the desired method: Name, Number, Category
    public void InventorySort(bool increment)
    {
        if (increment) sortType = (sortType + 1) % 8;
        switch (sortType) {
            case 0:
                Inventory.Sort((x, y) => x.Id.CompareTo(y.Id));
                sortFormat.text = "Numerical";
                break;
            case 1:
                Inventory.Reverse();
                sortFormat.text = "Reverse Numerical";
                break;
            case 2:
                Inventory.Sort((x, y) => x.Name.CompareTo(y.Name));
                sortFormat.text = "A - Z";
                break;
            case 3:
                Inventory.Reverse();
                sortFormat.text = "Z - A";
                break;
            case 4:
                Inventory.Sort((x, y) => y.Quantity.CompareTo(x.Quantity));
                sortFormat.text = "Most";
                break;
            case 5:
                Inventory.Reverse();
                sortFormat.text = "Least";
                break;
            case 6:
                Inventory.Sort((x, y) => x.Category.CompareTo(y.Category));
                sortFormat.text = "Categories";
                break;
            default:
                Inventory.Reverse();
                sortFormat.text = "Reverse Categories";
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
    public void PlayerLeaves() { if (player != null) player.CloseDialog(); else inner.Exit(); }
    public void ClickButton(int EastNorthWest) { currentConvo.PlayerContinue(EastNorthWest); }
    public void MouseClick(bool status) { if (player != null) player.ClickState(status); else inner.ClickState(status); }
    public void Locate(int loc) { location = loc; }

    //Record position for both scenes & load desired scene
    public void Scene(string scene)
    {
        loading = true;
        if (player != null) {
            WritePosition();
            WriteData(characterIDs, 0);
            WriteInven();
        } else {
            WriteData(interiorChars, 1);
        }
        StartCoroutine(Load(scene));
    }

    private IEnumerator Load(string scene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);
        while (!asyncLoad.isDone) yield return null;
    }

    private IEnumerator ConvoDelay()
    {
        yield return new WaitForSeconds(0.02f);
        currentConvo.PrintLine();
    }
}