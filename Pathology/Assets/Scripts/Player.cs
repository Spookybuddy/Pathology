using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private GameManager manager;

    private Vector2 keypad;
    private Vector2 joystick;
    public Vector2 direction;
    private Vector3 offset;
    public int moveSpd;

    public bool sprinting;
    public bool invenOpen;
    public bool dialogOpen;
    public bool paused;
    public bool confirm;
    public bool cancel;

    public bool colliding;

    void Awake()
    {
        manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
    }

    void Update()
    {
        if (!paused && !invenOpen && !dialogOpen) {
            direction = VectorGreater(keypad, joystick);
            offset = transform.position + new Vector3(direction.x, 0, direction.y);

            //Collision detection from 3 points
            if (Physics.Raycast(transform.position + new Vector3(0.33f, 0, 0), new Vector3(direction.x, 0, direction.y), 1)) colliding = true;
            else if (Physics.Raycast(transform.position - new Vector3(0.33f, 0, 0), new Vector3(direction.x, 0, direction.y), 1)) colliding = true;
            else if (Physics.Raycast(transform.position + new Vector3(0, 0, 0.15f), new Vector3(direction.x, 0, direction.y), 1)) colliding = true;
            else colliding = false;
            if (!colliding) transform.position = Vector3.MoveTowards(transform.position, offset, Time.deltaTime * moveSpd * (sprinting ? 2 : 1));
        }
    }

    //Returns greatest magnitude X Y of Vector2
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

    //Get dialog
    private void OnTriggerEnter(Collider collision)
    {
        manager.currentConvo = collision.GetComponent<CharacterText>();
    }

    //Converse
    private void OnTriggerStay(Collider collision)
    {
        if (dialogOpen && cancel) dialogOpen = false;
        if (!dialogOpen && confirm) dialogOpen = true;
        if (dialogOpen && confirm) manager.Advance();
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