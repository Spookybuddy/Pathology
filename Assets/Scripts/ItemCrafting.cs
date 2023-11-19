using UnityEngine;

public class ItemCrafting : MonoBehaviour
{
    public Material[] materials;
    private GameManager manager;
    public Rigidbody rigid;
    public Renderer render;
    public BoxCollider hitbox;
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
        if (transform.position.y < -10) Store();
    }

    //Enabled collider and gravity
    public void Enable(bool gravity, bool collider)
    {
        rigid.useGravity = gravity;
        hitbox.enabled = collider;
    }

    //Spawn with item data
    public void Create(Item i, bool g, bool c)
    {
        item = i;
        render.material = materials[item.Id];
        Enable(g, c);
    }

    //Return item to inventory
    public void Store()
    {
        manager.AddInven(item);
        Destroy(gameObject);
    }

    //Pass item data before destroying
    public int GetData(int X)
    {
        switch (X) {
            case 0:
                return item.Vitamin;
            case 1:
                return item.Mineral;
            default:
                return item.Enzymes;
        }
    }
}