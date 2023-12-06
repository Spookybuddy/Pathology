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
    private bool clickToggle;
    public int txtSpd;

    //Menu visuals
    public GameObject Title;
    public GameObject Options;
    public bool menuUp;
    public Slider volumeLvl;
    public Toggle toggle;
    public TextMeshProUGUI speed;
    private readonly string[] texts = new string[] { "Slow", "Normal", "Fast", "Instant" };

    //Menu controls
    private Vector2 joystick;
    private Vector2 keyboard;
    private Vector2 dirpad;
    public Vector2 _direction;
    private bool inputting;
    public int horizontal;
    public int vertical;
    public GameObject[] MM;
    public GameObject[] SM;
    private float delay;

    void Awake()
    {
        filename = Application.streamingAssetsPath + "/SaveData.txt";
        data = File.ReadAllLines(filename);
        settings = data[6];
        menuUp = true;
        clickToggle = false;
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

    //Event system because it never works
    void Update()
    {
        _direction = VectorGreater(joystick, keyboard);
        _direction = VectorGreater(_direction, dirpad);
        if (_direction.magnitude > 0.167f) inputting = true;
        delay = Mathf.Clamp01(delay - Time.deltaTime);
        if (inputting) {
            if (delay == 0) {
                vertical = (int)(vertical + (1.25 * _direction.y)) % (menuUp ? MM.Length : SM.Length);
                delay = 0.25f;
                inputting = false;
            }
        }
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
        vertical = 0;
    }

    //Volume of 100%
    public void ChangeVolume()
    {
        volume = (int)volumeLvl.value;
        Save();
    }

    //Toggle click movement active - BUG: Called when toggle display is changed; Fix soon
    public void ToggleClick() {
        if (clickToggle) clickMove = !clickMove;
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
        clickToggle = true;
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

    //Returns greatest magnitude X Y of Vector2. This allows for dual controller use without overriding the other
    private Vector2 VectorGreater(Vector2 A, Vector2 B)
    {
        if (Mathf.Abs(A.x) > Mathf.Abs(B.x)) {
            if (Mathf.Abs(A.y) > Mathf.Abs(B.y)) return A;
            else return new Vector2(A.x, B.y);
        } else {
            if (Mathf.Abs(A.y) > Mathf.Abs(B.y)) return new Vector2(B.x, A.y);
            else return B;
        }
    }

    private void Input()
    {
        vertical = (int)(vertical + (1.25 * _direction.y)) % (menuUp ? MM.Length : SM.Length);
        inputting = true;
        delay = 0.4f;
    }

    public void Joystick(InputAction.CallbackContext ctx) {
        joystick = ctx.ReadValue<Vector2>();
        if (ctx.performed) Input();
    }
    public void Keypad(InputAction.CallbackContext ctx) {
        keyboard = ctx.ReadValue<Vector2>();
        if (ctx.performed) Input();
    }
    public void DPad(InputAction.CallbackContext ctx) {
        dirpad = ctx.ReadValue<Vector2>();
        if (ctx.performed) Input();
    }
    public void Confirm(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !inputting) {
            Debug.Log(menuUp ? MM[Mathf.Abs(vertical)].name : "No");
            Input();
        }
    }
    public void Cancel(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !inputting) Input();
    }
}