using UnityEngine;

public class ItemCrafting : MonoBehaviour
{
    public Material[] materials;
    private GameManager manager;
    public Rigidbody rigid;
    public Renderer render;
    private Item item;

    void Start()
    {
        manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
    }

    //Returns Item to inventory if fallen out of bounds
    void FixedUpdate()
    {
        if (!rigid.useGravity) {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
        if (transform.position.y < -10) {
            manager.AddInven(item);
            Destroy(gameObject);
        }
    }

    //Dropped onto the crafting stations
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("One")) {

        } else if (other.CompareTag("Two")) {

        } else if (other.CompareTag("Three")) {

        }
    }

    public void EnableGravity(bool enabled) { rigid.useGravity = enabled; }

    //Spawn with item data
    public void Create(Item i)
    {
        item = i;
        render.material = materials[item.Id];
        EnableGravity(false);
    }
}