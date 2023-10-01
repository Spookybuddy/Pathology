using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private GameManager manager;
    public Camera mainCam;
    public GameObject dialogOverlay;

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
    public int moveSpd;
    public float mouseSensitivity;
    public float delay;
    private float inputDelay;
    public bool canClick;

    private bool sprinting;
    private bool invenOpen;
    private bool dialogOpen;
    private bool paused;
    private bool confirm;
    private bool cancel;
    private bool convoRange;
    private bool colliding;

    void Awake() { manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>(); }

    void Start() { canClick = true; }

    void Update()
    {
        //Move only when unpaused & not in a menu
        if (paused) {
            Cursor.visible = true;
            mouseDecay = 0.4f;
        } else {
            //Decay input delay
            inputDelay = Mathf.Clamp01(inputDelay - Time.deltaTime);
            Cursor.visible = (mouseMoved || mouseDecay > 0);
            mouseDecay = Mathf.Clamp01(mouseDecay - Time.deltaTime);
            dialogOverlay.SetActive(dialogOpen);
            if (!invenOpen && !dialogOpen) {
                //Inputting directions takes the highest magnitude, and overrides click navigation
                _direction = VectorGreater(keypad, joystick);
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
        }
    }

    //Inputs switch to influence conversation
    private void OnTriggerStay(Collider collision)
    {
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
        if (dialogOpen && (invenOpen || dirPad.Equals(Vector2.up)) && inputDelay == 0) {
            manager.Advance(2);
            inputDelay = delay;
        }
        //WEST pressed
        if (dialogOpen && (sprinting || dirPad.Equals(Vector2.left)) && inputDelay == 0) {
            manager.Advance(3);
            inputDelay = delay;
        }
    }

    //Exit from the dialog screen
    public void CloseDialog()
    {
        dialogOpen = false;
        inputDelay = delay;
    }

    //Raycast a mouse click for click movement?
    private void MouseClick()
    {
        if (dialogOpen && canClick && inputDelay == 0) {
            manager.Advance(-1);
            inputDelay = delay;
        } else {
            if (Physics.Raycast(mainCam.ScreenPointToRay(mousition), out RaycastHit hit, 100)) {
                mouseControlled = true;
                targeted = new Vector3(hit.point.x, 0, hit.point.z);
            }
        }
    }

    //Input action functions
    private void Check(InputAction.CallbackContext ctx) { manager.ControllerButtons(ctx.control.device.displayName); }
    private void Veck(InputAction.CallbackContext ctx) { if (ctx.ReadValue<Vector2>() != Vector2.zero) Check(ctx); }
    public void Arrows(InputAction.CallbackContext ctx) { keypad = ctx.ReadValue<Vector2>(); Veck(ctx); }
    public void Stick(InputAction.CallbackContext ctx) { joystick = ctx.ReadValue<Vector2>(); Veck(ctx); }
    public void Dpad(InputAction.CallbackContext ctx) { dirPad = ctx.ReadValue<Vector2>(); }
    public void Sprint(InputAction.CallbackContext ctx) { sprinting = ctx.performed; Check(ctx); }
    public void Inventory(InputAction.CallbackContext ctx) { invenOpen = ctx.performed; Check(ctx); }
    public void Next(InputAction.CallbackContext ctx) { confirm = ctx.performed; Check(ctx); }
    public void Back(InputAction.CallbackContext ctx) { cancel = ctx.performed; Check(ctx); }
    public void Pause() { paused = true; }
    public void Unpause() { paused = false; }
    public void InvertPause() { paused = !paused; }
    public void Mouse(InputAction.CallbackContext ctx) { if (ctx.performed) MouseClick(); }
    public void MousePos(InputAction.CallbackContext ctx) { mousition = ctx.ReadValue<Vector2>(); }
    public void MouseDelta(InputAction.CallbackContext ctx) { mouseMoved = (ctx.ReadValue<Vector2>().magnitude > mouseSensitivity); mouseDecay = 0.4f; }
}