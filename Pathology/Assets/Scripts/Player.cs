using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 direction;
    private Vector3 offset;
    public int moveSpd;
    public bool sprinting;
    public bool invenOpen;
    public bool paused;
    public bool confirm;
    public bool cancel;

    void Start()
    {
        
    }

    void Update()
    {
        if (!paused) {
            offset = transform.position + new Vector3(direction.x, 0, direction.y);
            transform.position = Vector3.MoveTowards(transform.position, offset, Time.deltaTime * moveSpd * (sprinting ? 2 : 1));
        }
    }

    public void Moving(InputAction.CallbackContext ctx) { direction = ctx.ReadValue<Vector2>(); }
    public void Sprint(InputAction.CallbackContext ctx) { sprinting = ctx.performed; }
    public void Inventory(InputAction.CallbackContext ctx) { invenOpen = ctx.performed; }
    public void Next(InputAction.CallbackContext ctx) { confirm = ctx.performed; }
    public void Back(InputAction.CallbackContext ctx) { cancel = ctx.performed; }
    public void Pause() { paused = true; }
    public void Unpause() { paused = false; }
}