using UnityEngine;
using System.Collections;

public class CatMeow : MonoBehaviour
{
    public AudioSource audioSource;

    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(10f, 20f));
            audioSource.Play();
        }
    }
}