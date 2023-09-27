using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private GameManager manager;
    public GameObject dialogOverlay;

    private Vector2 keypad;
    private Vector2 joystick;
    private Vector2 _direction;
    private Vector3 direction;
    private Vector3 offset;
    public int moveSpd;
    public float delay;
    private float inputDelay;

    public bool sprinting;
    public bool invenOpen;
    public bool dialogOpen;
    public bool paused;
    public bool confirm;
    public bool cancel;
    public bool convoRange;
    public bool colliding;

    void Awake()
    {
        manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
    }

    void Update()
    {
        //Move only when unpaused & not in a menu
        if (paused) {
            Cursor.visible = true;
        } else {
            //Decay input delay
            inputDelay = Mathf.Clamp01(inputDelay - Time.deltaTime);
            Cursor.visible = dialogOpen;
            dialogOverlay.SetActive(dialogOpen);
            if (!invenOpen && !dialogOpen) {
                _direction = VectorGreater(keypad, joystick);
                direction = new Vector3(_direction.x, 0, _direction.y);
                offset = transform.position + direction;

                //Collision detection from 3 points
                if (Physics.Raycast(transform.position + new Vector3(0.33f, 0, 0), direction, 1)) colliding = true;
                else if (Physics.Raycast(transform.position - new Vector3(0.33f, 0, 0), direction, 1)) colliding = true;
                else if (Physics.Raycast(transform.position + new Vector3(0, 0, 0.15f), direction, 1)) colliding = true;
                else colliding = false;
                if (!colliding) transform.position = Vector3.MoveTowards(transform.position, offset, Time.deltaTime * moveSpd * (sprinting ? 2 : 1));
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
        if (dialogOpen && cancel && inputDelay == 0) {
            manager.Decline();
            inputDelay = delay;
        }
        //EAST pressed
        if (dialogOpen && confirm && inputDelay == 0) {
            manager.Advance(1);
            inputDelay = delay;
        }
        //NORTH pressed
        if (dialogOpen && invenOpen && inputDelay == 0) {
            manager.Advance(2);
            inputDelay = delay;
        }
        //WEST pressed
        if (dialogOpen && sprinting && inputDelay == 0) {
            manager.Advance(3);
            inputDelay = delay;
        }
    }

    public void CloseDialog()
    {
        dialogOpen = false;
        inputDelay = delay;
    }

    //Input action functions
    public void Arrows(InputAction.CallbackContext ctx) { keypad = ctx.ReadValue<Vector2>(); }
    public void Stick(InputAction.CallbackContext ctx) { joystick = ctx.ReadValue<Vector2>(); }
    public void Sprint(InputAction.CallbackContext ctx) { sprinting = ctx.performed; }
    public void Inventory(InputAction.CallbackContext ctx) { invenOpen = ctx.performed; }
    public void Next(InputAction.CallbackContext ctx) { confirm = ctx.performed; }
    public void Back(InputAction.CallbackContext ctx) { cancel = ctx.performed; }
    public void Pause() { paused = true; }
    public void Unpause() { paused = false; }
    public void InvertPause() { paused = !paused; }
}