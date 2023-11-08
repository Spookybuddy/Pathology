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
        if (transform.position.y < -6) {
            manager.AddInven(item);
            Destroy(gameObject);
        }
    }

    //Dropped onto the crafting stations
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Craftng: " + other.name);
    }

    public void Create(Item i)
    {
        item = i;
        render.material = materials[item.Id];
    }
}