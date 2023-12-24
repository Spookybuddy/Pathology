using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public GameManager manager;
    public AudioSource success;
    public AudioSource failure;
    public AudioSource mortar;
    public AudioSource cauldron;
    public AudioSource beaker;
    private float percent;

    void Start()
    {
        StartCoroutine(Delay());
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(0.017f);
        percent = manager.Sound();
        success.volume = 0.7f * percent;
        failure.volume = 0.7f * percent;
        mortar.volume = 0.9f * percent;
        cauldron.volume = percent;
        beaker.volume = percent;
    }
}