using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private GameManager manager;

    private Vector2 keypad;
    private Vector2 joystick;
    private Vector2 _direction;
    private Vector3 direction;
    private Vector3 offset;
    public int moveSpd;

    public bool sprinting;
    public bool invenOpen;
    public bool dialogOpen;
    public bool inputDelay;
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
        if (paused) {

        } else {
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
        manager.currentConvo = trigger.GetComponent<CharacterText>();
    }

    //Inputs switch to influence conversation
    private void OnTriggerStay(Collider collision)
    {
        //Open dialog when close enough
        if (!dialogOpen && confirm && !inputDelay) {
            dialogOpen = true;
            manager.Advance(1);
            CallDelay(0.1f);
        }
        //SOUTH pressed
        if (dialogOpen && cancel && !inputDelay) {
            manager.Decline();
            CallDelay(0.1f);
        }
        //EAST pressed
        if (dialogOpen && confirm && !inputDelay) {
            manager.Advance(1);
            CallDelay(0.1f);
        }
        //NORTH pressed
        if (dialogOpen && invenOpen && !inputDelay) {
            manager.Advance(2);
            CallDelay(0.1f);
        }
        //WEST pressed
        if (dialogOpen && sprinting && !inputDelay) {
            manager.Advance(3);
            CallDelay(0.1f);
        }
    }

    public void CloseDialog()
    {
        dialogOpen = false;
        CallDelay(0.1f);
    }

    private void CallDelay(float time)
    {
        inputDelay = true;
        StartCoroutine(DelayInputs(time));
    }

    //Prevent button spam
    private IEnumerator DelayInputs(float time)
    {
        yield return new WaitForSeconds(time);
        inputDelay = false;
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
}