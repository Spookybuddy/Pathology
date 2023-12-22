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
    public Interior inner;
    public Camera innerCam;
    public AudioSource soundtrack;
    public AudioClip[] doorSFX;
    public AudioSource doors;
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
    public GameObject[] areas;
    public TextMeshProUGUI description;
    public GameObject popup;
    public Texture2D[] categories;
    private int deltaSlide;
    private int indexedItem;
    private int sortType;

    //Dialog data
    public AudioSource type;
    public TextMeshProUGUI textbox;
    public Image playerPortrait;
    public Sprite[] playerEmotes;
    public RawImage otherPortrait;
    public GameObject[] buttonIndicators;
    public GameObject continuing;
    private int controllerType;
    public Button[] buttons;
    public GameObject[] nameplates;
    public TextMeshProUGUI npcName;
    public GameObject offset;
    private readonly Vector4 shade = new Vector4(0.3f, 0.3f, 0.3f, 1);

    //Shader
    public Material shader;
    public float loadTimes;
    private float shaderValue;
    private float transition;

    //Saved data
    private string filename;
    private string itemCollection;
    private string[] savedData;
    private string[] catalog;
    private static Vector3 position;
    private static int location;
    private static int volume;
    private static bool CMEnabled;
    private static int txtSpd;
    private static int minimap;
    private bool loading;

    //Gets all the saved data at start of scene
    void Awake()
    {
        //Start with shader full
        transition = loadTimes - 0.001f;
        shader.SetFloat("_Scale", 500);
        StartCoroutine(Shade(false));

        //Read file data
        filename = Application.streamingAssetsPath + "/SaveData.txt";
        itemCollection = Application.streamingAssetsPath + "/Catalog.txt";
        savedData = File.ReadAllLines(filename);
        catalog = File.ReadAllLines(itemCollection);
        volume = int.Parse(savedData[6].Substring(0, 3));
        CMEnabled = (savedData[6].Substring(3, 1)).Equals("1");
        txtSpd = int.Parse(savedData[6].Substring(4, 1));
        minimap = int.Parse(savedData[6].Substring(5));
    }

    void Start()
    {
        //Read data depending on which scene is loaded
        ReadPosition();
        ReadInven();
        if (player != null) {
            player.ClickMovement(CMEnabled);
            player.MinimapSetting(minimap);
            player.Transitioning(true);

            //Move the player down to exit doors, but raycast first to prevent clipping
            if (Physics.Raycast(position, Vector3.forward, 2)) {
                player.transform.position = position + new Vector3(0, 0, -1.5f);
                doors.PlayOneShot(doorSFX[2], 1);
            }
            else player.transform.position = position;

            //Player out of bounds safety check
            if (player.transform.position.x < -10 || player.transform.position.x > 66 || player.transform.position.z < -6 || player.transform.position.z > 71) {
                position = Vector3.zero;
                player.transform.position = position;
            }
            ReadData(characterIDs, 0);
        } else {
            innerCam.transform.position = cameraLocations[location];
            ReadData(interiorChars, 1);
            if (location == 0) {
                currentConvo = null;
                inner.GameMode(true);
            } else {
                currentConvo = interiorChars[(location - 1)];
                PortraitNPC(0);
                inner.GameMode(false);
                StartCoroutine(ConvoDelay());
            }
        }
        loading = false;
    }

    //Save and quit
    void OnApplicationQuit()
    {
        loading = true;
        WriteInven();
        WritePosition();
        WriteSettings();
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

    //Write the player settings to file
    public void WriteSettings()
    {
        string update = "";
        update += volume.ToString("000");
        update += CMEnabled ? "1" : "0";
        update += txtSpd.ToString("0");
        update += minimap.ToString("0");
        savedData[6] = update;
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
        for (int j = 0; j < catalog[ID].Length; j++) {
            if (char.IsWhiteSpace(catalog[ID][j])) {
                add.Category = catalog[ID][j + 1];
                int x = 0;
                string stat = "";
                for (int i = j + 3; i < catalog[ID].Length; i++) {
                    if (char.IsWhiteSpace(catalog[ID][i]) && x < 3) {
                        switch (x) {
                            case 0:
                                //Error catch, returns 0 values and empty item
                                if (int.TryParse(stat, out int val)) {
                                    add.Vitamin = val;
                                    stat = "";
                                } else {
                                    add.Vitamin = 777;
                                    add.Mineral = 777;
                                    add.Enzymes = 777;
                                    add.Description = "Something went wrong";
                                    add.Name = name;
                                    return add;
                                }
                                break;
                            case 1:
                                add.Mineral = int.Parse(stat);
                                stat = "";
                                break;
                            case 2:
                                add.Enzymes = int.Parse(stat);
                                stat = "";
                                break;
                            default:
                                break;
                        }
                        x++;
                    } else stat += catalog[ID][i];
                }
                add.Description = stat;
                break;
            } else name += catalog[ID][j];
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

    //Pass settings data
    public float Sound() { return (volume / 100.0f); }
    public float TextSpeed() { return (txtSpd / -31f + 0.1f); }
    public void SetMinimap(int scale) { minimap = scale; WriteSettings(); }

    //Add the nearby item to inventory
    public void AddInven()
    {
        //Check if polayer already has that item ID in their inventory. If they do, add to its quantity. If not add to list
        if (Inventory.Exists(x => x.Id == nearbyItem.Id)) Inventory.Find(x => x.Id == nearbyItem.Id).Quantity = Mathf.Clamp(Inventory.Find(x => x.Id == nearbyItem.Id).Quantity + nearbyItem.Quantity, 1, 999);
        else Inventory.Add(nearbyItem);

        //Indicator to tell player what they picked up
        if (player != null) {
            GameObject p = Instantiate(popup, player.transform.position, Quaternion.identity);
            p.GetComponent<Popup>().Create(nearbyItem.Quantity, nearbyItem.Name);
        }
        InventorySort(false);
    }

    //Add specific item
    public void AddInven(Item i)
    {
        nearbyItem = i;
        AddInven();
    }

    //Add a new item of id & amount
    public void AddInven(int item, int amt)
    {
        if (Inventory.Exists(x => x.Id == item)) Inventory.Find(x => x.Id == item).Quantity += amt;
        else Inventory.Add(ParseCatalog(item, amt));
        InventorySort(false);
    }

    //Removes Item at the manager index and returns the item data
    public Item RemoveInven()
    {
        Item temp = new Item();
        temp.Copy(Inventory[indexedItem]);
        temp.Quantity = 1;
        Inventory[indexedItem].Quantity--;
        if (Inventory[indexedItem].Quantity < 1) Inventory.Remove(Inventory[indexedItem]);
        InventorySort(false);
        return temp;
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
        playerPortrait.sprite = playerEmotes[state];
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

    //Set inventory index via click
    public void SetIndex(int i)
    {
        indexedItem = (indexedItem + i + Inventory.Count) % Inventory.Count;
        int limit = Mathf.Min(Mathf.Max(Inventory.Count - 10, 0), indexedItem);
        if (indexedItem > limit) pointer.transform.localPosition = new Vector3(pointer.transform.localPosition.x, 175 - (indexedItem - limit) * 45, 0);
        else pointer.transform.localPosition = new Vector3(pointer.transform.localPosition.x, 175, 0);
        InventoryText();
    }

    //Inventory button to give a small description of the item from the item catalog
    public void ClickInven(int button)
    {
        int limit = Mathf.Min(Mathf.Max(Inventory.Count - 10, 0), indexedItem);
        description.text = Inventory[limit + button].Description;
        Debug.Log(Inventory[limit + button].Description);
    }

    //Inventory contoller button
    public void ClickInven() { ClickInven(indexedItem - Mathf.Min(Mathf.Max(Inventory.Count - 10, 0), indexedItem)); }

    public void ClearDescrip() { description.text = " "; }

    //Returns Indexed item
    public int GetIndex() { return indexedItem; }

    public int limitation() { return Mathf.Min(Inventory.Count, 10); }

    //Inventory slider
    public void Scrollbar()
    {
        Scroll((int)slider.value - deltaSlide);
        deltaSlide = (int)slider.value;
    }

    //Update the text so that only 10 items are shown at a time
    private void InventoryText()
    {
        for (int i = 0; i < icons.Length; i++) {
            icons[i].gameObject.SetActive(false);
            if (player != null) areas[i].SetActive(false);
        }
        inventoryListing.text = "";
        for (int i = Mathf.Min(Mathf.Max(Inventory.Count - 10, 0), indexedItem); i < Mathf.Min(Inventory.Count, indexedItem + 10); i++) {
            inventoryListing.text += "x" + Inventory[i].Quantity + " " + Inventory[i].Name + "\n";
            icons[i].texture = categories[(int)(Inventory[i].Category - 32)];
            icons[i].gameObject.SetActive(true);
            if (player != null) areas[i].SetActive(true);
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
        switch (controls[0]) {
            case 'P':
                ChangeControls(0); 
                return;
            case 'X':
                ChangeControls(1);
                return;
            case 'D':
                ChangeControls(2);
                return;
            default:
                controllerType = 3;
                for (int i = 0; i < buttonIndicators.Length; i++) buttonIndicators[i].SetActive(false);
                return;
        }
    }

    //Update button display if the control layout changes
    private void ChangeControls(int type)
    {
        if (controllerType != type) {
            for (int i = 0; i < buttonIndicators.Length; i++) buttonIndicators[i].SetActive(false);
            for (int i = 0; i < 4; i++) buttonIndicators[i + 4 * type].SetActive(true);
            controllerType = type;
        }
    }

    //Display text
    public void setDisplay(string txt) { textbox.text = txt; }
    public void addDisplay(char chr)
    {
        int ascii = Mathf.Max((int)chr - 64, 0) % 32;
        if (ascii > 0) TextSounds(ascii);
        textbox.text += chr;
    }

    private void TextSounds(int index)
    {
        type.pitch = index / 26f + 0.5f;
        type.PlayOneShot(type.clip, 1);
    }

    //Play the right door sounds
    public void DoorSound(int index)
    {
        if (index == 0) doors.PlayOneShot(doorSFX[0], 1);
        else doors.PlayOneShot(doorSFX[1], 1);
    }

    //Show/Hide buttons
    public void ButtonDisplay(int amount, bool input, bool cancel)
    {
        if (!input && !cancel) Buttons(amount, input, cancel);
        StartCoroutine(ButtonDelay(amount, input, cancel));
    }

    //Show the correct amount of buttons, offsetting them if there is room
    private void Buttons(int amount, bool input, bool cancel)
    {
        offset.transform.localPosition = Vector3.zero;
        if ((amount < 3 && !cancel) || (amount < 2 && cancel)) {
            if (textbox.text.Length > 50) {
                offset.transform.localPosition = new Vector3(0, -55, 0);
            }
        }
        for (int i = 0; i < 4; i++) buttons[i].gameObject.SetActive(false);
        if (input) {
            for (int i = 0; i < amount; i++) {
                buttons[i].gameObject.SetActive(true);
                buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = currentConvo.DialogLine(i + 1);
                if (controllerType < 3) buttonIndicators[4 * controllerType + i].SetActive(true);
            }
        }
        buttons[3].gameObject.SetActive(cancel);
        buttons[3].transform.localPosition = new Vector3(0, -55 * amount - 160, 0);
        if (controllerType < 3) buttonIndicators[4 * controllerType + 3].SetActive(cancel);
    }

    public void ContinueArrow(bool onOff) { continuing.SetActive(onOff); }

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
        if (transition != 0) return;
        loading = true;
        if (player != null) {
            WritePosition();
            WriteData(characterIDs, 0);
            WriteInven();
        } else {
            WriteData(interiorChars, 1);
            WriteInven();
        }
        StartCoroutine(Load(scene));
    }

    //Load scene until the transition is finished
    private IEnumerator Load(string scene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);
        asyncLoad.allowSceneActivation = false;
        transition = 0.001f;
        StartCoroutine(Shade(true));
        while (!asyncLoad.isDone) {
            if (asyncLoad.progress >= 0.9f && transition >= loadTimes) asyncLoad.allowSceneActivation = true;
            yield return null;
        }
    }

    //Button delay appearance
    private IEnumerator ButtonDelay(int amount, bool input, bool cancel)
    {
        yield return new WaitForSeconds(0.25f);
        Buttons(amount, input, cancel);
    }

    //Pass the conversation update
    private IEnumerator ConvoDelay()
    {
        yield return new WaitForSeconds(loadTimes);
        currentConvo.PrintLine();
    }

    //Fade out description text
    private IEnumerator TextFade()
    {
        yield return new WaitForSeconds(4);
        description.text = " ";
    }

    //Update the transition shader & audio fade in
    private IEnumerator Shade(bool pos)
    {
        yield return new WaitForSeconds(Time.deltaTime);
        if (0 < transition && transition < loadTimes) {
            transition = Mathf.Clamp(transition + (pos ? Time.deltaTime : -Time.deltaTime), 0, loadTimes);
            shaderValue = Mathf.Pow(transition * (10 / loadTimes), 2);
            shader.SetFloat("_Scale", shaderValue);
            if (soundtrack != null) soundtrack.volume = (loadTimes - transition) / (2 * loadTimes);
            StartCoroutine(Shade(pos));
        } else if (player != null) {
            player.Transitioning(false);
        }
        if (transition >= loadTimes) shader.SetFloat("_Scale", 500);
    }
}