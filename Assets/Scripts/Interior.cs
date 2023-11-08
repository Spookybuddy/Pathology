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
    public GameObject itemObj;
    public GameObject current;
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
    private bool sprinting;
    public int index;

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
            _direction = VectorGreater(dirPad, joystick);

            //Pass movement data to invenetory when opened
            if (invenOpen && _direction.magnitude != 0 && inputDelay == 0) {
                inputDelay = delay / 2;
                manager.Scroll(-(int)(Mathf.Clamp(_direction.y * 2, -1, 1)));
            }

            //Player can control a cursor when in crafting mode
            if (!opening && !invenOpen && !dialogOpen && canCraft) {
                direction = new Vector3(_direction.x, _direction.y, 0);
                if (direction.magnitude > 0) mouseMoved = false;
                offset = transform.position + direction;

                if (invenOpen && confirm) {
                    current = Instantiate(itemObj, new Vector3(0, 3, 0), Quaternion.identity) as GameObject;
                    invenOpen = false;
                }

                if (mouseMoved) {

                } else {
                    transform.position = Vector3.MoveTowards(transform.position, offset, Time.deltaTime * moveSpd);
                }
            }

            //Player Inputs
            if (dialogOpen) {
                //SOUTH pressed
                if ((cancel || dirPad.Equals(Vector2.down)) && inputDelay == 0) {
                    manager.Advance(4);
                    inputDelay = delay;
                }
                //EAST pressed
                if ((confirm || dirPad.Equals(Vector2.right)) && inputDelay == 0) {
                    manager.Advance(1);
                    inputDelay = delay;
                }
                //NORTH pressed
                if ((opening || dirPad.Equals(Vector2.up)) && inputDelay == 0) {
                    manager.Advance(2);
                    inputDelay = delay;
                }
                //WEST pressed
                if ((sprinting || dirPad.Equals(Vector2.left)) && inputDelay == 0) {
                    manager.Advance(3);
                    inputDelay = delay;
                }
            }
        }
    }

    //Set the gamemode
    public void GameMode(bool craft)
    {
        canCraft = craft;
        dialogOpen = !craft;
        invenOpen = false;
    }

    //Exit the scene
    public void Exit()
    {
        manager.Scene("Programming");
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

    public void ClickState(bool status) { canClick = status; }

    //Input action functions
    private void Check(InputAction.CallbackContext ctx) { manager.ControllerButtons(ctx.control.device.displayName); }
    private void Veck(InputAction.CallbackContext ctx) { if (ctx.ReadValue<Vector2>() != Vector2.zero) Check(ctx); }
    public void Stick(InputAction.CallbackContext ctx) { joystick = ctx.ReadValue<Vector2>(); Veck(ctx); }
    public void Dpad(InputAction.CallbackContext ctx) { dirPad = ctx.ReadValue<Vector2>(); }
    public void Inventory(InputAction.CallbackContext ctx) { if (canCraft) { opening = ctx.performed; invenOpen ^= opening; Check(ctx); } }
    public void Sprint(InputAction.CallbackContext ctx) { sprinting = ctx.performed; Check(ctx); }
    public void Next(InputAction.CallbackContext ctx) { confirm = ctx.performed; Check(ctx); }
    public void Back(InputAction.CallbackContext ctx) { cancel = ctx.performed; Check(ctx); }
    public void Left(InputAction.CallbackContext ctx) { index = (index + 3) % 4; }
    public void Right(InputAction.CallbackContext ctx) { index = (index + 1) % 4; }
    public void Mouse(InputAction.CallbackContext ctx) { if (ctx.performed) MouseClick(); }
    public void MousePos(InputAction.CallbackContext ctx) { mousition = ctx.ReadValue<Vector2>(); }
    public void MouseDelta(InputAction.CallbackContext ctx) { mouseMoved = (ctx.ReadValue<Vector2>().magnitude > mouseSensitivity); }
    public void MouseScroll(InputAction.CallbackContext ctx) { if (invenOpen) manager.Scroll(-(int)(Mathf.Clamp(ctx.ReadValue<Vector2>().y, -1, 1))); }
}