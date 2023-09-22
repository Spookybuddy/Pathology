using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public bool typing;
    public TextNode current;
    public int options;
    public TextMeshProUGUI dialog;
    public TextMeshProUGUI[] answers;

    void Update()
    {
        if (current != null && !typing) {
            for (int i = 0; i < answers.Length; i++) answers[i].gameObject.SetActive(false);
            for (int i = 0; i < current.responses.Length; i++) {
                answers[i].text = current.responses[i];
                answers[i].gameObject.SetActive(true);
            }
            //current = current.Next();
            typing = true;
        }
        Debug.Log("End");
    }

    private IEnumerator Characters(int i)
    {
        yield return new WaitForSeconds(0.05f);
        dialog.text = dialog.text + current.text[i];
        if (i < current.text.Length) StartCoroutine(Characters(i + 1));
    }
}