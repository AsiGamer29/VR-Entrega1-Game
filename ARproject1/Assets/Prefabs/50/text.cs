using UnityEngine;
using System.Collections;

public class text : MonoBehaviour
{
    float deadTimer = 1.0f;
    void Start()
    {
        StartCoroutine(Die());
    }
    IEnumerator Die()
    {
        Debug.Log("timeScale = " + Time.timeScale);
        yield return new WaitForSeconds(deadTimer);
        Debug.Log("Pasó el yield");
        Destroy(gameObject);
    }
}