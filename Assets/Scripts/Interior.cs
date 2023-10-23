using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Interior : MonoBehaviour
{
    public GameManager manager;
    public Camera mainCam;
    public GameObject inventoryOverlay;
    public GameObject dialogOverlay;
    public Vector3 itemPadding;
    public Vector3 invenPadding;

    public int moveSpd;
    public float mouseSensitivity;
    public float delay;

    //States
    public bool canCraft;
    private bool opening;
    private bool invenOpen;
    private bool dialogOpen;
    private bool paused;
    private bool confirm;
    private bool cancel;

    private Vector2 keypad;
    private Vector2 joystick;
    private Vector2 dirPad;
    private Vector3 mousition;
    private Vector2 _direction;
    private Vector3 direction;
    private Vector3 offset;
    private bool mouseMoved;
    private float inputDelay;
    private bool canClick;

    void Start()
    {
        Cursor.visible = true;
    }

    void Update()
    {
        //Move only when unpaused & not in a menu
        if (paused) {
        } else {
            //Decay input delay
            inputDelay = Mathf.Clamp01(inputDelay - Time.deltaTime);
            dialogOverlay.SetActive(dialogOpen);
            inventoryOverlay.SetActive(invenOpen);

            //Inputting directions takes the highest magnitude, and overrides click navigation
            _direction = VectorGreater(keypad, joystick);

            //Pass movement data to invenetory when opened
            if (invenOpen && _direction.magnitude != 0 && inputDelay == 0) {
                inputDelay = delay / 2;
                manager.Scroll(-(int)(Mathf.Clamp(_direction.y, -1, 1)));
            }

            //Player can control a cursor when in crafting mode
            if (!opening && !invenOpen && !dialogOpen && canCraft) {
                direction = new Vector3(_direction.x, 0, _direction.y);
                if (direction.magnitude > 0) mouseMoved = false;
                offset = transform.position + direction;

                if (mouseMoved) {

                } else {
                    transform.position = Vector3.MoveTowards(transform.position, offset, Time.deltaTime * moveSpd);
                }
            }
        }
    }

    //Set the gamemode
    public void GameMode(bool craft)
    {
        canCraft = craft;
        dialogOpen = false;
        invenOpen = false;
    }

    //Raycast a mouse click for click movement?
    private void MouseClick()
    {
        if (dialogOpen && canClick && inputDelay == 0) {
            manager.Advance(-1);
            inputDelay = delay;
        } else {
            inputDelay = delay;
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

    //Input action functions
    private void Check(InputAction.CallbackContext ctx) { manager.ControllerButtons(ctx.control.device.displayName); }
    private void Veck(InputAction.CallbackContext ctx) { if (ctx.ReadValue<Vector2>() != Vector2.zero) Check(ctx); }
    public void Arrows(InputAction.CallbackContext ctx) { keypad = ctx.ReadValue<Vector2>(); Veck(ctx); }
    public void Stick(InputAction.CallbackContext ctx) { joystick = ctx.ReadValue<Vector2>(); Veck(ctx); }
    public void Dpad(InputAction.CallbackContext ctx) { dirPad = ctx.ReadValue<Vector2>(); }
    public void Inventory(InputAction.CallbackContext ctx) { if (canCraft) { opening = ctx.performed; invenOpen ^= opening; Check(ctx); } }
    public void Next(InputAction.CallbackContext ctx) { confirm = ctx.performed; Check(ctx); }
    public void Back(InputAction.CallbackContext ctx) { cancel = ctx.performed; Check(ctx); }
    public void Pause() { paused = true; }
    public void Unpause() { paused = false; }
    public void InvertPause() { paused = !paused; }
    public void Mouse(InputAction.CallbackContext ctx) { if (ctx.performed) MouseClick(); }
    public void MousePos(InputAction.CallbackContext ctx) { mousition = ctx.ReadValue<Vector2>(); }
    public void MouseDelta(InputAction.CallbackContext ctx) { mouseMoved = (ctx.ReadValue<Vector2>().magnitude > mouseSensitivity); }
    public void MouseScroll(InputAction.CallbackContext ctx) { if (invenOpen) manager.Scroll(-(int)(Mathf.Clamp(ctx.ReadValue<Vector2>().y, -1, 1))); }
}