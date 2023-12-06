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
    private int volume;
    private bool clickMove;
    private bool clickToggle;
    private int txtSpd;

    //Menu visuals
    public GameObject Title;
    public GameObject Options;
    private bool menuUp;
    public Slider volumeLvl;
    public Toggle toggle;
    public TextMeshProUGUI speed;
    private readonly string[] texts = new string[] { "Slow", "Normal", "Fast", "Instant" };

    //Shader
    public Material shader;
    private float shaderValue;
    private float transition;

    //Menu controls
    private Vector2 joystick;
    private Vector2 keyboard;
    private Vector2 dirpad;
    private Vector2 _direction;
    private bool inputting;
    private int horizontal;
    private int vertical;
    public float sensitivity;
    public GameObject[] MM;
    public GameObject[] MMO;
    public GameObject[] SM;
    public GameObject[] SMO;
    public Vector2[] specialPadding;
    private float delay;
    private Vector2 Mpos;
    private readonly Vector2 padding = new Vector2(170, 50);

    void Awake()
    {
        //Read file data
        filename = Application.streamingAssetsPath + "/SaveData.txt";
        data = File.ReadAllLines(filename);
        settings = data[6];
        menuUp = true;
        clickToggle = false;
    }

    //Read in and set all the values from the save file
    void Start()
    {
        //Start with shader full
        transition = 0.87f;
        StartCoroutine(Shade(false));

        //Try reading the save file
        if (!int.TryParse(settings, out int result)) settings = "100100";
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
        UIOverlays();
        if (inputting) {
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
        
        //Mouse over
        if (menuUp) {
            for (int i = 0; i < MM.Length; i++) {
                if (Mpos.x > MM[i].transform.position.x - padding.x && Mpos.x < MM[i].transform.position.x + padding.x) {
                    if (Mpos.y > MM[i].transform.position.y - padding.y && Mpos.y < MM[i].transform.position.y + padding.y) {
                        vertical = i;
                    }
                }
            }
        } else {
            for (int i = 0; i < SM.Length; i++) {
                if (Mpos.x > SM[i].transform.position.x - specialPadding[i].x && Mpos.x < SM[i].transform.position.x + specialPadding[i].x) {
                    if (Mpos.y > SM[i].transform.position.y - specialPadding[i].y && Mpos.y < SM[i].transform.position.y + specialPadding[i].y) {
                        vertical = i;
                    }
                }
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
        if (transition != 0) return;
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
        if (transition != 0) return;
        Save();
        Application.Quit();
    }

    //Load scene
    public void Load(string scene)
    {
        if (transition != 0) return;
        StartCoroutine(ChangeScene(scene));
    }

    //Change the scene with the tranisition shader
    private IEnumerator ChangeScene(string scene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);
        asyncLoad.allowSceneActivation = false;
        transition = 0.001f;
        StartCoroutine(Shade(true));
        while (!asyncLoad.isDone) {
            if (asyncLoad.progress >= 0.9f && transition >= 0.875f) asyncLoad.allowSceneActivation = true;
            yield return null;
        }
    }

    //Update the transition shader
    private IEnumerator Shade(bool pos)
    {
        yield return new WaitForSeconds(Time.deltaTime);
        if (0 < transition && transition < 0.875f) {
            transition = Mathf.Clamp(transition + (pos ? Time.deltaTime : -Time.deltaTime), 0, 0.875f);
            shaderValue = Mathf.Pow(transition * (10 / 0.875f), 2);
            shader.SetFloat("_Scale", shaderValue);
            StartCoroutine(Shade(pos));
        }
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
    public void MousePosition(InputAction.CallbackContext ctx)
    {
        Mpos = ctx.ReadValue<Vector2>();
    }
}