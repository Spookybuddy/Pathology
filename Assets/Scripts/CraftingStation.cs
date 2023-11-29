using UnityEngine;
using System.IO;

public class CraftingStation : MonoBehaviour
{
    public int Station;
    public int mixValue;
    public GameObject itemPrefab;
    public Animator anime;
    private bool close;
    private GameObject current;
    private string file = "/Catalog.txt";
    private string[] catalog;

    //Raise item out of pot
    private bool moving;
    public Vector3 spawn;
    public Vector3 goal;

    [Header("Prodce Item of ID when Threshold value is met")]
    public int[] threshold;
    public int[] productID;

    private void Awake()
    {
        spawn += transform.position;
        goal += transform.position;
    }

    void Start()
    {
        file = Application.streamingAssetsPath + file;
        catalog = File.ReadAllLines(file);
    }

    void Update()
    {
        if (moving) {
            if (current.transform.position.y < goal.y) current.transform.position = Vector3.MoveTowards(current.transform.position, goal, Time.deltaTime * 7.5f);
            else moving = false;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        mixValue += other.GetComponent<ItemCrafting>().GetData(Station);
        Destroy(other.gameObject);
        for (int i = 0; i < threshold.Length; i++) {
            if (!close) close = (mixValue - threshold[i] < 4 && mixValue - threshold[i] > -4);
            if (anime != null) anime.SetBool("Boiling", close);

            if (mixValue == threshold[i]) {
                current = Instantiate(itemPrefab, spawn, Quaternion.identity);
                current.GetComponent<ItemCrafting>().Create(ParseCatalog(productID[i]), false, true);
                moving = true;
                mixValue = 0;
                close = false;
                anime.SetBool("Boiling", close);
            }
        }
    }

    //Parse the item data from the item catalog
    public Item ParseCatalog(int ID)
    {
        Item add = new Item();
        add.Id = ID;
        add.Quantity = 1;
        string name = "";
        for (int j = 0; j < catalog[ID].Length; j++) {
            if (char.IsWhiteSpace(catalog[ID][j])) {
                add.Category = catalog[ID][j + 1];
                int x = 0;
                string stat = "";
                for (int i = j + 3; i < catalog[ID].Length; i++) {
                    if (char.IsWhiteSpace(catalog[ID][i])) {
                        switch (x) {
                            case 0:
                                add.Vitamin = int.Parse(stat);
                                stat = "";
                                break;
                            case 1:
                                add.Mineral = int.Parse(stat);
                                stat = "";
                                break;
                            case 2:
                                add.Enzymes = int.Parse(stat);
                                break;
                        }
                        x++;
                    } else stat += catalog[ID][i];
                }
                break;
            } else name += catalog[ID][j];
        }
        add.Name = name;
        return add;
    }
}