using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    //Save data settings
    private string filename;
    private string[] data;
    public string settings;
    public int volume;
    public bool clickMove;
    public int txtSpd;

    //Menu visuals
    public GameObject Title;
    public GameObject Options;
    public bool menuUp;
    public Slider volumeLvl;
    public Toggle toggle;
    public TextMeshProUGUI speed;
    private readonly string[] texts = new string[] { "Slow", "Normal", "Fast", "Instant" };

    void Awake()
    {
        filename = Application.streamingAssetsPath + "/SaveData.txt";
        data = File.ReadAllLines(filename);
        settings = data[6];
        menuUp = true;
    }

    //Read in and set all the values from the save file
    void Start()
    {
        volume = int.Parse(settings.Substring(0, 3));
        clickMove = settings[3].Equals('1');
        txtSpd = int.Parse(settings.Substring(4, 1));
        SetValues();
        Title.SetActive(true);
        Options.SetActive(false);
    }

    //Write the vales to the save data & file
    private void Save()
    {
        string updated = "";
        updated += volume.ToString("000");
        updated += clickMove ? "1" : "0";
        updated += txtSpd.ToString("0");
        updated += settings.Substring(5);
        settings = updated;
        data[6] = settings;
        File.WriteAllLines(filename, data);
    }

    //Erases the save data
    public void Reset()
    {
        data = new string[6];
        Save();
    }

    //Changes the menu displaying
    public void ChangeMenu()
    {
        menuUp = !menuUp;
        Title.SetActive(menuUp);
        Options.SetActive(!menuUp);
    }

    //Volume of 100%
    public void ChangeVolume()
    {
        volume = (int)volumeLvl.value;
        Save();
    }

    //Toggle click movement active - BUG: Called when toggle display is changed; Fix soon
    public void ToggleClick() {
        clickMove = !clickMove;
        Save();
    }

    //Text speed value
    public void ChangeSpeed(int value)
    {
        txtSpd = (txtSpd + value + 4) % 4;
        speed.text = texts[txtSpd];
        Save();
    }

    //Update displays
    private void SetValues()
    {
        toggle.isOn = clickMove;
        volumeLvl.value = volume;
        speed.text = texts[txtSpd];
    }

    //Exit the game
    public void Quit()
    {
        Save();
        Application.Quit();
    }

    //Load scene
    public void Load(string scene)
    {
        StartCoroutine(ChangeScene(scene));
    }

    private IEnumerator ChangeScene(string scene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);
        while (!asyncLoad.isDone) yield return null;
    }
}