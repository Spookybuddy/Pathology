using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //Requirements
    public GameManager manager;
    public Camera mainCam;
    public Camera miniCam;
    public AudioSource source;
    public AudioClip[] sounds;
    public Animator animate;

    //UI
    public GameObject dialogOverlay;
    public GameObject inventoryOverlay;
    public GameObject minimapOverlay;
    public GameObject pauseOverlay;
    public GameObject[] menuOverlays;
    public GameObject[] pauseButtons;
    private int vertical;

    private Vector2 keypad;
    private Vector2 joystick;
    private Vector2 dirPad;
    public Vector3 mousition;
    private Vector2 _direction;
    private Vector3 direction;
    private Vector3 targeted;
    private Vector3 offset;
    private readonly Vector3 fixedPos = new(900, 0, 0);
    private bool mouseControlled;
    private bool mouseMoved;
    private float mouseDecay;
    private float inputDelay;
    private bool canClick;
    public bool zoomStop;

    //Settings
    public int moveSpd;
    public float mouseSensitivity;
    public float delay;
    private int mapScale;
    private bool CMEnabled;

    //Player states
    private bool sprinting;
    private bool opening;
    private bool invenOpen;
    private bool dialogOpen;
    private bool hoverItem;
    private bool paused;
    private bool confirm;
    private bool cancel;
    private bool colliding;
    private bool loading;
    private bool stepping;

    private void Awake() { manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>(); }

    void Start()
    {
        loading = true;
        canClick = true;
        zoomStop = false;
    }

    void Update()
    {
        //Move only when unpaused & not in a menu
        if (paused) {
            //Set states
            Cursor.visible = true;
            dialogOpen = false;
            invenOpen = false;
            Menus(true, false, false, false);

            //UI controls
            _direction = VectorGreater(keypad, joystick);
            if (_direction.magnitude > 0.2f && mouseDecay == 0) {
                vertical = (vertical + (int)(1.4f * _direction.y) + menuOverlays.Length) % menuOverlays.Length;
                mouseDecay = 0.2f;
            }
            mouseDecay = Mathf.Clamp01(mouseDecay - Time.deltaTime);

            //Update outlines
            for (int i = 0; i < menuOverlays.Length; i++) {
                if (mousition.x > pauseButtons[i].transform.position.x - 120 && mousition.x < pauseButtons[i].transform.position.x + 120) {
                    if (mousition.y > pauseButtons[i].transform.position.y - 30 && mousition.y < pauseButtons[i].transform.position.y + 30) {
                        vertical = i;
                    }
                }
                menuOverlays[i].SetActive(i == vertical);
            }

            //Inputs
            if (confirm) pauseButtons[vertical].GetComponent<Button>().onClick.Invoke();
            if (cancel) Unpause();
        } else {
            //Decay input delay
            inputDelay = Mathf.Clamp01(inputDelay - Time.deltaTime);
            Cursor.visible = (mouseMoved || mouseDecay > 0);
            mouseDecay = Mathf.Clamp01(mouseDecay - Time.deltaTime);
            Menus(false, invenOpen, !invenOpen && !dialogOpen, dialogOpen);
            if (invenOpen) inventoryOverlay.transform.localPosition = Vector3.MoveTowards(inventoryOverlay.transform.localPosition, Vector3.zero, Time.deltaTime * Mathf.Max(Vector3.Distance(inventoryOverlay.transform.localPosition, Vector3.zero), 10) * 10);
            else inventoryOverlay.transform.localPosition = fixedPos;

            //Inputting directions takes the highest magnitude, and overrides click navigation
            _direction = VectorGreater(keypad, joystick);

            //Pass movement data to inventory when opened
            if (invenOpen && _direction.magnitude != 0 && inputDelay == 0) {
                inputDelay = delay / 2;
                manager.Scroll(-(int)(Mathf.Clamp(_direction.y, -1, 1)));
            }

            //Player controls when not in menus
            if (!opening && !invenOpen && !dialogOpen && !loading) {
                direction = new Vector3(_direction.x, 0, _direction.y);
                if (direction.magnitude > 0) mouseControlled = false;
                offset = transform.position + direction;

                //Collision detection from 3 points
                if (Physics.Raycast(transform.position + new Vector3(0.33f, 0, 0), direction, 1)) colliding = true;
                else if (Physics.Raycast(transform.position - new Vector3(0.33f, 0, 0), direction, 1)) colliding = true;
                else if (Physics.Raycast(transform.position + new Vector3(0, 0, 0.15f), direction, 1)) colliding = true;
                else colliding = false;

                //Mouse collision detection
                if (Physics.Raycast(transform.position, (targeted - transform.position).normalized, 1)) mouseControlled = false;

                //Walk animation
                if (mouseControlled) {
                    if (Vector3.Distance(transform.position, targeted) > 0.1f) Walking(targeted - transform.position);
                    else UpdateWalk(false, false, false, false);
                } else {
                    if (direction.magnitude > 0) Walking(direction);
                    else UpdateWalk(false, false, false, false);
                }

                //Move if not going to collide
                if (!colliding) {
                    //Play sound
                    if (!stepping && (mouseControlled || direction.magnitude > 0)) {
                        stepping = true;
                        StartCoroutine(StepSound());
                    }

                    //Move by mouse or controls
                    if (mouseControlled) {
                        transform.position = Vector3.MoveTowards(transform.position, targeted, Time.deltaTime * moveSpd * (sprinting ? 2 : 1));
                        if (Vector3.Distance(transform.position, targeted) < 0.1f) mouseControlled = false;
                    } else {
                        transform.position = Vector3.MoveTowards(transform.position, offset, Time.deltaTime * moveSpd * (sprinting ? 2 : 1));
                    }
                }
            } else UpdateWalk(false, false, false, false);
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

    //Get character you're talking to
    private void OnTriggerEnter(Collider trigger)
    {
        if (trigger.CompareTag("NPC")) manager.currentConvo = trigger.GetComponent<CharacterText>();
        else if (trigger.CompareTag("Door")) {
            loading = true;
            if (int.TryParse(trigger.name, out int door)) {
                manager.Locate(door);
                manager.DoorSound(door);
                manager.Scene("Program Inner");
            }
        } else if (trigger.CompareTag("Item")) {
            hoverItem = true;
            manager.nearbyItem = trigger.GetComponent<ItemScript>().Pickup();
        }
    }

    //Inputs switch to influence conversation
    private void OnTriggerStay(Collider trigger)
    {
        if (trigger.CompareTag("NPC")) {
            //Open dialog when close enough
            if (!dialogOpen && confirm && inputDelay == 0) {
                dialogOpen = true;
                manager.Advance(1);
                inputDelay = delay;
            }
            //SOUTH pressed
            if (dialogOpen && (cancel || dirPad.Equals(Vector2.down)) && inputDelay == 0) {
                manager.Advance(4);
                inputDelay = delay;
            }
            //EAST pressed
            if (dialogOpen && (confirm || dirPad.Equals(Vector2.right)) && inputDelay == 0) {
                manager.Advance(1);
                inputDelay = delay;
            }
            //NORTH pressed
            if (dialogOpen && (opening || dirPad.Equals(Vector2.up)) && inputDelay == 0) {
                manager.Advance(2);
                inputDelay = delay;
            }
            //WEST pressed
            if (dialogOpen && (sprinting || dirPad.Equals(Vector2.left)) && inputDelay == 0) {
                manager.Advance(3);
                inputDelay = delay;
            }
        } else if (trigger.CompareTag("Item")) {
            //NORTH pressed to pickup
            if (hoverItem && confirm && inputDelay == 0) {
                hoverItem = false;
                manager.AddInven();
                manager.nearbyItem = null;
                Destroy(trigger.gameObject);
                inputDelay = delay;
            }
        }
    }

    //Remove outline effect on item
    private void OnTriggerExit(Collider trigger)
    {
        if (trigger.CompareTag("Item")) {
            trigger.GetComponent<ItemScript>().Disable();
        }
    }

    //Exit from the dialog screen
    public void CloseDialog()
    {
        dialogOpen = false;
        inputDelay = delay;
    }

    public void ClickState(bool status) { canClick = status; }
    public void ClickMovement(bool state) { CMEnabled = state; }
    public void MinimapSetting(int scale) { mapScale = scale; UpdateMap(); }
    public void Transitioning(bool load) { loading = load; }

    //Raycast a mouse click for click movement?
    private void MouseClick()
    {
        if (dialogOpen && canClick && inputDelay == 0) {
            manager.Advance(-1);
            inputDelay = delay;
        } else if (CMEnabled) {
            if (Physics.Raycast(mainCam.ScreenPointToRay(mousition), out RaycastHit hit, 100) && !dialogOpen) {
                if (hit.transform.CompareTag("Ground")) {
                    mouseControlled = true;
                    targeted = new Vector3(hit.point.x, 0, hit.point.z);
                    inputDelay = delay;
                }
            }
        }
    }

    //Update the minimap camera to the new scale
    private void Map(int val)
    {
        if (zoomStop || Mathf.Abs(val) < 1) return;
        mapScale = Mathf.Clamp(mapScale + val, 1, 3);
        UpdateMap();
    }

    private void Map()
    {
        if (zoomStop) return;
        mapScale = Mathf.Clamp((mapScale + 1) % 4, 1, 3);
        UpdateMap();
    }

    public void UpdateMap()
    {
        miniCam.orthographicSize = mapScale * 5 + 5;
        manager.SetMinimap(mapScale);
        zoomStop = true;
    }

    //Update the animator & walking states
    public void Walking(Vector3 moving)
    {
        if (Mathf.Abs(moving.z) > Mathf.Abs(moving.x)) {
            if (moving.z > 0) UpdateWalk(true, false, false, false);
            else if (moving.z < 0) UpdateWalk(false, true, false, false);
        } else {
            if (moving.x > 0) UpdateWalk(false, false, true, false);
            else if (moving.x < 0) UpdateWalk(false, false, false, true);
        }
    }

    //Set the animator boolean states
    public void UpdateWalk(bool U, bool D, bool R, bool L)
    {
        animate.SetBool("Back", U);
        animate.SetBool("Front", D);
        animate.SetBool("Right", R);
        animate.SetBool("Left", L);
    }

    //Update the menus with given bools
    private void Menus(bool P, bool I, bool M, bool D)
    {
        pauseOverlay.SetActive(P);
        inventoryOverlay.SetActive(I);
        minimapOverlay.SetActive(M);
        dialogOverlay.SetActive(D);
    }

    //Play the step sound only when walking at set intervals
    private IEnumerator StepSound()
    {
        /*
        if (mouseControlled || direction.magnitude > 0) {
            source.pitch = Random.Range(0.875f, 1.125f);
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit tile, 2)) {
                if (tile.transform.CompareTag("Ground")) source.PlayOneShot(sounds[int.Parse(tile.transform.name)], manager.Sound());
            }
        }
        */
        yield return new WaitForSeconds(sprinting ? 0.25f : 0.5f);
        stepping = false;
    }

    //Input action functions
    private void Check(InputAction.CallbackContext ctx) {
        manager.ControllerButtons(ctx.control.device.displayName);
    }

    private void Veck(InputAction.CallbackContext ctx) {
        if (ctx.ReadValue<Vector2>() != Vector2.zero) Check(ctx);
    
    }
    public void Arrows(InputAction.CallbackContext ctx) {
        keypad = ctx.ReadValue<Vector2>();
        Veck(ctx);
    }

    public void Stick(InputAction.CallbackContext ctx) {
        joystick = ctx.ReadValue<Vector2>();
        Veck(ctx);
    }

    public void Zoom(InputAction.CallbackContext ctx) {
        Map(Mathf.RoundToInt(ctx.ReadValue<Vector2>().y));
        if (Mathf.Abs(ctx.ReadValue<Vector2>().y) < 0.1f) zoomStop = false;
    }

    public void ZoomTab(InputAction.CallbackContext ctx) {
        if (ctx.performed && !zoomStop) Map();
        if (ctx.canceled && zoomStop) zoomStop = false;
    }

    public void Dpad(InputAction.CallbackContext ctx) {
        //dirPad = ctx.ReadValue<Vector2>();
    }

    public void Sprint(InputAction.CallbackContext ctx) {
        sprinting = ctx.performed;
        Check(ctx);
    }

    public void Inventory(InputAction.CallbackContext ctx) {
        if (!paused && !dialogOpen) {
            opening = ctx.performed;
            invenOpen ^= opening;
            Check(ctx);
        }
    }

    public void Next(InputAction.CallbackContext ctx) {
        confirm = ctx.performed;
        Check(ctx);
    }

    public void Back(InputAction.CallbackContext ctx) {
        cancel = ctx.performed;
        Check(ctx);
    }

    public void Pause() {
        if (!dialogOpen) paused = true;
        vertical = 0;
    }

    public void Unpause() {
        paused = false;
    }

    public void InvertPause() {
        paused = !paused;
    }

    public void Mouse(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            MouseClick();
            Check(ctx);
        }
    }

    public void MousePos(InputAction.CallbackContext ctx) {
        mousition = ctx.ReadValue<Vector2>();
    }

    public void MouseDelta(InputAction.CallbackContext ctx) {
        mouseMoved = (ctx.ReadValue<Vector2>().magnitude > mouseSensitivity);
        mouseDecay = 0.4f;
    }

    public void MouseScroll(InputAction.CallbackContext ctx) {
        if (invenOpen) manager.Scroll(-(int)(Mathf.Clamp(ctx.ReadValue<Vector2>().y, -1, 1)));
    }
}