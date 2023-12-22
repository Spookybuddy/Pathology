using System.Collections;
using UnityEngine;
using TMPro;

public class Popup : MonoBehaviour
{
    public float time;
    public TextMeshPro text;
    private float alpha = 0;
    private Vector4 colors;

    void Update()
    {
        colors = new Vector4(1, 1, 1, alpha);
        text.color = colors;
        alpha = Mathf.Clamp01(alpha - (Time.deltaTime / time));
    }

    //Give the text number and name, and start the destruction process
    public void Create(int a, string s)
    {
        text.text = "+" + a + " " + s;
        alpha = 1;
        StartCoroutine(Delete());
    }

    //Destroy after X sec
    private IEnumerator Delete()
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}