using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public GameManager manager;
    public Camera mainCam;
    public Camera miniCam;
    public GameObject dialogOverlay;
    public GameObject inventoryOverlay;
    public GameObject minimapOverlay;
    public GameObject pauseOverlay;

    private Vector2 keypad;
    private Vector2 joystick;
    private Vector2 dirPad;
    private Vector3 mousition;
    private Vector2 _direction;
    private Vector3 direction;
    private Vector3 targeted;
    private Vector3 offset;
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

    private void Awake() { manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>(); }

    void Start() {
        canClick = true;
        zoomStop = false;
    }

    void Update()
    {
        //Move only when unpaused & not in a menu
        if (paused) {
            Cursor.visible = true;
            dialogOpen = false;
            invenOpen = false;
            Menus(true, false, false, false);
        } else {
            //Decay input delay
            inputDelay = Mathf.Clamp01(inputDelay - Time.deltaTime);
            Cursor.visible = (mouseMoved || mouseDecay > 0);
            mouseDecay = Mathf.Clamp01(mouseDecay - Time.deltaTime);
            Menus(false, invenOpen, !invenOpen && !dialogOpen, dialogOpen);

            //Inputting directions takes the highest magnitude, and overrides click navigation
            _direction = VectorGreater(keypad, joystick);

            //Pass movement data to invenetory when opened
            if (invenOpen && _direction.magnitude != 0 && inputDelay == 0) {
                inputDelay = delay / 2;
                manager.Scroll(-(int)(Mathf.Clamp(_direction.y, -1, 1)));
            }

            //Player controls when not in menus
            if (!opening && !invenOpen && !dialogOpen) {
                direction = new Vector3(_direction.x, 0, _direction.y);
                if (direction.magnitude > 0) mouseControlled = false;
                offset = transform.position + direction;

                if (mouseControlled) transform.position = Vector3.MoveTowards(transform.position, targeted, Time.deltaTime * moveSpd * (sprinting ? 2 : 1));

                //Collision detection from 3 points
                if (Physics.Raycast(transform.position + new Vector3(0.33f, 0, 0), direction, 1)) colliding = true;
                else if (Physics.Raycast(transform.position - new Vector3(0.33f, 0, 0), direction, 1)) colliding = true;
                else if (Physics.Raycast(transform.position + new Vector3(0, 0, 0.15f), direction, 1)) colliding = true;
                else colliding = false;
                if (!colliding && !mouseControlled) transform.position = Vector3.MoveTowards(transform.position, offset, Time.deltaTime * moveSpd * (sprinting ? 2 : 1));
            }
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
            manager.Locate(int.Parse(trigger.gameObject.name));
            manager.Scene("Program Inner");
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

    //Exit from the dialog screen
    public void CloseDialog()
    {
        dialogOpen = false;
        inputDelay = delay;
    }

    public void ClickState(bool status) { canClick = status; }
    public void ClickMovement(bool state) { CMEnabled = state; }
    public void MinimapSetting(int scale) { mapScale = scale; UpdateMap(); }

    //Raycast a mouse click for click movement?
    private void MouseClick()
    {
        if (dialogOpen && canClick && inputDelay == 0) {
            manager.Advance(-1);
            inputDelay = delay;
        } else if (CMEnabled) {
            if (Physics.Raycast(mainCam.ScreenPointToRay(mousition), out RaycastHit hit, 100) && !dialogOpen) {
                mouseControlled = true;
                targeted = new Vector3(hit.point.x, 0, hit.point.z);
                inputDelay = delay;
            }
        }
    }

    //Update the minimap camera to the new scale
    private void Map(int val)
    {
        if (zoomStop || Mathf.Abs(val) < 1) return;
        mapScale = Mathf.Clamp(mapScale + val, 1, 3);
        UpdateMap();
        manager.SetMinimap(mapScale);
        zoomStop = true;
    }

    private void Map()
    {
        if (zoomStop) return;
        mapScale = Mathf.Clamp((mapScale + 1) % 4, 1, 3);
        UpdateMap();
        manager.SetMinimap(mapScale);
        zoomStop = true;
    }

    public void UpdateMap() { miniCam.orthographicSize = mapScale * 5; }

    //Update the menus with given bools
    private void Menus(bool P, bool I, bool M, bool D)
    {
        pauseOverlay.SetActive(P);
        inventoryOverlay.SetActive(I);
        minimapOverlay.SetActive(M);
        dialogOverlay.SetActive(D);
    }

    //Input action functions
    private void Check(InputAction.CallbackContext ctx) { manager.ControllerButtons(ctx.control.device.displayName); }
    private void Veck(InputAction.CallbackContext ctx) { if (ctx.ReadValue<Vector2>() != Vector2.zero) Check(ctx); }
    public void Arrows(InputAction.CallbackContext ctx) { keypad = ctx.ReadValue<Vector2>(); Veck(ctx); }
    public void Stick(InputAction.CallbackContext ctx) { joystick = ctx.ReadValue<Vector2>(); Veck(ctx); }
    public void Zoom(InputAction.CallbackContext ctx) { Map(Mathf.RoundToInt(ctx.ReadValue<Vector2>().y)); if (Mathf.Abs(ctx.ReadValue<Vector2>().y) < 0.1f) zoomStop = false; }
    public void ZoomTab(InputAction.CallbackContext ctx) { Map(); if (ctx.canceled && zoomStop) zoomStop = false; }
    public void Dpad(InputAction.CallbackContext ctx) { dirPad = ctx.ReadValue<Vector2>(); }
    public void Sprint(InputAction.CallbackContext ctx) { sprinting = ctx.performed; Check(ctx); }
    public void Inventory(InputAction.CallbackContext ctx) { if (!paused && !dialogOpen) { opening = ctx.performed; invenOpen ^= opening; Check(ctx); } }
    public void Next(InputAction.CallbackContext ctx) { confirm = ctx.performed; Check(ctx); }
    public void Back(InputAction.CallbackContext ctx) { cancel = ctx.performed; Check(ctx); }
    public void Pause() { if (!dialogOpen) paused = true; }
    public void Unpause() { paused = false; }
    public void InvertPause() { paused = !paused; }
    public void Mouse(InputAction.CallbackContext ctx) { if (ctx.performed) MouseClick(); }
    public void MousePos(InputAction.CallbackContext ctx) { mousition = ctx.ReadValue<Vector2>(); }
    public void MouseDelta(InputAction.CallbackContext ctx) { mouseMoved = (ctx.ReadValue<Vector2>().magnitude > mouseSensitivity); mouseDecay = 0.4f; }
    public void MouseScroll(InputAction.CallbackContext ctx) { if (invenOpen) manager.Scroll(-(int)(Mathf.Clamp(ctx.ReadValue<Vector2>().y, -1, 1))); }
}