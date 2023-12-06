using System.Collections;
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
    private int horizontal;
    private int vertical;
    public float sensitivity;
    public GameObject[] MM;
    public GameObject[] MMO;
    public GameObject[] SM;
    public GameObject[] SMO;
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
        UIOverlays();
    }

    //Event system because it never works
    void Update()
    {
        _direction = VectorGreater(joystick, keyboard);
        _direction = VectorGreater(_direction, dirpad);
        if (_direction.magnitude > 0.167f) inputting = true;
        delay = Mathf.Clamp01(delay - Time.deltaTime);
        if (inputting) {
            UIOverlays();
            if (delay == 0) {
                if (menuUp) {
                    vertical = (vertical - (int)(sensitivity * _direction.y) + MM.Length) % MM.Length;
                } else {
                    vertical = (vertical - (int)(sensitivity * _direction.y) + SM.Length) % SM.Length;
                    //Special cases for different UI elements
                    switch (vertical) {
                        case 0:
                            horizontal = volume;
                            horizontal = Mathf.Clamp(horizontal + (int)(sensitivity * _direction.x), 0, 100);
                            volumeLvl.value = horizontal;
                            break;
                        case 2:
                            if ((int)(sensitivity * _direction.x) != 0) {
                                ChangeSpeed((int)(sensitivity * _direction.x));
                                horizontal = txtSpd;
                            }
                            break;
                        case 3:
                            if ((int)(1.5f * _direction.x) != 0) vertical = 4;
                            break;
                        case 4:
                            if ((int)(sensitivity * _direction.x) != 0) vertical = 3;
                            break;
                        default:
                            horizontal = 0;
                            break;
                    }
                }
                delay = 0.2f;
                inputting = false;
            }
        }
        //outline.transform.localPosition = (menuUp ? MM[vertical].transform.localPosition : SM[vertical].transform.localPosition);
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
        UIOverlays();
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

    //UI indicators
    private void UIOverlays()
    {
        if (menuUp) for (int i = 0; i < MMO.Length; i++) MMO[i].SetActive(i == vertical);
        else for (int i = 0; i < SMO.Length; i++) SMO[i].SetActive(i == vertical);
    }

    //First input with larger delay
    private void Input(float y)
    {
        if (menuUp) vertical = (vertical - (int)(1.7f * y) + MM.Length) % MM.Length;
        else vertical = (vertical - (int)(1.7f * y) + SM.Length) % SM.Length;
        inputting = true;
        delay = 0.333f;
    }

    public void Joystick(InputAction.CallbackContext ctx) {
        joystick = ctx.ReadValue<Vector2>();
        if (ctx.performed && !inputting) Input(joystick.y);
    }
    public void Keypad(InputAction.CallbackContext ctx) {
        keyboard = ctx.ReadValue<Vector2>();
        if (ctx.performed && !inputting) Input(keyboard.y);
    }
    public void DPad(InputAction.CallbackContext ctx) {
        dirpad = ctx.ReadValue<Vector2>();
        if (ctx.performed && !inputting) Input(dirpad.y);
    }
    public void Confirm(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) {
            if (menuUp) {
                MM[vertical].GetComponent<Button>().onClick.Invoke();
            } else {
                switch (vertical) {
                    case 0:
                        break;
                    case 1:
                        toggle.isOn = !toggle.isOn;
                        break;
                    case 2:
                        break;
                    default:
                        SM[vertical].GetComponent<Button>().onClick.Invoke();
                        break;
                }
            }
        }
    }
    public void Cancel(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) {
            if (!menuUp) ChangeMenu();
        }
    }
}