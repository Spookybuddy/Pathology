using UnityEngine;
using System.IO;

[System.Serializable]
public class Item
{
    public int Id;
    public string Name;
    public char Category;
    public int Quantity;
    public int Vitamin;
    public int Mineral;
    public int Enzymes;

    public void Copy(Item i)
    {
        Id = i.Id;
        Name = i.Name;
        Category = i.Category;
        Quantity = i.Quantity;
        Vitamin = i.Vitamin;
        Mineral = i.Mineral;
        Enzymes = i.Enzymes;
    }
}

public class ItemScript : MonoBehaviour
{
    private GameManager manager;
    private string location = "/SaveData.txt";
    private string read;
    private bool spawn;
    private Item item;

    //Visual aid
    public Renderer visibility;
    public GameObject glow;
    private float lerp;
    private float delta;
    private Vector3 start;
    private Vector3 end;
    public Material[] itemIcons;

    //To be modified by the level design
    public int saveIndex;
    public int itemId;
    public int itemAmount;

    //Read in from save file to determine if item has been picked up, as well as create a new item from the catalog
    void Start()
    {
        manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        location = Application.streamingAssetsPath + location;
        read = File.ReadAllLines(location)[4];
        spawn = read[saveIndex] == '0';
        if (!spawn) Destroy(gameObject);

        //Get the item from the catalog using the manager
        item = manager.ParseCatalog(itemId, itemAmount);

        //Update visual aspects: Outline, Image, Bob
        visibility.material = itemIcons[item.Id];
        glow = transform.GetChild(0).gameObject;
        glow.SetActive(false);
        start = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);
        end = new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z);
    }

    private void Update()
    {
        //Bob up and down only when visible
        if (visibility.isVisible) {
            lerp = (lerp + Time.deltaTime) % 4;
            delta = Mathf.Abs(lerp - 2) / 2;
            transform.position = Vector3.Lerp(start, end, delta);
        }
    }

    public Item Pickup()
    {
        glow.SetActive(true);
        return item;
    }

    public void Disable()
    {
        glow.SetActive(false);
    }

    //Call function when picked up
    void OnDestroy()
    {
        if (spawn) manager.WriteBool(4, saveIndex, '1');
    }
}